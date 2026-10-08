using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Domain;
using Molkky.Web.Infrastructure;
using Molkky.Web.Pages;

namespace Molkky.Web.Tests;

public class GameplayTests : AppTestContext
{
    private static Game Play(Game game, params int[] throws) => throws.Aggregate(game, (current, points) => current.Throw(points));

    // Ala has 48 and throws next: 2 wins.
    private static Game AlaOneThrowFromWinning() =>
        Play(Game.CreateNew(["Ala", "Bob"], GameSettings.Default), 12, 1, 1, 12, 1, 12, 1, 12, 1);

    private static IElement Button(IRenderedComponent<Gameplay> page, string text) =>
        page.FindAll("button").Single(button => button.TextContent.Trim() == text);

    // Taps a pin, or Miss for 0. The throw counts straight away.
    private static void Throw(IRenderedComponent<Gameplay> page, int points) =>
        Button(page, points == 0 ? "Miss" : points.ToString()).Click();

    private static IElement UndoButton(IRenderedComponent<Gameplay> page) => Button(page, "Undo");

    private static string CurrentPlayer(IRenderedComponent<Gameplay> page) => page.Find("h1").TextContent.Trim();

    [Fact]
    public void Showing_the_game_saves_nothing()
    {
        SaveGame(Game.CreateNew(["Ala", "Bob"], GameSettings.Default));

        var page = Render<Gameplay>();
        page.Render();

        Assert.Contains("Ala", page.Markup);
        Assert.Empty(Storage.Writes);
    }

    [Fact]
    public void Each_throw_is_saved_straight_away()
    {
        SaveGame(Game.CreateNew(["Ala", "Bob"], GameSettings.Default));
        var page = Render<Gameplay>();

        Throw(page, 7);
        page.WaitForAssertion(() => Assert.Equal([7], SavedGame()!.Throws));

        Throw(page, 0);
        page.WaitForAssertion(() => Assert.Equal([7, 0], SavedGame()!.Throws));
    }

    [Fact]
    public void The_winning_throw_is_saved_before_the_endgame_page_opens()
    {
        SaveGame(AlaOneThrowFromWinning());
        string? winnerSavedWhenLeaving = null;
        Services.GetRequiredService<NavigationManager>().LocationChanged += (_, _) => winnerSavedWhenLeaving = SavedGame()?.Winner?.Name;
        var page = Render<Gameplay>();

        Throw(page, 2);

        page.WaitForAssertion(() => Assert.EndsWith("/endgame", Uri));
        Assert.Equal("Ala", winnerSavedWhenLeaving);
    }

    [Fact]
    public void A_finished_game_opens_the_endgame_page()
    {
        SaveGame(AlaOneThrowFromWinning().Throw(2));

        Render<Gameplay>();

        Assert.EndsWith("/endgame", Uri);
    }

    [Fact]
    public void Without_a_readable_saved_game_a_new_game_starts()
    {
        Storage.Items[GameStore.Key] = "not json";

        Render<Gameplay>();

        Assert.Equal("http://localhost/", Uri);
    }

    [Fact]
    public void Without_a_saved_game_a_new_game_starts()
    {
        Render<Gameplay>();

        Assert.Equal("http://localhost/", Uri);
    }

    [Fact]
    public void Shows_whose_turn_it_is_with_their_score_and_what_they_need_to_win()
    {
        SaveGame(Play(Game.CreateNew(["Ala", "Bob"], GameSettings.Default), 12, 5));

        var page = Render<Gameplay>();

        Assert.Equal("Bob", CurrentPlayer(page));
        Assert.Contains("Round 2", page.Markup);
        Assert.Contains("To win: 45", page.Markup);
    }

    [Fact]
    public void Undo_is_disabled_until_there_is_a_throw_to_take_back()
    {
        SaveGame(Game.CreateNew(["Ala", "Bob"], GameSettings.Default));
        var page = Render<Gameplay>();

        Assert.True(UndoButton(page).HasAttribute("disabled"));

        Throw(page, 7);

        page.WaitForAssertion(() => Assert.False(UndoButton(page).HasAttribute("disabled")));
    }

    [Fact]
    public void Undo_takes_back_the_last_throw_and_is_saved_straight_away()
    {
        SaveGame(Game.CreateNew(["Ala", "Bob"], GameSettings.Default));
        var page = Render<Gameplay>();
        Throw(page, 7);
        Throw(page, 3);
        page.WaitForAssertion(() => Assert.Equal([7, 3], SavedGame()!.Throws));

        UndoButton(page).Click();

        page.WaitForAssertion(() => Assert.Equal([7], SavedGame()!.Throws));
        Assert.Equal("Bob", CurrentPlayer(page));
        Assert.Contains("Ala · 7", page.Markup);
    }

    [Fact]
    public void Shows_the_last_throw_that_undo_takes_back()
    {
        SaveGame(Game.CreateNew(["Ala", "Bob"], GameSettings.Default));
        var page = Render<Gameplay>();

        Throw(page, 9);
        page.WaitForAssertion(() => Assert.Contains("Ala · 9", page.Markup));

        Throw(page, 0);
        page.WaitForAssertion(() => Assert.Contains("Bob · Miss", page.Markup));
    }

    [Fact]
    public void Undo_goes_back_across_the_end_of_a_round()
    {
        // Ala 12 and Bob 1 end round 1; Bob, on less, throws first in round 2.
        SaveGame(Play(Game.CreateNew(["Ala", "Bob"], GameSettings.Default), 12, 1));
        var page = Render<Gameplay>();
        Assert.Contains("Round 2", page.Markup);

        UndoButton(page).Click();

        page.WaitForAssertion(() => Assert.Equal([12], SavedGame()!.Throws));
        Assert.Contains("Round 1", page.Markup);
        Assert.Equal("Bob", CurrentPlayer(page));
    }

    [Fact]
    public void Undo_brings_back_a_player_eliminated_by_the_last_throw()
    {
        // Ala misses three rounds in a row and is out; the others score 1 a throw.
        var game = Play(Game.CreateNew(["Ala", "Bob", "Cyd"], GameSettings.Default), 0, 1, 1, 0, 1, 1);
        SaveGame(game);
        var page = Render<Gameplay>();
        Throw(page, 0);
        page.WaitForAssertion(() => Assert.Equal(["Ala"], page.Rows(ScoreboardMarkup.OutOfTheGame)!.Select(row => row.Player)));

        UndoButton(page).Click();

        page.WaitForAssertion(() => Assert.Equal(game.Throws, SavedGame()!.Throws));
        Assert.Null(page.Rows(ScoreboardMarkup.OutOfTheGame));
        Assert.Equal("Ala", CurrentPlayer(page));
        Assert.Equal(new ScoreboardRow("Ala", "2", 0, Current: true), page.Rows(ScoreboardMarkup.ThrowingOrder)![0]);
    }
}

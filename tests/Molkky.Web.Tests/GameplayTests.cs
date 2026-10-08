using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Domain;
using Molkky.Web.Infrastructure;
using Molkky.Web.Pages;
using MudBlazor;

namespace Molkky.Web.Tests;

public class GameplayTests : MudBlazorTestContext
{
    private static Game Play(Game game, params int[] throws) => throws.Aggregate(game, (current, points) => current.Throw(points));

    // Ala has 48 and throws next: 2 wins.
    private static Game AlaOneThrowFromWinning() =>
        Play(Game.CreateNew(["Ala", "Bob"], GameSettings.Default), 12, 1, 1, 12, 1, 12, 1, 12, 1);

    // In the app the dialog provider sits in MainLayout; tests render it next to the page.
    private (IRenderedComponent<MudDialogProvider> Dialogs, IRenderedComponent<Gameplay> Page) RenderGameplay()
    {
        var dialogs = Render<MudDialogProvider>();
        var page = Render<Gameplay>();
        return (dialogs, page);
    }

    // Taps a score on the keyboard and confirms it.
    private static void Throw(IRenderedComponent<MudDialogProvider> dialogs, IRenderedComponent<Gameplay> page, int points)
    {
        page.FindAll("button").Single(button => button.TextContent.Trim() == points.ToString()).Click();
        dialogs.WaitForAssertion(() => Assert.Contains($"Add score {points}?", dialogs.Markup));
        dialogs.FindAll("button").Single(button => button.TextContent.Trim() == "Confirm").Click();
    }

    [Fact]
    public void Showing_the_game_saves_nothing()
    {
        SaveGame(Game.CreateNew(["Ala", "Bob"], GameSettings.Default));

        var (_, page) = RenderGameplay();
        page.Render();

        Assert.Contains("Ala", page.Markup);
        Assert.Empty(Storage.Writes);
    }

    [Fact]
    public void Each_throw_is_saved_straight_away()
    {
        SaveGame(Game.CreateNew(["Ala", "Bob"], GameSettings.Default));
        var (dialogs, page) = RenderGameplay();

        Throw(dialogs, page, 7);
        page.WaitForAssertion(() => Assert.Equal([7], SavedGame()!.Throws));

        Throw(dialogs, page, 0);
        page.WaitForAssertion(() => Assert.Equal([7, 0], SavedGame()!.Throws));
    }

    [Fact]
    public void The_winning_throw_is_saved_before_the_endgame_page_opens()
    {
        SaveGame(AlaOneThrowFromWinning());
        string? winnerSavedWhenLeaving = null;
        Services.GetRequiredService<NavigationManager>().LocationChanged += (_, _) => winnerSavedWhenLeaving = SavedGame()?.Winner?.Name;
        var (dialogs, page) = RenderGameplay();

        Throw(dialogs, page, 2);

        page.WaitForAssertion(() => Assert.EndsWith("/endgame", Uri));
        Assert.Equal("Ala", winnerSavedWhenLeaving);
    }

    [Fact]
    public void A_finished_game_opens_the_endgame_page()
    {
        SaveGame(AlaOneThrowFromWinning().Throw(2));

        RenderGameplay();

        Assert.EndsWith("/endgame", Uri);
    }

    [Fact]
    public void Without_a_readable_saved_game_a_new_game_starts()
    {
        Storage.Items[GameStore.Key] = "not json";

        RenderGameplay();

        Assert.Equal("http://localhost/", Uri);
    }
}

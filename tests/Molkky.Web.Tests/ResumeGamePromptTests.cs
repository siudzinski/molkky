using Bunit;
using Molkky.Domain;
using Molkky.Domain.Storage;
using Molkky.Web.Infrastructure;
using Molkky.Web.Shared;

namespace Molkky.Web.Tests;

public class ResumeGamePromptTests : MudBlazorTestContext
{
    // Round 1 done: Cyd (1) throws first in round 2, then Bob (3), then Ala (5).
    private static Game UnfinishedGame() =>
        Game.CreateNew(["Ala", "Bob", "Cyd"], GameSettings.Default).Throw(5).Throw(3).Throw(1);

    // Bob is the last player standing.
    private static Game FinishedGame() =>
        new[] { 0, 1, 0, 1, 0 }.Aggregate(Game.CreateNew(["Ala", "Bob"], GameSettings.Default), (game, points) => game.Throw(points));

    // In the app the prompt sits in MainLayout, so it runs once per app start.
    private IRenderedComponent<ResumeGamePrompt> RenderPrompt() => Render<ResumeGamePrompt>();

    private static void Click(IRenderedComponent<ResumeGamePrompt> prompt, string text) =>
        prompt.FindAll("button").Single(button => button.TextContent.Trim() == text).Click();

    [Fact]
    public void Offers_to_resume_a_saved_unfinished_game()
    {
        SaveGame(UnfinishedGame());

        var prompt = RenderPrompt();

        prompt.WaitForAssertion(() => Assert.Contains("Resume game?", prompt.Markup));
        Assert.Contains("Cyd, Bob, Ala", prompt.Markup);
        Assert.Contains("Round 2", prompt.Markup);
    }

    [Fact]
    public void Resume_opens_the_saved_game_as_it_was()
    {
        SaveGame(UnfinishedGame());
        var saved = Storage.Items[GameStore.Key];
        var prompt = RenderPrompt();
        prompt.WaitForAssertion(() => Assert.Contains("Resume game?", prompt.Markup));

        Click(prompt, "Resume");

        prompt.WaitForAssertion(() => Assert.EndsWith("/gameplay", Uri));
        Assert.Equal(saved, Storage.Items[GameStore.Key]);
        Assert.DoesNotContain("Resume game?", prompt.Markup);
    }

    [Fact]
    public void New_game_discards_the_saved_game()
    {
        SaveGame(UnfinishedGame());
        var prompt = RenderPrompt();
        prompt.WaitForAssertion(() => Assert.Contains("Resume game?", prompt.Markup));

        Click(prompt, "New game");

        prompt.WaitForAssertion(() => Assert.False(Storage.Items.ContainsKey(GameStore.Key)));
        Assert.Equal("http://localhost/", Uri);
    }

    [Fact]
    public void A_finished_game_is_not_offered()
    {
        SaveGame(FinishedGame());

        var prompt = RenderPrompt();

        Assert.DoesNotContain("Resume game?", prompt.Markup);
    }

    [Fact]
    public void Nothing_is_offered_without_a_saved_game()
    {
        var prompt = RenderPrompt();

        Assert.DoesNotContain("Resume game?", prompt.Markup);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("""{"version":2,"game":{}}""")]
    [InlineData("""{"Players":[],"MaximumPointsStrategy":1,"MissedThrowsStrategy":1,"NumberOfThrowsInRound":0,"RoundNumber":1}""")]
    public void Old_or_unreadable_game_data_is_not_offered(string json)
    {
        Storage.Items[GameStore.Key] = json;

        var prompt = RenderPrompt();

        Assert.DoesNotContain("Resume game?", prompt.Markup);
        Assert.Null(StorageFormat.LoadGame(json));
    }
}

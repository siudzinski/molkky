using AngleSharp.Dom;
using Bunit;
using Molkky.Domain;
using Molkky.Domain.Storage;
using Molkky.Web.Infrastructure;
using Molkky.Web.Pages;

namespace Molkky.Web.Tests;

public class NewGameTests : MudBlazorTestContext
{
    private static IElement Button(IRenderedComponent<NewGame> page, string text) =>
        page.FindAll("button").Single(button => button.TextContent.Trim() == text);

    private static void AddPlayers(IRenderedComponent<NewGame> page, params string[] names)
    {
        foreach (var name in names)
        {
            page.Find("input").Change(name);
            Button(page, "Add").Click();
        }
    }

    // The game the page saved when it was started.
    private Game StartedGame() => SavedGame()!;

    private static string[] Names(Game game) => game.Players.Select(p => p.Name).ToArray();

    [Fact]
    public void A_game_needs_at_least_two_players()
    {
        var page = Render<NewGame>();

        Assert.True(Button(page, "Start game").HasAttribute("disabled"));

        AddPlayers(page, "Ala");
        Assert.True(Button(page, "Start game").HasAttribute("disabled"));

        AddPlayers(page, "Bob");
        Assert.False(Button(page, "Start game").HasAttribute("disabled"));
    }

    [Fact]
    public void Starting_a_game_opens_the_gameplay_page_with_every_player_once()
    {
        var page = Render<NewGame>();
        AddPlayers(page, "Ala", "Bob", "Cyd");

        Button(page, "Start game").Click();

        page.WaitForAssertion(() => Assert.EndsWith("/gameplay", Uri));
        Assert.Equal(new[] { "Ala", "Bob", "Cyd" }, Names(StartedGame()).Order());
    }

    [Fact]
    public void The_starting_order_is_shuffled()
    {
        string[] names = ["Ala", "Bob", "Cyd", "Dan", "Ewa"];
        var page = Render<NewGame>();
        var orders = new List<string>();

        // 10 starts of 5 players: all in one order by chance has a probability of 120^-9.
        for (var i = 0; i < 10; i++)
        {
            AddPlayers(page, names);
            Button(page, "Start game").Click();
            page.WaitForAssertion(() => Assert.Equal(i + 1, Storage.Writes.Count(key => key == GameStore.Key)));

            var order = Names(StartedGame());
            Assert.Equal(names.Order(), order.Order());
            orders.Add(string.Join(",", order));
        }

        Assert.True(orders.Distinct().Count() > 1, $"Every start had the same order: {orders[0]}");
    }

    [Fact]
    public void A_new_game_has_the_default_settings_when_none_are_saved()
    {
        var page = Render<NewGame>();
        AddPlayers(page, "Ala", "Bob");

        Button(page, "Start game").Click();

        page.WaitForAssertion(() => Assert.EndsWith("/gameplay", Uri));
        Assert.Equal(MaximumPointsStrategies.MaxScoreInHalf, StartedGame().Settings.MaximumPoints);
        Assert.Equal(MissedThrowsStrategies.Disqualified, StartedGame().Settings.MissedThrows);
    }

    [Fact]
    public void A_new_game_has_the_saved_settings()
    {
        var settings = new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.BackToZero);
        Storage.Items[SettingsStore.Key] = StorageFormat.SaveSettings(settings);
        var page = Render<NewGame>();
        AddPlayers(page, "Ala", "Bob");

        Button(page, "Start game").Click();

        page.WaitForAssertion(() => Assert.EndsWith("/gameplay", Uri));
        Assert.Equal(MaximumPointsStrategies.BackToZero, StartedGame().Settings.MaximumPoints);
        Assert.Equal(MissedThrowsStrategies.BackToZero, StartedGame().Settings.MissedThrows);
    }
}

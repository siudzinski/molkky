using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Domain;
using Molkky.Web.Pages;

namespace Molkky.Web.Tests;

public class EndgameTests : AppTestContext
{
    // Ala scores 1 a throw, Bob 12 and Cyd misses: from round 2 on the order is Cyd, Ala, Bob. Cyd is
    // out after round 3; Bob reaches exactly 50 with the last throw of round 5.
    private static Game WonByBob() =>
        new[] { 1, 12, 0, /* round 2 */ 0, 1, 12, /* round 3 */ 0, 1, 12, /* round 4 */ 1, 12, /* round 5 */ 1, 2 }
            .Aggregate(Game.CreateNew(["Ala", "Bob", "Cyd"], new GameSettings(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.Disqualified)),
                (game, points) => game.Throw(points));

    [Fact]
    public void Shows_the_winner()
    {
        Assert.Equal(["Cyd"], WonByBob().Losers.Select(p => p.Name));
        SaveGame(WonByBob());

        var page = Render<Endgame>();

        Assert.Contains("Bob won the game!", page.Markup);
    }

    [Fact]
    public void Play_again_saves_the_new_game_in_the_finishing_order_and_opens_it()
    {
        var game = WonByBob();
        SaveGame(game);
        var page = Render<Endgame>();

        page.FindAll("button").Single(button => button.TextContent.Trim() == "Play again").Click();

        page.WaitForAssertion(() => Assert.EndsWith("/gameplay", Uri));
        var replay = SavedGame()!;
        Assert.Empty(replay.Throws);
        Assert.Equal(game.Settings, replay.Settings);
        Assert.Equal(["Cyd", "Ala", "Bob"], replay.Players.Select(p => p.Name));
    }

    [Fact]
    public void Without_a_finished_game_a_new_game_starts()
    {
        SaveGame(Game.CreateNew(["Ala", "Bob"], GameSettings.Default));

        Render<Endgame>();

        Assert.Equal("http://localhost/", Uri);
    }

    // The chart's numbers, as its table gives them to screen readers: each player's score after every throw,
    // from 0. Cyd is out after three misses, so their line stops after round 3.
    [Fact]
    public void The_chart_shows_each_players_score_after_every_throw_starting_at_0()
    {
        SaveGame(WonByBob());
        var page = Render<Endgame>();

        var rows = page.FindAll("figure table tbody tr")
            .ToDictionary(row => row.QuerySelector("th")!.TextContent.Trim(), row => row.QuerySelectorAll("td").Select(cell => cell.TextContent.Trim()).ToArray());

        Assert.Equal(["0", "12", "24", "36", "48", "50"], rows["Bob"]);
        Assert.Equal(["0", "1", "2", "3", "4", "5"], rows["Ala"]);
        Assert.Equal(["0", "0", "0", "0", "", ""], rows["Cyd"]);
        Assert.Equal(3, page.FindAll("figure svg polyline").Count);
    }

    [Fact]
    public void The_chart_draws_one_point_per_score_and_lists_the_winner_first()
    {
        SaveGame(WonByBob());
        var page = Render<Endgame>();

        var pointsPerLine = page.FindAll("figure svg polyline").Select(line => line.GetAttribute("points")!.Split(' ').Length);
        // Each legend entry: a colour dot (no text), the name and the final score.
        var legend = page.FindAll("figcaption li").Select(item => item.QuerySelectorAll("span").Select(span => span.TextContent.Trim()).Where(text => text != "").ToArray());

        Assert.Equal([6, 6, 4], pointsPerLine);
        Assert.Equal([["Bob", "50"], ["Ala", "5"], ["Cyd", "0"]], legend);
    }

    [Fact]
    public void Undo_takes_back_the_winning_throw_saves_it_and_goes_back_to_the_game()
    {
        var game = WonByBob();
        SaveGame(game);
        Game? savedWhenLeaving = null;
        Services.GetRequiredService<NavigationManager>().LocationChanged += (_, _) => savedWhenLeaving = SavedGame();
        var page = Render<Endgame>();
        Assert.Contains("Bob · 2", page.Markup);

        page.FindAll("button").Single(button => button.TextContent.Trim() == "Undo").Click();

        page.WaitForAssertion(() => Assert.EndsWith("/gameplay", Uri));
        Assert.Equal(game.Throws.SkipLast(1), savedWhenLeaving!.Throws);
        Assert.False(savedWhenLeaving.AnyWinner);
        Assert.Equal("Bob", savedWhenLeaving.CurrentPlayer.Name);
    }

    [Fact]
    public void New_game_opens_the_new_game_page()
    {
        SaveGame(WonByBob());
        var page = Render<Endgame>();

        page.FindAll("button").Single(button => button.TextContent.Trim() == "New game").Click();

        Assert.Equal("http://localhost/", Uri);
    }

    // "Change players": the sheet lists the players in the order they finished in (Cyd, Ala, Bob; colours
    // Ala 0, Bob 1, Cyd 2), each a checkbox labelled with their name.
    private static IRenderedComponent<Endgame> ChangePlayers(IRenderedComponent<Endgame> page)
    {
        page.FindAll("button").Single(button => button.TextContent.Trim() == "Change players").Click();
        return page;
    }

    private static string[] Listed(IRenderedComponent<Endgame> page) =>
        page.FindAll("[role=dialog] li span.truncate").Select(name => name.TextContent.Trim()).ToArray();

    private static IElement Checkbox(IRenderedComponent<Endgame> page, string name) =>
        page.FindAll("[role=dialog] label:has(input[type=checkbox])")
            .Single(label => label.QuerySelector("span.truncate")!.TextContent.Trim() == name)
            .QuerySelector("input[type=checkbox]")!;

    private static void AddPlayer(IRenderedComponent<Endgame> page, string name)
    {
        page.Find("[role=dialog] form input").Input(name);
        page.Find("[role=dialog] form").Submit();
    }

    private static IElement SheetButton(IRenderedComponent<Endgame> page, string text) =>
        page.FindAll("[role=dialog] button").Single(button => button.TextContent.Trim() == text);

    [Fact]
    public void Change_players_lists_everyone_in_the_order_they_finished_in_all_playing()
    {
        SaveGame(WonByBob());

        var page = ChangePlayers(Render<Endgame>());

        Assert.Equal("Who's playing?", page.Find("[role=dialog] h2").TextContent);
        Assert.Equal(["Cyd", "Ala", "Bob"], Listed(page));
        Assert.All(["Cyd", "Ala", "Bob"], name => Assert.True(Checkbox(page, name).HasAttribute("checked")));
    }

    [Fact]
    public void Start_saves_the_next_game_without_who_left_and_with_who_joined_then_opens_it()
    {
        var game = WonByBob();
        SaveGame(game);
        Game? savedWhenLeaving = null;
        Services.GetRequiredService<NavigationManager>().LocationChanged += (_, _) => savedWhenLeaving = SavedGame();
        var page = ChangePlayers(Render<Endgame>());

        Checkbox(page, "Cyd").Change(false);
        AddPlayer(page, " Dan ");
        Assert.Equal(["Cyd", "Ala", "Bob", "Dan"], Listed(page));
        SheetButton(page, "Start game").Click();

        page.WaitForAssertion(() => Assert.EndsWith("/gameplay", Uri));
        Assert.Empty(savedWhenLeaving!.Throws);
        Assert.Equal(game.Settings, savedWhenLeaving.Settings);
        // Ala and Bob in the order they finished in, Dan somewhere among them with Cyd's free colour.
        Assert.Equal(["Ala", "Bob"], savedWhenLeaving.Players.Select(p => p.Name).Where(name => name != "Dan"));
        Assert.Equal([("Ala", 0), ("Bob", 1), ("Dan", 2)], savedWhenLeaving.Players.Select(p => (p.Name, p.ColorIndex)).Order());
    }

    [Fact]
    public void A_tap_brings_back_a_player_left_out()
    {
        SaveGame(WonByBob());
        var page = ChangePlayers(Render<Endgame>());

        Checkbox(page, "Ala").Change(false);
        Assert.False(Checkbox(page, "Ala").HasAttribute("checked"));

        Checkbox(page, "Ala").Change(true);

        Assert.True(Checkbox(page, "Ala").HasAttribute("checked"));
    }

    [Theory]
    [InlineData("Cyd")]
    [InlineData("cyd")]
    public void A_name_already_on_the_list_brings_that_player_back(string name)
    {
        SaveGame(WonByBob());
        var page = ChangePlayers(Render<Endgame>());
        Checkbox(page, "Cyd").Change(false);

        AddPlayer(page, name);

        Assert.Equal(["Cyd", "Ala", "Bob"], Listed(page));
        Assert.True(Checkbox(page, "Cyd").HasAttribute("checked"));
        Assert.Equal("", page.Find("[role=dialog] form input").GetAttribute("value"));
    }

    [Fact]
    public void A_player_added_twice_is_listed_once_and_can_be_removed()
    {
        SaveGame(WonByBob());
        var page = ChangePlayers(Render<Endgame>());

        AddPlayer(page, "Dan");
        AddPlayer(page, "DAN");
        Assert.Equal(["Cyd", "Ala", "Bob", "Dan"], Listed(page));

        page.Find("[role=dialog] button[aria-label='Remove Dan']").Click();

        Assert.Equal(["Cyd", "Ala", "Bob"], Listed(page));
    }

    [Fact]
    public void Start_needs_two_players()
    {
        SaveGame(WonByBob());
        var page = ChangePlayers(Render<Endgame>());

        Checkbox(page, "Ala").Change(false);
        Checkbox(page, "Bob").Change(false);

        Assert.True(SheetButton(page, "Start game").HasAttribute("disabled"));
        Assert.Contains("Add at least 2 players to start.", page.Find("[role=dialog]").TextContent);

        AddPlayer(page, "Dan");

        Assert.False(SheetButton(page, "Start game").HasAttribute("disabled"));
    }

    [Fact]
    public void Cancel_closes_the_sheet_and_saves_nothing()
    {
        SaveGame(WonByBob());
        var page = ChangePlayers(Render<Endgame>());
        Checkbox(page, "Cyd").Change(false);

        SheetButton(page, "Cancel").Click();

        Assert.Empty(page.FindAll("[role=dialog]"));
        Assert.Empty(Storage.Writes);
        Assert.EndsWith("/", Uri);
    }

    [Fact]
    public void Showing_the_endgame_saves_nothing()
    {
        SaveGame(WonByBob());

        var page = Render<Endgame>();
        page.Render();

        Assert.Empty(Storage.Writes);
    }
}

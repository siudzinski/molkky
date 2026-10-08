using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Domain;
using Molkky.Web.Pages;

namespace Molkky.Web.Tests;

public class EndgameTests : MudBlazorTestContext
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

    [Fact]
    public void Showing_the_endgame_saves_nothing()
    {
        SaveGame(WonByBob());

        var page = Render<Endgame>();
        page.Render();

        Assert.Empty(Storage.Writes);
    }
}

using Bunit;
using Molkky.Domain;
using Molkky.Web.Shared;

namespace Molkky.Web.Tests;

public class ScoreboardTests : AppTestContext
{
    private static Game Play(Game game, params int[] throws) => throws.Aggregate(game, (current, points) => current.Throw(points));

    private IRenderedComponent<Scoreboard> RenderScoreboard(Game game) =>
        Render<Scoreboard>(parameters => parameters.Add(board => board.Game, game));

    [Fact]
    public void Lists_the_players_in_this_rounds_throwing_order_with_their_scores()
    {
        // Round 1: Ala 9, Bob 3, Cyd 12. Round 2 starts with the lowest score.
        var game = Play(Game.CreateNew(["Ala", "Bob", "Cyd"], GameSettings.Default), 9, 3, 12);

        var rows = RenderScoreboard(game).Rows(ScoreboardMarkup.ThrowingOrder)!;

        Assert.Equal([("Bob", 3), ("Ala", 9), ("Cyd", 12)], rows.Select(row => (row.Player, row.Score)));
    }

    [Fact]
    public void Marks_the_current_player_and_only_them()
    {
        // Bob has thrown in round 2; Ala is next.
        var game = Play(Game.CreateNew(["Ala", "Bob", "Cyd"], GameSettings.Default), 9, 3, 12, 5);

        var rows = RenderScoreboard(game).Rows(ScoreboardMarkup.ThrowingOrder)!;

        Assert.Equal(["Ala"], rows.Where(row => row.Current).Select(row => row.Player));
        Assert.Equal(game.CurrentPlayer.Name, rows.Single(row => row.Current).Player);
    }

    [Fact]
    public void Shows_each_players_misses_in_a_row()
    {
        // Ala misses twice, Bob once then hits (his misses are cleared), Cyd once.
        var game = Play(Game.CreateNew(["Ala", "Bob", "Cyd"], GameSettings.Default), 0, 0, 4, 0, 1, 0);

        var rows = RenderScoreboard(game).Rows(ScoreboardMarkup.ThrowingOrder)!;

        Assert.Equal([("Ala", "2"), ("Bob", ""), ("Cyd", "1")], rows.Select(row => (row.Player, row.Misses)).OrderBy(row => row.Player));
    }

    [Fact]
    public void Eliminated_players_are_listed_apart_with_their_score()
    {
        // Ala misses three times in a row and is out; Bob and Cyd play on.
        var game = Play(Game.CreateNew(["Ala", "Bob", "Cyd"], GameSettings.Default), 6, 1, 1, /* Bob, Cyd, Ala */ 1, 1, 0, 1, 1, 0, 1, 1, 0);
        Assert.Equal(["Ala"], game.Losers.Select(p => p.Name));

        var board = RenderScoreboard(game);

        Assert.Equal([new ScoreboardRow("Ala", "3", 6, Current: false)], board.Rows(ScoreboardMarkup.OutOfTheGame)!);
        Assert.DoesNotContain("Ala", board.Rows(ScoreboardMarkup.ThrowingOrder)!.Select(row => row.Player));
    }

    [Fact]
    public void Without_eliminations_there_is_no_out_of_the_game_list()
    {
        var game = Play(Game.CreateNew(["Ala", "Bob"], GameSettings.Default), 0, 0);

        var board = RenderScoreboard(game);

        Assert.Null(board.Rows(ScoreboardMarkup.OutOfTheGame));
    }

    [Fact]
    public void Each_player_has_the_colour_of_their_palette_index()
    {
        var game = Play(Game.CreateNew(["Ala", "Bob", "Cyd"], GameSettings.Default), 9, 3, 12);

        var board = RenderScoreboard(game);

        foreach (var player in game.Players)
        {
            var row = board.FindAll("tbody tr").Single(tr => tr.QuerySelector("th")!.TextContent.Trim() == player.Name);
            var avatar = row.QuerySelector("[aria-hidden=true]")!;
            Assert.Equal(player.FirstLetter, avatar.TextContent.Trim());
            Assert.Contains(PlayerColors.For(player.ColorIndex).Avatar, avatar.ClassName);
        }
    }
}

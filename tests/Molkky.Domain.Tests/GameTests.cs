using System.Text.Json;
using Molkky.Domain.StorageModels;

namespace Molkky.Domain.Tests;

public class GameTests
{
    private const MaximumPointsStrategies MaxScoreInHalf = MaximumPointsStrategies.MaxScoreInHalf;
    private const MissedThrowsStrategies Disqualified = MissedThrowsStrategies.Disqualified;

    private static Game NewGame(params string[] names) => NewGame(MaxScoreInHalf, Disqualified, names);

    private static Game NewGame(
        MaximumPointsStrategies maximumPoints,
        MissedThrowsStrategies missedThrows,
        params string[] names) =>
        Game.CreateNew(names, new GameSettings(maximumPoints, missedThrows));

    // Plays the throws in order, checking that each one is made by the expected player.
    private static Game Play(Game game, params (string Player, int Points)[] throws)
    {
        foreach (var (player, points) in throws)
        {
            Assert.Equal(player, game.CurrentPlayer.Name);
            game = game.Throw(points);
        }

        return game;
    }

    private static string[] Names(IEnumerable<Player> players) => players.Select(p => p.Name).ToArray();

    // Ala reaches exactly 50 on the fifth throw. Bob throws first from round 2 on (lower score).
    private static Game GameWonByAla() =>
        Play(NewGame("Ala", "Bob"),
            ("Ala", 12), ("Bob", 1),
            ("Bob", 1), ("Ala", 12),
            ("Bob", 1), ("Ala", 12),
            ("Bob", 1), ("Ala", 12),
            ("Bob", 1), ("Ala", 2));

    [Fact]
    public void Players_throw_in_the_order_given()
    {
        var game = NewGame("Ala", "Bob", "Cyd");

        Assert.Equal(1, game.RoundNumber);
        game = Play(game, ("Ala", 1), ("Bob", 2));

        Assert.Equal("Cyd", game.CurrentPlayer.Name);
        Assert.Equal(1, game.RoundNumber);
    }

    [Fact]
    public void A_round_ends_once_every_active_player_has_thrown()
    {
        var game = NewGame("Ala", "Bob", "Cyd");

        game = Play(game, ("Ala", 1), ("Bob", 2), ("Cyd", 3));

        Assert.Equal(2, game.RoundNumber);
    }

    [Fact]
    public void The_order_does_not_change_during_a_round()
    {
        var game = NewGame("Ala", "Bob", "Cyd");

        game = Play(game, ("Ala", 10), ("Bob", 2));

        Assert.Equal(new[] { "Ala", "Bob", "Cyd" }, Names(game.Players));
    }

    // House rule (not the official fixed order): after each round the lowest score throws first.
    [Fact]
    public void After_each_round_players_are_re_sorted_lowest_score_first()
    {
        var game = NewGame("Ala", "Bob", "Cyd");

        game = Play(game, ("Ala", 10), ("Bob", 2), ("Cyd", 5));

        Assert.Equal(new[] { "Bob", "Cyd", "Ala" }, Names(game.Players));
        Assert.Equal("Bob", game.CurrentPlayer.Name);

        game = Play(game, ("Bob", 12), ("Cyd", 1), ("Ala", 1));

        Assert.Equal(new[] { "Cyd", "Ala", "Bob" }, Names(game.Players));
    }

    [Fact]
    public void Players_with_equal_scores_keep_their_previous_order()
    {
        var game = NewGame("Ala", "Bob", "Cyd");

        game = Play(game, ("Ala", 5), ("Bob", 5), ("Cyd", 1));

        Assert.Equal(new[] { "Cyd", "Ala", "Bob" }, Names(game.Players));
    }

    [Fact]
    public void Scoring_exactly_50_wins_the_game()
    {
        var game = GameWonByAla();

        Assert.True(game.AnyWinner);
        Assert.Equal("Ala", game.Winner?.Name);
    }

    [Fact]
    public void Going_over_50_does_not_win()
    {
        var game = Play(NewGame("Ala", "Bob"),
            ("Ala", 12), ("Bob", 1),
            ("Bob", 1), ("Ala", 12),
            ("Bob", 1), ("Ala", 12),
            ("Bob", 1), ("Ala", 12),
            ("Bob", 1), ("Ala", 5));

        Assert.False(game.AnyWinner);
        Assert.Equal(25, game.Players.Single(p => p.Name == "Ala").Score);
    }

    [Fact]
    public void Throws_after_the_game_is_won_are_ignored()
    {
        var game = GameWonByAla();
        var scoresBefore = game.Players.Select(p => p.Score).ToArray();

        var afterTheWin = game.Throw(12);

        Assert.Equal(scoresBefore, afterTheWin.Players.Select(p => p.Score).ToArray());
        Assert.Equal("Ala", afterTheWin.Winner?.Name);
        Assert.Equal(game.Throws, afterTheWin.Throws);
    }

    [Fact]
    public void A_disqualified_player_moves_to_the_losers_and_is_skipped()
    {
        var game = NewGame("Ala", "Bob", "Cyd");

        game = Play(game,
            ("Ala", 0), ("Bob", 1), ("Cyd", 2),
            ("Ala", 0), ("Bob", 1), ("Cyd", 2),
            ("Ala", 0));

        Assert.Equal(new[] { "Ala" }, Names(game.Losers));
        Assert.Equal(new[] { "Bob", "Cyd" }, Names(game.Players));
        Assert.False(game.AnyWinner);

        game = Play(game, ("Bob", 1), ("Cyd", 2));

        Assert.Equal(4, game.RoundNumber);
        Assert.Equal("Bob", game.CurrentPlayer.Name);
    }

    [Fact]
    public void Eliminating_the_last_thrower_of_a_round_starts_the_next_round()
    {
        var game = NewGame("Ala", "Bob", "Cyd");

        game = Play(game,
            ("Ala", 1), ("Bob", 2), ("Cyd", 10),
            ("Ala", 1), ("Bob", 2), ("Cyd", 0),
            ("Ala", 1), ("Bob", 2), ("Cyd", 0),
            ("Ala", 1), ("Bob", 2), ("Cyd", 0));

        Assert.Equal(new[] { "Cyd" }, Names(game.Losers));
        Assert.Equal(5, game.RoundNumber);
        Assert.Equal("Ala", game.CurrentPlayer.Name);
    }

    [Fact]
    public void The_last_player_standing_wins()
    {
        var game = NewGame("Ala", "Bob");

        game = Play(game, ("Ala", 0), ("Bob", 1), ("Ala", 0), ("Bob", 1), ("Ala", 0));

        Assert.True(game.AnyWinner);
        Assert.Equal("Bob", game.Winner?.Name);
    }

    [Fact]
    public void Nobody_is_eliminated_when_three_misses_mean_back_to_zero()
    {
        var game = NewGame(MaxScoreInHalf, MissedThrowsStrategies.BackToZero, "Ala", "Bob");

        game = Play(game, ("Ala", 0), ("Bob", 1), ("Ala", 0), ("Bob", 1), ("Ala", 0), ("Bob", 1));

        Assert.Empty(game.Losers);
        Assert.Equal(2, game.Players.Count);
        Assert.False(game.AnyWinner);
    }

    [Fact]
    public void Play_again_starts_over_with_the_same_players_order_and_settings()
    {
        var game = NewGame(MaxScoreInHalf, Disqualified, "Ala", "Bob");
        game = Play(game, ("Ala", 0), ("Bob", 1), ("Ala", 0), ("Bob", 1), ("Ala", 0));
        var orderAtTheEnd = Names(game.AllPlayers);

        // The replayed game is right straight away: no save and reload needed to bring the losers back.
        var replay = game.PlayAgain();

        Assert.False(replay.AnyWinner);
        Assert.Equal(1, replay.RoundNumber);
        Assert.Empty(replay.Losers);
        Assert.Equal(orderAtTheEnd, Names(replay.Players));
        Assert.All(replay.Players, player =>
        {
            Assert.Equal(0, player.Score);
            Assert.Equal(0, player.NumberOfFailedThrows);
            Assert.Empty(player.ScoreHistory);
        });
        Assert.Equal(MaxScoreInHalf, replay.Settings.MaximumPoints);
        Assert.Equal(Disqualified, replay.Settings.MissedThrows);
    }

    [Fact]
    public void A_saved_game_loads_back_mid_round()
    {
        var game = NewGame("Ala", "Bob", "Cyd");
        game = Play(game, ("Ala", 0), ("Bob", 1), ("Cyd", 2), ("Ala", 0), ("Bob", 5));

        // Same serialisation as GameSessionStorage in the web app.
        var json = JsonSerializer.Serialize(game.ToGameState());
        var state = JsonSerializer.Deserialize<GameState>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var loaded = Game.FromGameState(state!);

        Assert.Equal(2, loaded.RoundNumber);
        Assert.Equal("Cyd", loaded.CurrentPlayer.Name);
        Assert.Equal(Names(game.Players), Names(loaded.Players));
        foreach (var (before, after) in game.Players.Zip(loaded.Players))
        {
            Assert.Equal(before.Score, after.Score);
            Assert.Equal(before.NumberOfFailedThrows, after.NumberOfFailedThrows);
            Assert.Equal(before.ScoreHistory, after.ScoreHistory);
            Assert.Equal(before.ColorIndex, after.ColorIndex);
        }
    }

    [Fact]
    public void Endgame_stats_have_each_players_score_after_every_throw_starting_from_0()
    {
        var stats = GameWonByAla().ToStats();

        Assert.Equal("Ala", stats.Winner);
        Assert.Equal(new double[] { 0, 12, 24, 36, 48, 50 }, stats.Players.Single(p => p.Name == "Ala").ScoreHistory);
        Assert.Equal(new double[] { 0, 1, 2, 3, 4, 5 }, stats.Players.Single(p => p.Name == "Bob").ScoreHistory);
        Assert.Equal(new[] { "0", "1", "2", "3", "4", "5" }, stats.Rounds);
    }

    [Fact]
    public void Endgame_stats_need_a_winner()
    {
        var game = NewGame("Ala", "Bob");

        Assert.Throws<InvalidOperationException>(game.ToStats);
    }

    [Fact]
    public void Starting_a_game_shuffles_the_players()
    {
        string[] names = ["Ala", "Bob", "Cyd", "Dan", "Ewa"];
        var settings = new GameSettings(MaxScoreInHalf, Disqualified);

        var orders = Enumerable.Range(0, 20)
            .Select(seed => Names(Game.Start(names, settings, new Random(seed)).Players))
            .ToList();

        Assert.All(orders, order => Assert.Equal(names, order.Order()));
        Assert.True(orders.Select(order => string.Join(",", order)).Distinct().Count() > 1);
    }

    [Fact]
    public void The_starting_order_comes_from_the_given_randomness()
    {
        string[] names = ["Ala", "Bob", "Cyd", "Dan", "Ewa"];
        var settings = new GameSettings(MaxScoreInHalf, Disqualified);

        var first = Game.Start(names, settings, new Random(42));
        var second = Game.Start(names, settings, new Random(42));

        Assert.Equal(Names(first.Players), Names(second.Players));
        Assert.Equal(settings, first.Settings);
        Assert.Empty(first.Throws);
    }

    [Fact]
    public void Players_get_a_colour_by_seat_in_the_starting_order()
    {
        var game = NewGame("Ala", "Bob", "Cyd", "Dan", "Ewa", "Fra");

        Assert.Equal([0, 1, 2, 3, 4, 5], game.Players.Select(p => p.ColorIndex));
    }

    [Fact]
    public void Players_keep_their_colour_when_the_order_changes_and_on_play_again()
    {
        var game = Play(NewGame("Ala", "Bob", "Cyd"), ("Ala", 10), ("Bob", 2), ("Cyd", 5));

        Assert.Equal([("Bob", 1), ("Cyd", 2), ("Ala", 0)], game.Players.Select(p => (p.Name, p.ColorIndex)));
        Assert.Equal([("Bob", 1), ("Cyd", 2), ("Ala", 0)], game.PlayAgain().Players.Select(p => (p.Name, p.ColorIndex)));
    }

    [Theory]
    [InlineData]
    [InlineData("Ala")]
    public void A_game_needs_at_least_two_players(params string[] names)
    {
        Assert.Throws<ArgumentException>(() => NewGame(names));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Every_player_needs_a_name(string name)
    {
        Assert.Throws<ArgumentException>(() => NewGame("Ala", name));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(13)]
    public void A_throw_scores_0_to_12(int points)
    {
        var game = NewGame("Ala", "Bob");

        Assert.Throws<ArgumentOutOfRangeException>(() => game.Throw(points));
        Assert.Equal(0, game.Throw(0).Players.Single(p => p.Name == "Ala").Score);
        Assert.Equal(12, game.Throw(12).Players.Single(p => p.Name == "Ala").Score);
    }
}

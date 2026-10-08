using Molkky.Domain.Rules;

namespace Molkky.Domain.Tests;

public class RulesTests
{
    private static readonly GameRules Defaults =
        GameRules.For(new GameSettings(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.Disqualified));

    private static Player PlayerWith(string name, params int[] throws) =>
        throws.Aggregate(Player.CreateNew(name), (player, points) => player.Throw(points, Defaults));

    private static string[] Names(IEnumerable<Player> players) => players.Select(p => p.Name).ToArray();

    [Theory]
    [InlineData(MaximumPointsStrategies.MaxScoreInHalf, typeof(MaxScoreInHalf))]
    [InlineData(MaximumPointsStrategies.BackToZero, typeof(MaxScoreBackToZero))]
    public void Each_maximum_points_setting_has_its_rule(MaximumPointsStrategies setting, Type rule)
    {
        var rules = GameRules.For(new GameSettings(setting, MissedThrowsStrategies.Disqualified));

        Assert.IsType(rule, rules.MaximumPoints);
    }

    [Theory]
    [InlineData(MissedThrowsStrategies.Disqualified, typeof(ThreeMissesDisqualify))]
    [InlineData(MissedThrowsStrategies.BackToZero, typeof(ThreeMissesBackToZero))]
    public void Each_missed_throws_setting_has_its_rule(MissedThrowsStrategies setting, Type rule)
    {
        var rules = GameRules.For(new GameSettings(MaximumPointsStrategies.MaxScoreInHalf, setting));

        Assert.IsType(rule, rules.MissedThrows);
    }

    [Fact]
    public void An_unknown_setting_has_no_rule()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GameRules.For(new GameSettings((MaximumPointsStrategies)0, MissedThrowsStrategies.Disqualified)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GameRules.For(new GameSettings(MaximumPointsStrategies.MaxScoreInHalf, (MissedThrowsStrategies)3)));
    }

    [Theory]
    [InlineData(51)]
    [InlineData(62)]
    public void Max_score_in_half_drops_a_score_over_50_to_25(int score)
    {
        Assert.Equal(25, new MaxScoreInHalf().ScoreAfterGoingOver(score));
    }

    [Theory]
    [InlineData(51)]
    [InlineData(62)]
    public void Max_score_back_to_zero_drops_a_score_over_50_to_0(int score)
    {
        Assert.Equal(0, new MaxScoreBackToZero().ScoreAfterGoingOver(score));
    }

    [Fact]
    public void Three_misses_disqualify_keeps_the_score_and_the_three_misses_that_put_the_player_out()
    {
        Assert.Equal((20, 3), new ThreeMissesDisqualify().AfterThirdMiss(20));
    }

    [Fact]
    public void Three_misses_back_to_zero_resets_the_score_and_the_misses()
    {
        Assert.Equal((0, 0), new ThreeMissesBackToZero().AfterThirdMiss(20));
    }

    [Fact]
    public void Every_game_re_sorts_by_lowest_score_first()
    {
        Assert.IsType<LowestScoreThrowsFirst>(Defaults.RoundOrder);
    }

    [Fact]
    public void Lowest_score_throws_first_in_the_next_round()
    {
        Player[] players = [PlayerWith("Ala", 10), PlayerWith("Bob", 2), PlayerWith("Cyd", 5)];

        Assert.Equal(new[] { "Bob", "Cyd", "Ala" }, Names(new LowestScoreThrowsFirst().OrderForNextRound(players)));
    }

    [Fact]
    public void Lowest_score_throws_first_keeps_the_order_of_equal_scores()
    {
        Player[] players = [PlayerWith("Ala", 5), PlayerWith("Bob", 5), PlayerWith("Cyd", 1), PlayerWith("Dan", 5)];

        Assert.Equal(new[] { "Cyd", "Ala", "Bob", "Dan" }, Names(new LowestScoreThrowsFirst().OrderForNextRound(players)));
    }

    [Fact]
    public void Lowest_score_throws_first_sorts_eliminated_players_by_their_last_score_too()
    {
        Player[] players = [PlayerWith("Ala", 10), PlayerWith("Bob", 6, 0, 0, 0), PlayerWith("Cyd", 2)];
        Assert.False(players[1].CanPlay);

        Assert.Equal(new[] { "Cyd", "Bob", "Ala" }, Names(new LowestScoreThrowsFirst().OrderForNextRound(players)));
    }
}

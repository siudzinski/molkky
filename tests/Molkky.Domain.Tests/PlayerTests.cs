namespace Molkky.Domain.Tests;

public class PlayerTests
{
    private const MaximumPointsStrategies MaxScoreInHalf = MaximumPointsStrategies.MaxScoreInHalf;
    private const MissedThrowsStrategies Disqualified = MissedThrowsStrategies.Disqualified;

    private static Player Throws(
        MaximumPointsStrategies maximumPoints,
        MissedThrowsStrategies missedThrows,
        params int[] throws)
    {
        var player = Player.CreateNew("ala");
        foreach (var points in throws)
        {
            player = player.Throw(points, new GameSettings(maximumPoints, missedThrows));
        }

        return player;
    }

    // The app's default settings.
    private static Player Throws(params int[] throws) => Throws(MaxScoreInHalf, Disqualified, throws);

    [Fact]
    public void A_new_player_has_no_score_and_no_misses()
    {
        var player = Player.CreateNew("ala");

        Assert.Equal("ala", player.Name);
        Assert.Equal("A", player.FirstLetter);
        Assert.Equal(0, player.Score);
        Assert.Equal(0, player.NumberOfFailedThrows);
        Assert.False(player.InDanger);
        Assert.True(player.CanPlay);
        Assert.Empty(player.ScoreHistory);
    }

    [Fact]
    public void A_hit_adds_its_points_to_the_score()
    {
        var player = Throws(7, 12);

        Assert.Equal(19, player.Score);
        Assert.Equal(new[] { 7, 19 }, player.ScoreHistory);
    }

    [Fact]
    public void Exactly_50_points_is_kept()
    {
        var player = Throws(12, 12, 12, 12, 2);

        Assert.Equal(50, player.Score);
    }

    [Theory]
    [InlineData(MaximumPointsStrategies.MaxScoreInHalf, 25)]
    [InlineData(MaximumPointsStrategies.BackToZero, 0)]
    public void Going_over_50_drops_the_score_and_play_continues_from_there(
        MaximumPointsStrategies maximumPoints, int droppedTo)
    {
        var player = Throws(maximumPoints, Disqualified, 12, 12, 12, 12, 5);

        Assert.Equal(droppedTo, player.Score);
        Assert.Equal(new[] { 12, 24, 36, 48, droppedTo }, player.ScoreHistory);

        player = player.Throw(10, new GameSettings(maximumPoints, Disqualified));

        Assert.Equal(droppedTo + 10, player.Score);
        Assert.Equal(droppedTo + 10, player.ScoreHistory[^1]);
    }

    [Theory]
    [InlineData(MissedThrowsStrategies.Disqualified)]
    [InlineData(MissedThrowsStrategies.BackToZero)]
    public void A_miss_counts_as_a_failed_throw_and_keeps_the_score(MissedThrowsStrategies missedThrows)
    {
        var player = Throws(MaxScoreInHalf, missedThrows, 5, 0);

        Assert.Equal(5, player.Score);
        Assert.Equal(1, player.NumberOfFailedThrows);
        Assert.True(player.InDanger);
        Assert.True(player.CanPlay);
        Assert.Equal(new[] { 5, 5 }, player.ScoreHistory);
    }

    [Theory]
    [InlineData(MissedThrowsStrategies.Disqualified)]
    [InlineData(MissedThrowsStrategies.BackToZero)]
    public void Only_misses_in_a_row_count_because_a_hit_clears_them(MissedThrowsStrategies missedThrows)
    {
        var player = Throws(MaxScoreInHalf, missedThrows, 0, 0, 3);

        Assert.Equal(0, player.NumberOfFailedThrows);
        Assert.False(player.InDanger);

        player = player.Throw(0, new GameSettings(MaxScoreInHalf, missedThrows));
        player = player.Throw(0, new GameSettings(MaxScoreInHalf, missedThrows));

        Assert.Equal(2, player.NumberOfFailedThrows);
        Assert.True(player.CanPlay);
        Assert.Equal(3, player.Score);
    }

    [Fact]
    public void Three_misses_in_a_row_eliminate_the_player_when_set_to_disqualified()
    {
        var player = Throws(MaxScoreInHalf, Disqualified, 20, 0, 0, 0);

        Assert.False(player.CanPlay);
        Assert.Equal(3, player.NumberOfFailedThrows);
        Assert.Equal(20, player.Score);
    }

    [Fact]
    public void An_eliminated_player_ignores_further_throws()
    {
        var player = Throws(MaxScoreInHalf, Disqualified, 20, 0, 0, 0);

        player = player.Throw(5, new GameSettings(MaxScoreInHalf, Disqualified));

        Assert.Equal(20, player.Score);
        Assert.Equal(new[] { 20, 20, 20, 20 }, player.ScoreHistory);
    }

    [Fact]
    public void Three_misses_in_a_row_reset_the_score_to_0_when_set_to_back_to_zero()
    {
        var player = Throws(MaxScoreInHalf, MissedThrowsStrategies.BackToZero, 20, 0, 0, 0);

        Assert.True(player.CanPlay);
        Assert.Equal(0, player.Score);
        Assert.Equal(0, player.NumberOfFailedThrows);
        Assert.False(player.InDanger);
    }

    // The endgame chart is drawn from the score history, so it must follow the reset to 0.
    [Fact]
    public void Three_misses_in_a_row_record_0_in_the_score_history_when_set_to_back_to_zero()
    {
        var player = Throws(MaxScoreInHalf, MissedThrowsStrategies.BackToZero, 20, 0, 0, 0, 5);

        Assert.Equal(new[] { 20, 20, 20, 0, 5 }, player.ScoreHistory);
    }

    // Was Player.Reset; a player is reset by starting the game again. Ala scores 12 (a game throw is
    // at most 12) and is eliminated, while Bob and Cyd keep the game going.
    [Fact]
    public void Reset_clears_score_misses_and_history()
    {
        var game = Game.CreateNew(["ala", "bob", "cyd"], new GameSettings(MaxScoreInHalf, Disqualified));
        // From round 2 on, ala has the highest score and throws last.
        foreach (var points in new[] { 12, 1, 1, /* round 2 */ 1, 1, 0, /* round 3 */ 1, 1, 0, /* round 4 */ 1, 1, 0 })
        {
            game = game.Throw(points);
        }
        Assert.Equal(["ala"], game.Losers.Select(p => p.Name));

        var player = game.PlayAgain().AllPlayers.Single(p => p.Name == "ala");

        Assert.Equal(0, player.Score);
        Assert.Equal(0, player.NumberOfFailedThrows);
        Assert.True(player.CanPlay);
        Assert.Empty(player.ScoreHistory);
    }
}

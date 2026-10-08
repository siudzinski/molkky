namespace Molkky.Domain.Tests;

public class UndoTests
{
    private const MaximumPointsStrategies MaxScoreInHalf = MaximumPointsStrategies.MaxScoreInHalf;
    private const MissedThrowsStrategies Disqualified = MissedThrowsStrategies.Disqualified;

    private static Game NewGame(params string[] names) => NewGame(MaxScoreInHalf, Disqualified, names);

    private static Game NewGame(
        MaximumPointsStrategies maximumPoints,
        MissedThrowsStrategies missedThrows,
        params string[] names) =>
        Game.CreateNew(names, new GameSettings(maximumPoints, missedThrows));

    private static Game Play(Game game, params int[] throws) => throws.Aggregate(game, (current, points) => current.Throw(points));

    private static string[] Names(IEnumerable<Player> players) => players.Select(p => p.Name).ToArray();

    private static Player Get(Game game, string name) => game.AllPlayers.Single(p => p.Name == name);

    // Everything the game shows, to compare two games.
    private static string State(Game game) =>
        $"round {game.RoundNumber}, {game.CurrentPlayer.Name} to throw, winner {game.Winner?.Name ?? "none"}, " +
        string.Join(" ", game.AllPlayers.Select(p =>
            $"{p.Name}:{p.Score}:{p.NumberOfFailedThrows}:{(p.CanPlay ? "in" : "out")}:[{string.Join(",", p.ScoreHistory)}]")) +
        $", throws [{string.Join(",", game.Throws)}]";

    [Fact]
    public void Undo_drops_the_last_throw()
    {
        var game = Play(NewGame("Ala", "Bob", "Cyd"), 5, 3);

        var undone = game.Undo();

        Assert.Equal([5], undone.Throws);
        Assert.Equal("Bob", undone.CurrentPlayer.Name);
        Assert.Equal(0, Get(undone, "Bob").Score);
        Assert.Empty(Get(undone, "Bob").ScoreHistory);
        Assert.Equal(5, Get(undone, "Ala").Score);
    }

    [Fact]
    public void Undoing_the_last_throw_of_a_round_restores_the_round_and_the_order()
    {
        var game = Play(NewGame("Ala", "Bob", "Cyd"), 10, 2, 5);
        Assert.Equal(new[] { "Bob", "Cyd", "Ala" }, Names(game.Players));

        var undone = game.Undo();

        Assert.Equal(1, undone.RoundNumber);
        Assert.Equal(new[] { "Ala", "Bob", "Cyd" }, Names(undone.Players));
        Assert.Equal("Cyd", undone.CurrentPlayer.Name);
    }

    [Fact]
    public void Undoing_an_elimination_brings_the_player_back()
    {
        var game = Play(NewGame("Ala", "Bob", "Cyd"), 0, 1, 2, 0, 1, 2, 0);
        Assert.Equal(new[] { "Ala" }, Names(game.Losers));

        var undone = game.Undo();

        Assert.Empty(undone.Losers);
        Assert.Equal("Ala", undone.CurrentPlayer.Name);
        Assert.Equal(2, Get(undone, "Ala").NumberOfFailedThrows);
    }

    [Fact]
    public void Undoing_the_winning_throw_continues_the_game()
    {
        // Ala reaches exactly 50 with the last throw.
        var game = Play(NewGame("Ala", "Bob"), 12, 1, 1, 12, 1, 12, 1, 12, 1, 2);
        Assert.Equal("Ala", game.Winner?.Name);

        var undone = game.Undo();

        Assert.False(undone.AnyWinner);
        Assert.Equal(5, undone.RoundNumber);
        Assert.Equal("Ala", undone.CurrentPlayer.Name);
        Assert.Equal(48, Get(undone, "Ala").Score);
    }

    [Fact]
    public void Undoing_the_third_miss_gives_back_the_score_it_reset()
    {
        // Ala throws last from round 2 on and misses three times.
        var game = Play(NewGame(MaxScoreInHalf, MissedThrowsStrategies.BackToZero, "Ala", "Bob"), 10, 1, 1, 0, 1, 0, 1, 0);
        Assert.Equal(0, Get(game, "Ala").Score);

        var undone = game.Undo();

        Assert.Equal(10, Get(undone, "Ala").Score);
        Assert.Equal(2, Get(undone, "Ala").NumberOfFailedThrows);
        Assert.Equal(new[] { 10, 10, 10 }, Get(undone, "Ala").ScoreHistory);
    }

    [Fact]
    public void There_is_nothing_to_undo_before_the_first_throw()
    {
        var game = NewGame("Ala", "Bob");

        Assert.False(game.CanUndo);
        Assert.Same(game, game.Undo());
        Assert.True(game.Throw(0).CanUndo);
    }

    [Theory]
    [InlineData(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.Disqualified)]
    [InlineData(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.BackToZero)]
    [InlineData(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.Disqualified)]
    [InlineData(MaximumPointsStrategies.BackToZero, MissedThrowsStrategies.BackToZero)]
    public void Undoing_any_throw_gives_the_game_from_before_it(
        MaximumPointsStrategies maximumPoints, MissedThrowsStrategies missedThrows)
    {
        var random = new Random(7);
        for (var i = 0; i < 50; i++)
        {
            var game = NewGame(maximumPoints, missedThrows, "Ala", "Bob", "Cyd", "Dan");
            while (!game.AnyWinner && game.Throws.Count < 200)
            {
                var next = game.Throw(random.Next(3) == 0 ? 0 : random.Next(1, 13));

                Assert.Equal(State(game), State(next.Undo()));
                game = next;
            }
        }
    }
}

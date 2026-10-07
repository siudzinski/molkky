using System.Collections.Immutable;
using Molkky.Domain.StorageModels;

namespace Molkky.Domain;

// A game is its settings, the players in their starting order and the throws so far. Everything
// else (scores, misses, eliminations, round, current player, winner, chart data) is computed by
// replaying the throws, so a game never holds state that could disagree with its throws.
// Immutable: a throw, an undo or a new round returns a new Game.
public sealed class Game
{
    public const int PointsToWin = 50;
    public const int MaxPointsPerThrow = 12;

    private readonly IReadOnlyList<Seat> _seats;
    private readonly ImmutableList<int> _throws;

    public GameSettings Settings { get; }
    public IReadOnlyList<int> Throws => _throws;

    // Everyone, eliminated players included, in the current order.
    public IReadOnlyList<Player> AllPlayers { get; }
    // The players still in the game, in throwing order.
    public IReadOnlyList<Player> Players { get; }
    public IReadOnlyList<Player> Losers { get; }
    public Player CurrentPlayer { get; }
    public Player? Winner { get; }
    public bool AnyWinner => Winner is not null;
    public int RoundNumber { get; }

    private Game(GameSettings settings, IReadOnlyList<Seat> seats, ImmutableList<int> throws)
    {
        if (seats.Count < 2) throw new ArgumentException("A game needs at least 2 players.", nameof(seats));
        if (seats.Any(seat => string.IsNullOrWhiteSpace(seat.Name))) throw new ArgumentException("Every player needs a name.", nameof(seats));

        Settings = settings;
        _seats = seats;
        _throws = throws;

        var order = seats.Select(Player.AtStart).ToList();
        var throwsInRound = 0;
        var round = 1;

        foreach (var points in throws)
        {
            CheckPoints(points);
            var active = order.Where(player => player.CanPlay).ToList();
            if (WinnerAmong(active) is not null) throw new ArgumentException("A throw after the game was won.", nameof(throws));

            var thrower = active[throwsInRound];
            var afterThrow = thrower.Throw(points, settings);
            order[order.IndexOf(thrower)] = afterThrow;

            // An eliminated thrower leaves the list, so the next player moves into their place.
            if (afterThrow.CanPlay)
            {
                throwsInRound++;
            }

            if (throwsInRound == order.Count(player => player.CanPlay))
            {
                throwsInRound = 0;
                order = order.OrderBy(player => player.Score).ToList();
                round++;
            }
        }

        AllPlayers = order;
        Players = order.Where(player => player.CanPlay).ToList();
        Losers = order.Where(player => !player.CanPlay).ToList();
        CurrentPlayer = Players[throwsInRound];
        Winner = WinnerAmong(Players);
        RoundNumber = round;
    }

    // Players throw in the given order.
    public static Game CreateNew(IEnumerable<string> names, GameSettings settings) =>
        new(settings, names.Select(name => new Seat(name, ColorProvider.Instance.GetNextColor())).ToList(), []);

    // The current player's throw: 0 is a miss, 1-12 a hit. Throws after the game is won are ignored.
    public Game Throw(int points)
    {
        CheckPoints(points);
        return AnyWinner ? this : new Game(Settings, _seats, _throws.Add(points));
    }

    public bool CanUndo => !_throws.IsEmpty;

    // Drops the last throw; the replay puts back everything it changed.
    public Game Undo() => CanUndo ? new Game(Settings, _seats, _throws.RemoveAt(_throws.Count - 1)) : this;

    // Same players and settings, starting in the order they finished in; eliminated players come back.
    public Game PlayAgain() => new(Settings, AllPlayers.Select(player => player.Seat).ToList(), []);

    public static Game FromGameState(GameState gameState) =>
        new(
            new GameSettings(gameState.MaximumPointsStrategy, gameState.MissedThrowsStrategy),
            gameState.Players.Select(player => new Seat(player.Name, player.AvatarColor)).ToList(),
            [.. gameState.Throws]);

    public GameState ToGameState() =>
        new(
            _seats.Select(seat => new PlayerState(seat.Name, seat.AvatarColor)),
            Settings.MaximumPoints,
            Settings.MissedThrows,
            [.. _throws]);

    public Stats ToStats()
    {
        if (Winner is null)
        {
            throw new InvalidOperationException("The game has no winner yet.");
        }

        var players = AllPlayers.Select(p => new PlayerStats(p.Name, p.ScoreHistory));

        return new Stats(Winner.Name, players, RoundNumber);
    }

    private static Player? WinnerAmong(IReadOnlyList<Player> active) =>
        active.FirstOrDefault(player => player.Score == PointsToWin) ?? (active.Count == 1 ? active[0] : null);

    private static void CheckPoints(int points)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(points);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(points, MaxPointsPerThrow);
    }
}

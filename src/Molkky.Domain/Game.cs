using System.Collections.Immutable;
using Molkky.Domain.Rules;

namespace Molkky.Domain;

// A game is its settings, the players in their starting order and the throws so far. Everything
// else (scores, misses, eliminations, round, current player, winner, chart data) is computed by
// replaying the throws, so a game never holds state that could disagree with its throws.
// Immutable: a throw, an undo or playing again returns a new Game.
public sealed class Game
{
    public const int PointsToWin = 50;
    public const int MaxPointsPerThrow = 12;

    private readonly IReadOnlyList<Seat> _seats;
    private readonly ImmutableList<int> _throws;

    public GameSettings Settings { get; }
    public IReadOnlyList<int> Throws => _throws;
    // The players in their starting order.
    internal IReadOnlyList<Seat> Seats => _seats;

    // Everyone, eliminated players included, in the current order.
    public IReadOnlyList<Player> AllPlayers { get; }
    // The players still in the game, in throwing order.
    public IReadOnlyList<Player> Players { get; }
    public IReadOnlyList<Player> Losers { get; }
    public Player CurrentPlayer { get; }
    public Player? Winner { get; }
    public bool AnyWinner => Winner is not null;
    public int RoundNumber { get; }
    public bool CanUndo => !_throws.IsEmpty;

    private Game(GameSettings settings, IReadOnlyList<Seat> seats, ImmutableList<int> throws)
    {
        if (seats.Count < 2) throw new ArgumentException("A game needs at least 2 players.", nameof(seats));
        if (seats.Any(seat => string.IsNullOrWhiteSpace(seat.Name))) throw new ArgumentException("Every player needs a name.", nameof(seats));

        Settings = settings;
        _seats = seats;
        _throws = throws;
        var rules = GameRules.For(settings);

        var order = seats.Select(Player.AtStart).ToList();
        var throwsInRound = 0;
        var round = 1;

        foreach (var points in throws)
        {
            CheckPoints(points);
            var active = order.Where(player => player.CanPlay).ToList();
            if (WinnerAmong(active) is not null) throw new ArgumentException("A throw after the game was won.", nameof(throws));

            var thrower = active[throwsInRound];
            var afterThrow = thrower.Throw(points, rules);
            order[order.IndexOf(thrower)] = afterThrow;

            // An eliminated thrower leaves the list, so the next player moves into their place.
            if (afterThrow.CanPlay)
            {
                throwsInRound++;
            }

            if (throwsInRound == order.Count(player => player.CanPlay))
            {
                throwsInRound = 0;
                order = rules.RoundOrder.OrderForNextRound(order);
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

    // A new game: the starting order is shuffled with the given randomness.
    public static Game Start(IEnumerable<string> names, GameSettings settings, Random random)
    {
        var order = names.ToArray();
        random.Shuffle(order);
        return CreateNew(order, settings);
    }

    // Players throw in the given order, and get their colour by seat.
    public static Game CreateNew(IEnumerable<string> names, GameSettings settings) =>
        new(settings, names.Select((name, seat) => new Seat(name, seat)).ToList(), []);

    // The current player's throw: 0 is a miss, 1-12 a hit. Throws after the game is won are ignored.
    public Game Throw(int points)
    {
        CheckPoints(points);
        return AnyWinner ? this : new Game(Settings, _seats, _throws.Add(points));
    }

    // Drops the last throw; the replay puts back everything it changed.
    public Game Undo() => CanUndo ? new Game(Settings, _seats, _throws.RemoveAt(_throws.Count - 1)) : this;

    // Same players and settings, starting in the order they finished in; eliminated players come back.
    public Game PlayAgain() => new(Settings, AllPlayers.Select(player => player.Seat).ToList(), []);

    // The same settings with changed players: those staying (players of this game, eliminated ones too)
    // keep the order they finished in and their colour; each one joining goes into a random place among
    // them, with the first colour nobody has.
    public Game PlayAgain(IEnumerable<Player> staying, IEnumerable<string> joining, Random random)
    {
        var stayingSeats = staying.Select(player => player.Seat).ToHashSet();
        var seats = AllPlayers.Select(player => player.Seat).Where(stayingSeats.Contains).ToList();
        if (seats.Count != stayingSeats.Count) throw new ArgumentException("Only this game's players can stay.", nameof(staying));

        foreach (var name in joining)
        {
            var color = Enumerable.Range(0, seats.Count + 1).First(index => seats.All(seat => seat.ColorIndex != index));
            seats.Insert(random.Next(seats.Count + 1), new Seat(name, color));
        }

        return new Game(Settings, seats, []);
    }

    // A stored game. Throws ArgumentException when the players or throws break the rules.
    internal static Game Restore(GameSettings settings, IReadOnlyList<Seat> seats, IEnumerable<int> throws) =>
        new(settings, seats, [.. throws]);

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

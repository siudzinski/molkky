using System.Collections.Immutable;
using Molkky.Domain.Rules;

namespace Molkky.Domain;

// A player's state at one point of a game. Computed by replaying the throws, never stored.
public sealed class Player
{
    internal const int MissesInARowToBeOut = 3;

    private readonly ImmutableList<int> _scoreHistory;

    internal Seat Seat { get; }

    public string Name => Seat.Name;
    public string FirstLetter => Name[..1].ToUpper();
    // Index into the UI's colour palette, which repeats when there are more players than colours.
    public int ColorIndex => Seat.ColorIndex;
    public int Score { get; }
    // The score after each of this player's throws.
    public IReadOnlyList<int> ScoreHistory => _scoreHistory;
    public int NumberOfFailedThrows { get; }
    public bool InDanger => NumberOfFailedThrows > 0;
    public bool CanPlay => NumberOfFailedThrows < MissesInARowToBeOut;

    private Player(Seat seat, int score, int numberOfFailedThrows, ImmutableList<int> scoreHistory)
    {
        Seat = seat;
        Score = score;
        NumberOfFailedThrows = numberOfFailedThrows;
        _scoreHistory = scoreHistory;
    }

    internal static Player AtStart(Seat seat) => new(seat, 0, 0, []);

    public static Player CreateNew(string name) => AtStart(new Seat(name, 0));

    // The player after one more throw: a hit (1-12) adds its points and clears the misses, a miss (0)
    // counts as a failed throw. An eliminated player ignores throws.
    public Player Throw(int points, GameRules rules)
    {
        if (!CanPlay) return this;

        var score = Score;
        var failedThrows = NumberOfFailedThrows;

        if (points > 0)
        {
            score += points;
            failedThrows = 0;
            if (score > Game.PointsToWin)
            {
                score = rules.MaximumPoints.ScoreAfterGoingOver(score);
            }
        }
        else
        {
            failedThrows++;
            if (failedThrows == MissesInARowToBeOut)
            {
                (score, failedThrows) = rules.MissedThrows.AfterThirdMiss(score);
            }
        }

        return new Player(Seat, score, failedThrows, _scoreHistory.Add(score));
    }
}

using System.Collections.Immutable;

namespace Molkky.Domain;

// A player's state at one point of a game. Computed by replaying the throws, never stored.
public sealed class Player
{
    private const int MissesInARowToBeOut = 3;

    private readonly ImmutableList<int> _scoreHistory;

    internal Seat Seat { get; }

    public string Name => Seat.Name;
    public string FirstLetter => Name[..1].ToUpper();
    public string AvatarColor => Seat.AvatarColor;
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

    public static Player CreateNew(string name) => AtStart(new Seat(name, ColorProvider.Instance.GetNextColor()));

    // The player after one more throw. An eliminated player ignores throws.
    public Player Throw(int points, GameSettings settings)
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
                //TODO inject strategy instead of enum
                if (settings.MaximumPoints == MaximumPointsStrategies.MaxScoreInHalf)
                {
                    score = 25;
                }
                if (settings.MaximumPoints == MaximumPointsStrategies.BackToZero)
                {
                    score = 0;
                }
            }
        }
        else
        {
            failedThrows++;
            //TODO inject strategy instead of enum
            if (settings.MissedThrows == MissedThrowsStrategies.BackToZero && failedThrows == MissesInARowToBeOut)
            {
                failedThrows = 0;
                score = 0;
            }
        }

        return new Player(Seat, score, failedThrows, _scoreHistory.Add(score));
    }
}

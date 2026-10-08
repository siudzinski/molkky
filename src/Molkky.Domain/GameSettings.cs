namespace Molkky.Domain;

// The rule variants a game is played with.
public sealed record GameSettings(MaximumPointsStrategies MaximumPoints, MissedThrowsStrategies MissedThrows)
{
    public static GameSettings Default { get; } = new(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.Disqualified);
}

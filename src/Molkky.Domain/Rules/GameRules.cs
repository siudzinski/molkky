namespace Molkky.Domain.Rules;

// The rules a game is played by: one rule per setting, plus the house rule for the order.
public sealed class GameRules
{
    public IMaximumPointsRule MaximumPoints { get; }
    public IMissedThrowsRule MissedThrows { get; }
    public LowestScoreThrowsFirst RoundOrder { get; } = new();

    private GameRules(IMaximumPointsRule maximumPoints, IMissedThrowsRule missedThrows)
    {
        MaximumPoints = maximumPoints;
        MissedThrows = missedThrows;
    }

    public static GameRules For(GameSettings settings) =>
        new(
            settings.MaximumPoints switch
            {
                MaximumPointsStrategies.MaxScoreInHalf => new MaxScoreInHalf(),
                MaximumPointsStrategies.BackToZero => new MaxScoreBackToZero(),
                _ => throw new ArgumentOutOfRangeException(nameof(settings), settings.MaximumPoints, "Unknown maximum points setting."),
            },
            settings.MissedThrows switch
            {
                MissedThrowsStrategies.Disqualified => new ThreeMissesDisqualify(),
                MissedThrowsStrategies.BackToZero => new ThreeMissesBackToZero(),
                _ => throw new ArgumentOutOfRangeException(nameof(settings), settings.MissedThrows, "Unknown missed throws setting."),
            });
}

namespace Molkky.Domain.StorageModels;

// Players in their starting order; the game is replayed from the throws.
public record GameState(
    IEnumerable<PlayerState> Players,
    MaximumPointsStrategies MaximumPointsStrategy,
    MissedThrowsStrategies MissedThrowsStrategy,
    int[] Throws);

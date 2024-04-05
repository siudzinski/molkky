namespace molkky.Domain.StorageModels;

public record GameState(
    IEnumerable<PlayerState> Players, 
    MaximumPointsStrategies MaximumPointsStrategy,
    MissedThrowsStrategies MissedThrowsStrategy,
    int NumberOfThrowsInRound, 
    int RoundNumber);

namespace molkky.Domain.StorageModels;

public record GameState(
    IEnumerable<PlayerState> Players, 
    MaximumPointsStrategies MaximumPointsStrategy,
    int NumberOfThrowsInRound, 
    int RoundNumber);

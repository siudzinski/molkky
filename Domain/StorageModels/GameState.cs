namespace molkky.Domain.StorageModels;

public record GameState(IEnumerable<PlayerState> Players, int NumberOfThrowsInRound, int RoundNumber);

namespace molkky.Domain.StorageModels;

public record PlayerState(string Name, int Score, int NumberOfFailedThrows, int[] ScoreHistory);

public static class PlayerExtensions
{
    public static IEnumerable<PlayerState> ToPlayerStateList(this IEnumerable<Player> players)
    {
        return players.Select(_ => _.ToPlayerState());
    }
}

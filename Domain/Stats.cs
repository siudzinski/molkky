namespace molkky.Domain;

public class Stats
{
    public string Winner { get; }
    public IEnumerable<PlayerStats> Players { get; }
    public string[] Rounds { get; }

    public Stats(
        string winner,
        IEnumerable<PlayerStats> players,
        int roundsNumber)
    {
        Winner = winner;
        Players = players;
        Rounds = Enumerable.Range(1, roundsNumber).Select(i => i.ToString()).ToArray();
    }
}

public record PlayerStats(string Name, IEnumerable<int> ScoreHistory);
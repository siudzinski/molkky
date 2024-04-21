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
        Rounds = Enumerable.Range(0, roundsNumber).Select(i => i.ToString()).ToArray();
    }
}

public class PlayerStats
{
    public string Name { get; }
    public double[] ScoreHistory { get; }

    public PlayerStats(string name, IEnumerable<int> scoreHistory)
    {
        Name = name;
        ScoreHistory = new double[] { 0 }.Concat(scoreHistory.Select(x => (double)x)).ToArray();
    }
}
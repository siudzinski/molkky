namespace molkky.Domain;

public class Game
{
    public List<Player> Players { get; private set; }
    public List<Player> Losers { get; private set; } = new List<Player>();
    public Player CurrentPlayer => Players[_numberOfThrowsInRound];
    public Player? Winner => Players.FirstOrDefault(_ => _.Won) ?? (Players.Count > 1 ? null : Players.First());

    private List<Player> _players;
    private int _numberOfThrowsInRound = 0;

    private Game(List<Player> players)
    {
        _players = players;
        Players = _players;
    }

    public static Game CreateNew(List<Player> players)
    {
        return new Game(players);
    }

    public void SetThrowScoreForCurrentPlayer(int score)
    {
        CurrentPlayer.AddPoints(score);
        EvaluatePlayers();
    }

    private void EvaluatePlayers()
    {
        if(CurrentPlayer.CanPlay)
        {
            _numberOfThrowsInRound++;
        }

        if(_numberOfThrowsInRound ==  _players.Where(_ => _.CanPlay).Count())
        {
            _numberOfThrowsInRound = 0;
            _players = _players.OrderBy(_ => _.Score).ToList();
        }

        Players = _players.Where(_ => _.CanPlay).ToList();
        Losers = _players.Where(_ => !_.CanPlay).ToList();
    }
}

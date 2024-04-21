using molkky.Domain.StorageModels;

namespace molkky.Domain;

public class Game
{
    private const int PointsRequiredToWin = 50;

    public List<Player> Players { get; private set; }
    public List<Player> Losers { get; private set; } = new List<Player>();
    public Player CurrentPlayer => Players[_numberOfThrowsInRound];
    public Player? Winner => Players.FirstOrDefault(_ => _.Score == PointsRequiredToWin) ?? (Players.Count > 1 ? null : Players.First());
    public bool AnyWinner => Winner is not null;
    public int RoundNumber => _roundNumber;

    private List<Player> _players;
    private int _numberOfThrowsInRound = 0;
    private int _roundNumber = 1;
    private readonly MaximumPointsStrategies _maximumPointsStrategy;
    private readonly MissedThrowsStrategies _missedThrowsStrategy;

    private Game(IEnumerable<Player> players, MaximumPointsStrategies maximumPointsStrategy, MissedThrowsStrategies missedThrowsStrategy)
    {
        _players = players.ToList();
        _maximumPointsStrategy = maximumPointsStrategy;
        _missedThrowsStrategy = missedThrowsStrategy;
        Players = _players.Where(_ => _.CanPlay).ToList();
        Losers = _players.Where(_ => !_.CanPlay).ToList();
    }

    private Game(
        IEnumerable<Player> players,
        MaximumPointsStrategies maximumPointsStrategy,
        MissedThrowsStrategies missedThrowsStrategy,
        int numberOfThrowsInRound, 
        int roundNumber) : this(players, maximumPointsStrategy, missedThrowsStrategy)
    {
        _numberOfThrowsInRound = numberOfThrowsInRound;
        _roundNumber = roundNumber;
    }

    public static Game CreateNew(
        IEnumerable<Player> players, 
        MaximumPointsStrategies maximumPointsStrategy, 
        MissedThrowsStrategies missedThrowsStrategy)
    {
        return new Game(players, maximumPointsStrategy, missedThrowsStrategy);
    }

    public static Game FromGameState(GameState gameState)
    {
        return new Game(
            gameState.Players.Select(Player.FromPlayerState), 
            gameState.MaximumPointsStrategy,
            gameState.MissedThrowsStrategy,
            gameState.NumberOfThrowsInRound,
            gameState.RoundNumber);
    }

    public GameState ToGameState()
    {
        return new GameState(
            _players.Select(p => p.ToPlayerState()), _maximumPointsStrategy, _missedThrowsStrategy, _numberOfThrowsInRound, _roundNumber);
    }

    public Stats ToStats()
    {
        if (Winner is null)
        {
            throw new InvalidOperationException();
        }

        var players = _players.Select(p => new PlayerStats(p.Name, p.ScoreHistory));

        return new Stats(Winner.Name, players, _roundNumber);
    }

    public void SetThrowScoreForCurrentPlayer(int score)
    {
        if(Winner is not null) return;
        
        CurrentPlayer.AddPoints(score, _maximumPointsStrategy, _missedThrowsStrategy);
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
            _roundNumber++;
        }

        Players = _players.Where(_ => _.CanPlay).ToList();
        Losers = _players.Where(_ => !_.CanPlay).ToList();
    }
}

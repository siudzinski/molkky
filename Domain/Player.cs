using System.Collections.ObjectModel;
using molkky.Domain.StorageModels;

namespace molkky.Domain;

public class Player
{
    private int _score = 0;
    private int _numberOfFailedThrows = 0;
    private List<int> _scoreHistory = new();

    public string Name { get; private set; }
    public string FirstLetter => Name[..1].ToUpper();
    public int Score => _score;
    public ReadOnlyCollection<int> ScoreHistory => _scoreHistory.AsReadOnly();
    public int NumberOfFailedThrows => _numberOfFailedThrows;
    public bool InDanger => _numberOfFailedThrows > 0;
    public bool CanPlay => _numberOfFailedThrows < 3;

    private Player(string name)
    {
        Name = name;
    }

    private Player(string name, int score, int numberOfFailedThrows, int[] scoreHistory) : this(name)
    {
        _score = score;
        _numberOfFailedThrows = numberOfFailedThrows;
        _scoreHistory = scoreHistory.ToList();
    }

    public static Player CreateNew(string name)
    {
        return new Player(name);
    }

    public static Player FromPlayerState(PlayerState playerState)
    {
        return new Player(playerState.Name, playerState.Score, playerState.NumberOfFailedThrows, playerState.ScoreHistory);
    }

    public PlayerState ToPlayerState()
    {
        return new PlayerState(Name, _score, _numberOfFailedThrows, _scoreHistory.ToArray());
    }

    public void Reset()
    {
        _score = 0;
        _numberOfFailedThrows = 0;
        _scoreHistory = new();
    }

    public void AddPoints(
        int score, 
        MaximumPointsStrategies maximumPointsStrategy, 
        MissedThrowsStrategies missedThrowsStrategy)
    {
        if(!CanPlay) return;
        
        if(score > 0)
        {
            _score += score;
            _numberOfFailedThrows = 0;
            if(_score > 50)
            {
                //TODO inject strategy instead of enum
                if(maximumPointsStrategy == MaximumPointsStrategies.MaxScoreInHalf)
                {
                    _score = 25;
                    AddToScoreHistory(25, isAbsolute: true);
                }
                if(maximumPointsStrategy == MaximumPointsStrategies.BackToZero) 
                {
                    _score = 0;
                    AddToScoreHistory(0, isAbsolute: true);
                }
            }
            else
            {
                AddToScoreHistory(score);
            }
        }
        else
        {
            //TODO inject strategy instead of enum
            if(missedThrowsStrategy == MissedThrowsStrategies.Disqualified)
            {
                _numberOfFailedThrows++;
            }
            if(missedThrowsStrategy == MissedThrowsStrategies.BackToZero)
            {
                _numberOfFailedThrows++;
                if(_numberOfFailedThrows == 3)
                {
                    _numberOfFailedThrows = 0;
                    _score = 0;
                }
            }
            AddToScoreHistory(score);
        }
    }

    private void AddToScoreHistory(int score, bool isAbsolute = false)
    {
        if(isAbsolute)
        {
            _scoreHistory.Add(score);
            return;
        }

        _scoreHistory.Add(score + (_scoreHistory.Any() ? _scoreHistory.Last() : 0));
    }
}

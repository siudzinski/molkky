using molkky.Domain.StorageModels;

namespace molkky.Domain;

public class Player
{
    private int _score = 0;
    private int _numberOfFailedThrows = 0;

    public string Name { get; private set; }
    public int Score => _score;
    public int NumberOfFailedThrows => _numberOfFailedThrows;
    public bool InDanger => _numberOfFailedThrows > 0;
    public bool CanPlay => _numberOfFailedThrows < 3;

    private Player(string name)
    {
        Name = name;
    }

    private Player(string name, int score, int numberOfFailedThrows) : this(name)
    {
        _score = score;
        _numberOfFailedThrows = numberOfFailedThrows;
    }

    public static Player CreateNew(string name)
    {
        return new Player(name);
    }

    public static Player FromPlayerState(PlayerState playerState)
    {
        return new Player(playerState.Name, playerState.Score, playerState.NumberOfFailedThrows);
    }

    public PlayerState ToPlayerState()
    {
        return new PlayerState(Name, _score, _numberOfFailedThrows);
    }

    public void AddPoints(int score, MaximumPointsStrategies maximumPointsStrategy)
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
                }
                if(maximumPointsStrategy == MaximumPointsStrategies.BackToZero) 
                {
                    _score = 0;
                }
            }
        }
        else
        {
            _numberOfFailedThrows++;
        }
    }
}

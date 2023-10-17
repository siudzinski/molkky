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
    public bool CanPlay => _numberOfFailedThrows < 2;
    public bool Won => _score == 50;

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

    public void AddPoints(int score)
    {
        if(!CanPlay) return;
        
        if(score > 0)
        {
            _score += score;
            _numberOfFailedThrows = 0;
            if(_score > 50)
            {
                _score = 25;
            }
        }
        else
        {
            _numberOfFailedThrows++;
        }
    }
}

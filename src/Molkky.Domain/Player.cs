using System.Collections.ObjectModel;
using Molkky.Domain.StorageModels;

namespace Molkky.Domain;

public class Player
{
    private int _score = 0;
    private int _numberOfFailedThrows = 0;
    private List<int> _scoreHistory = new();

    public string Name { get; private set; }
    public string FirstLetter => Name[..1].ToUpper();
    public string AvatarColor { get; private set; }
    public int Score => _score;
    public ReadOnlyCollection<int> ScoreHistory => _scoreHistory.AsReadOnly();
    public int NumberOfFailedThrows => _numberOfFailedThrows;
    public bool InDanger => _numberOfFailedThrows > 0;
    public bool CanPlay => _numberOfFailedThrows < 3;

    private Player(string name)
    {
        Name = name;
        AvatarColor = ColorProvider.Instance.GetNextColor();
    }

    private Player(string name, int score, int numberOfFailedThrows, int[] scoreHistory, string avatarColor)
    {
        Name = name;
        _score = score;
        _numberOfFailedThrows = numberOfFailedThrows;
        _scoreHistory = scoreHistory.ToList();
        AvatarColor = avatarColor;
    }

    public static Player CreateNew(string name)
    {
        return new Player(name);
    }

    public static Player FromPlayerState(PlayerState playerState)
    {
        return new Player(playerState.Name, playerState.Score, playerState.NumberOfFailedThrows, playerState.ScoreHistory, playerState.AvatarColor);
    }

    public PlayerState ToPlayerState()
    {
        return new PlayerState(Name, _score, _numberOfFailedThrows, _scoreHistory.ToArray(), AvatarColor);
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
        if (!CanPlay) return;

        if (score > 0)
        {
            _score += score;
            _numberOfFailedThrows = 0;
            if (_score > 50)
            {
                //TODO inject strategy instead of enum
                if (maximumPointsStrategy == MaximumPointsStrategies.MaxScoreInHalf)
                {
                    _score = 25;
                }
                if (maximumPointsStrategy == MaximumPointsStrategies.BackToZero)
                {
                    _score = 0;
                }
            }
        }
        else
        {
            //TODO inject strategy instead of enum
            if (missedThrowsStrategy == MissedThrowsStrategies.Disqualified)
            {
                _numberOfFailedThrows++;
            }
            if (missedThrowsStrategy == MissedThrowsStrategies.BackToZero)
            {
                _numberOfFailedThrows++;
                if (_numberOfFailedThrows == 3)
                {
                    _numberOfFailedThrows = 0;
                    _score = 0;
                }
            }
        }

        // Record the score itself, not a running sum, so the history follows every reset.
        _scoreHistory.Add(_score);
    }
}

namespace molkky.Domain;

public class Player
{
    private int _score = 0;
    private int _numberOfFailedThrows = 0;

    public required string Name { get; init; }
    public int Score => _score;
    public bool CanPlay => _numberOfFailedThrows < 1;
    public bool Won => _score == 50;

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

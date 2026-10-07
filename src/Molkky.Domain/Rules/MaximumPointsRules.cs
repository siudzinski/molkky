namespace Molkky.Domain.Rules;

// Setting "what happens when a player scores more than 50": the score after a hit that takes
// a player over the points needed to win.
public interface IMaximumPointsRule
{
    int ScoreAfterGoingOver(int score);
}

// Back to half the winning score: 25.
public sealed class MaxScoreInHalf : IMaximumPointsRule
{
    public int ScoreAfterGoingOver(int score) => Game.PointsToWin / 2;
}

public sealed class MaxScoreBackToZero : IMaximumPointsRule
{
    public int ScoreAfterGoingOver(int score) => 0;
}

namespace Molkky.Domain.Rules;

// Setting "what happens when a player misses 3 times in a row": the score and the count of
// misses in a row after the third miss. A player left with 3 misses is out of the game.
public interface IMissedThrowsRule
{
    (int Score, int FailedThrows) AfterThirdMiss(int score);
}

// Out of the game, keeping the score.
public sealed class ThreeMissesDisqualify : IMissedThrowsRule
{
    public (int Score, int FailedThrows) AfterThirdMiss(int score) => (score, Player.MissesInARowToBeOut);
}

// Score and misses back to 0, still in the game.
public sealed class ThreeMissesBackToZero : IMissedThrowsRule
{
    public (int Score, int FailedThrows) AfterThirdMiss(int score) => (0, 0);
}

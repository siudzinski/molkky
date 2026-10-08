namespace Molkky.Domain.Rules;

// House rule, not the official fixed order: after each round the players are re-sorted so the
// lowest score throws first. Players on equal scores keep their order from the round before.
// Eliminated players are sorted along with the rest, which sets the order a play-again starts in.
public sealed class LowestScoreThrowsFirst
{
    public List<Player> OrderForNextRound(IEnumerable<Player> players) => players.OrderBy(player => player.Score).ToList();
}

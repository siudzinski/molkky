namespace Molkky.Domain;

// A player as entered when the game starts: everything about a player that is not computed from the throws.
// ColorIndex is the player's place in the starting order of their first game, or for someone who joined on
// play again the first index nobody else had; the UI maps it to a colour.
internal sealed record Seat(string Name, int ColorIndex);

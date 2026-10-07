namespace Molkky.Domain;

// A player as entered when the game starts: everything about a player that is not computed from the throws.
internal sealed record Seat(string Name, string AvatarColor);

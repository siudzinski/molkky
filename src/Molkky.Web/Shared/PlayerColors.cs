namespace Molkky.Web.Shared;

// A player's colour as Tailwind classes, whole so Tailwind finds them (see Styles/app.css).
// Avatar: background and initial. Line and Swatch: the endgame chart's line and legend dot.
public sealed record PlayerColor(string Avatar, string Line, string Swatch);

// Maps a player's palette index (Player.ColorIndex) to their colour: the player-1 … player-8 tokens in
// Styles/app.css, each with a light and a dark value. Ordered so that the first few players differ most.
public static class PlayerColors
{
    private static readonly PlayerColor[] Palette =
    [
        new("bg-player-1 text-on-player", "stroke-player-1", "bg-player-1"),
        new("bg-player-2 text-on-player", "stroke-player-2", "bg-player-2"),
        new("bg-player-3 text-on-player", "stroke-player-3", "bg-player-3"),
        new("bg-player-4 text-on-player", "stroke-player-4", "bg-player-4"),
        new("bg-player-5 text-on-player", "stroke-player-5", "bg-player-5"),
        new("bg-player-6 text-on-player", "stroke-player-6", "bg-player-6"),
        new("bg-player-7 text-on-player", "stroke-player-7", "bg-player-7"),
        new("bg-player-8 text-on-player", "stroke-player-8", "bg-player-8"),
    ];

    public static int Count => Palette.Length;

    public static PlayerColor For(int colorIndex) => Palette[colorIndex % Palette.Length];
}

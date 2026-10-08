namespace Molkky.Web.Shared;

// A player's colour as Tailwind classes, whole so Tailwind finds them (see Styles/app.css).
// Avatar: background and initial. Line and Swatch: the endgame chart's line and legend dot.
public sealed record PlayerColor(string Avatar, string Line, string Swatch);

// Maps a player's palette index (Player.ColorIndex) to their colour. The hues keep the order players had
// with MudBlazor (primary, secondary, info, success, warning). Shades keep the white initial readable;
// chart lines get lighter on the dark background.
public static class PlayerColors
{
    private static readonly PlayerColor[] Palette =
    [
        new("bg-violet-600 text-white", "stroke-violet-600 dark:stroke-violet-400", "bg-violet-600 dark:bg-violet-400"),
        new("bg-pink-600 text-white", "stroke-pink-600 dark:stroke-pink-400", "bg-pink-600 dark:bg-pink-400"),
        new("bg-sky-700 text-white", "stroke-sky-600 dark:stroke-sky-400", "bg-sky-600 dark:bg-sky-400"),
        new("bg-green-700 text-white", "stroke-green-600 dark:stroke-green-400", "bg-green-600 dark:bg-green-400"),
        new("bg-orange-600 text-white", "stroke-orange-500 dark:stroke-orange-400", "bg-orange-500 dark:bg-orange-400"),
    ];

    public static PlayerColor For(int colorIndex) => Palette[colorIndex % Palette.Length];
}

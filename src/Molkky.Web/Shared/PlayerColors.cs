using MudBlazor;

namespace Molkky.Web.Shared;

// Maps a player's palette index (Player.ColorIndex) to an avatar colour.
public static class PlayerColors
{
    private static readonly Color[] Palette = [Color.Primary, Color.Secondary, Color.Info, Color.Success, Color.Warning];

    public static Color For(int colorIndex) => Palette[colorIndex % Palette.Length];
}

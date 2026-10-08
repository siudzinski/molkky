using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace Molkky.Web.Tests;

// A scoreboard row as shown: the player's name, misses in a row ("" for none), score, and whether it
// is marked as the current player.
public sealed record ScoreboardRow(string Player, string Misses, int Score, bool Current);

public static class ScoreboardMarkup
{
    public const string ThrowingOrder = "Throwing order";
    public const string OutOfTheGame = "Out of the game";

    // The rows of the scoreboard table with this caption, top to bottom; null when there is no such table.
    public static ScoreboardRow[]? Rows<TComponent>(this IRenderedComponent<TComponent> component, string caption)
        where TComponent : IComponent =>
        component.FindAll("table")
            .SingleOrDefault(table => table.QuerySelector("caption")?.TextContent.Trim() == caption)?
            .QuerySelectorAll("tbody tr")
            .Select(Row)
            .ToArray();

    private static ScoreboardRow Row(IElement row)
    {
        var cells = row.QuerySelectorAll("td");
        return new ScoreboardRow(
            row.QuerySelector("th")!.TextContent.Trim(),
            cells[1].TextContent.Trim(),
            int.Parse(cells[2].TextContent.Trim()),
            row.GetAttribute("aria-current") == "true");
    }
}

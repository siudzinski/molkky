using System.Text.Json;

namespace Molkky.Domain.Storage;

// How a game and the settings are stored: JSON documents with a version, { "version": 1, ... }.
// Loading never throws: nothing saved, unreadable JSON, another version or a game the rules reject
// all load as null, and the app starts fresh.
public static class StorageFormat
{
    public const int Version = 1;

    public static string SaveGame(Game game) =>
        JsonSerializer.Serialize(
            new GameDocumentV1(
                Version,
                new SettingsV1(game.Settings.MaximumPoints, game.Settings.MissedThrows),
                [.. game.Seats.Select(seat => new PlayerV1(seat.Name, seat.ColorIndex))],
                [.. game.Throws]),
            StorageJsonContext.Default.GameDocumentV1);

    public static Game? LoadGame(string? json)
    {
        try
        {
            if (VersionOf(json) != Version) return null;

            var document = JsonSerializer.Deserialize(json!, StorageJsonContext.Default.GameDocumentV1)!;
            if (document.Players.Any(player => player is null)) return null;

            // The replay checks the players and every throw against the rules.
            return Game.Restore(
                new GameSettings(document.Settings.MaximumPoints, document.Settings.MissedThrows),
                [.. document.Players.Select(player => new Seat(player.Name, player.Color))],
                document.Throws);
        }
        catch (Exception e) when (e is JsonException or ArgumentException)
        {
            return null;
        }
    }

    public static string SaveSettings(GameSettings settings) =>
        JsonSerializer.Serialize(
            new SettingsDocumentV1(Version, settings.MaximumPoints, settings.MissedThrows),
            StorageJsonContext.Default.SettingsDocumentV1);

    public static GameSettings? LoadSettings(string? json)
    {
        try
        {
            if (VersionOf(json) != Version) return null;

            var document = JsonSerializer.Deserialize(json!, StorageJsonContext.Default.SettingsDocumentV1)!;
            var settings = new GameSettings(document.MaximumPoints, document.MissedThrows);

            return Enum.IsDefined(settings.MaximumPoints) && Enum.IsDefined(settings.MissedThrows) ? settings : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // The "version" of a JSON object, or null when there is none to read.
    private static int? VersionOf(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("version", out var version)
            && version.ValueKind == JsonValueKind.Number
            && version.TryGetInt32(out var number)
                ? number
                : null;
    }
}

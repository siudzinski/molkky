using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace Molkky.Web.Infrastructure;

// Light or dark as the system is set, or as chosen in the app.
public enum Theme
{
    System,
    Light,
    Dark,
}

// The chosen theme. The script at the top of index.html reads the saved one and puts it on the page
// before the app starts (data-theme on <html>), so the page never shows the other theme first; a change
// here is saved and passed to that script (molkkyTheme.apply).
public sealed class ThemeStore(ILocalStorage storage, IJSRuntime jsRuntime)
{
    public const string StorageKey = "molkky.theme";
    private const int Version = 1;

    // The stored names, which index.html reads too.
    private static readonly Dictionary<Theme, string> Names = new()
    {
        [Theme.System] = "system",
        [Theme.Light] = "light",
        [Theme.Dark] = "dark",
    };

    public Theme Theme { get; private set; } = Theme.System;

    public event Action? OnThemeChanged;

    // System when no theme is saved, or the saved one is unreadable or not one the app has.
    public async Task Load()
    {
        Theme = Read(await storage.GetItem(StorageKey)) ?? Theme.System;
    }

    public async Task SetTheme(Theme theme)
    {
        Theme = theme;
        OnThemeChanged?.Invoke();
        await jsRuntime.InvokeVoidAsync("molkkyTheme.apply", Names[theme]);
        await storage.SetItem(StorageKey, JsonSerializer.Serialize(new ThemeDocumentV1(Version, Names[theme]), ThemeJsonContext.Default.ThemeDocumentV1));
    }

    private static Theme? Read(string? json)
    {
        try
        {
            var document = string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize(json, ThemeJsonContext.Default.ThemeDocumentV1);

            return document is { Version: Version } ? Names.Where(name => name.Value == document.Theme).Select(name => (Theme?)name.Key).SingleOrDefault() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

// The stored theme, version 1: { "version": 1, "theme": "dark" }, the theme one of "system", "light" or "dark".
internal sealed record ThemeDocumentV1(int Version, string Theme);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ThemeDocumentV1))]
internal sealed partial class ThemeJsonContext : JsonSerializerContext;

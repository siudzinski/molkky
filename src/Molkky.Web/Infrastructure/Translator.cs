using System.Text.Json;
using System.Text.Json.Serialization;

namespace Molkky.Web.Infrastructure;

public class Translator(ILocalStorage storage)
{
    public const string StorageKey = "molkky.language";
    private const int Version = 1;

    public string Language { get; private set; } = Translations.English;

    // Every visible string in the current language.
    public Texts Text => Translations.For(Language);

    public event Action? OnLanguageChanged;

    // English when no language is saved, or the saved one is unreadable or not one the app has.
    public async Task LoadLanguage()
    {
        Language = Read(await storage.GetItem(StorageKey)) ?? Translations.English;
    }

    public async Task SetLanguage(string language)
    {
        Language = language;
        OnLanguageChanged?.Invoke();
        await storage.SetItem(StorageKey, JsonSerializer.Serialize(new LanguageDocumentV1(Version, language), LanguageJsonContext.Default.LanguageDocumentV1));
    }

    private static string? Read(string? json)
    {
        try
        {
            var document = string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize(json, LanguageJsonContext.Default.LanguageDocumentV1);

            return document is { Version: Version } && Translations.Languages.ContainsKey(document.Language) ? document.Language : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

// The stored language, version 1: { "version": 1, "language": "pl" }.
internal sealed record LanguageDocumentV1(int Version, string Language);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(LanguageDocumentV1))]
internal sealed partial class LanguageJsonContext : JsonSerializerContext;

using Microsoft.JSInterop;

namespace molkky.Infrastructure;

public class Translator
{
    private const string StorageKey = "language";

    private readonly IJSRuntime _jsRuntime;

    public string Language { get; private set; } = Translations.English;

    public Translator(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public event Action? OnLanguageChanged;

    public async Task LoadLanguage()
    {
        var language = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", StorageKey);
        Language = language ?? Translations.English;
    }

    public void SetLanguage(string language)
    {
        Language = language;
        SaveLanguage();
        OnLanguageChanged?.Invoke();
    }

    public string Get(string key)
    {
        return Translations.Items[Language][key];
    }

    private void SaveLanguage()
    {
        _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, Language);
    }
}
using System.Text.Json;
using Microsoft.JSInterop;
using molkky.Domain;
using molkky.Domain.StorageModels;

namespace molkky.Infrastructure;

public class SettingsSessionStorage
{
    private const string StorageKey = "settings";

    private readonly IJSRuntime _jsRuntime;

    private readonly SettingsState defaultSettingsState = new(MaximumPointsStrategies.MaxScoreInHalf, MissedThrowsStrategies.Disqualified);

    public SettingsSessionStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<SettingsState> LoadSettingsState()
    {
        var jsonString = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", StorageKey);
        if (jsonString != null)
        {

            var settingsState = JsonSerializer.Deserialize<SettingsState>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if(settingsState is not null)
            {
                return settingsState;
            }
        }

        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, JsonSerializer.Serialize(defaultSettingsState));

        return defaultSettingsState;
    }

    public async Task SaveSettingsState(SettingsState settingsState)
    {
        var jsonString = JsonSerializer.Serialize(settingsState);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, jsonString);
    }
}

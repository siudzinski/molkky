using System.Text.Json;
using Microsoft.JSInterop;
using molkky.Domain;
using molkky.Domain.StorageModels;

namespace molkky.Infrastructure;

public class SettingsSessionStorage
{
    private const string StorageKey = "settings";

    private readonly IJSRuntime _jsRuntime;

    public SettingsSessionStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitSettingsState()
    {
        var jsonString = JsonSerializer.Serialize(new SettingsState(MaximumPointsStrategies.MaxScoreInHalf));
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, jsonString);
    }

    public async Task<MaximumPointsStrategies> LoadSettingsState()
    {
        var jsonString = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", StorageKey);
        if (jsonString != null)
        {

            var settingsState = JsonSerializer.Deserialize<SettingsState>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if(settingsState is null) return MaximumPointsStrategies.MaxScoreInHalf;

            return settingsState.MaximumPointsStrategy;
        }

        return MaximumPointsStrategies.MaxScoreInHalf;
    }

    public async Task SaveSettingsState(MaximumPointsStrategies maximumPointsStrategy)
    {
        var jsonString = JsonSerializer.Serialize(new SettingsState(maximumPointsStrategy));
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, jsonString);
    }

    public async Task RemoveSettingsState()
    {
        await _jsRuntime.InvokeAsync<string>("sessionStorage.removeItem", StorageKey);
    }
}

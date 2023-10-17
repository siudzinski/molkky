using System.Text.Json;
using Microsoft.JSInterop;
using molkky.Domain;
using molkky.Domain.StorageModels;

namespace molkky.Infrastructure;

public class GameSessionStorage
{
    private const string StorageKey = "game";

    private readonly IJSRuntime _jsRuntime;

    public GameSessionStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<Game?> LoadGameState()
    {
        var jsonString = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", StorageKey);
        if (jsonString != null)
        {

            var gameState = JsonSerializer.Deserialize<GameState>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if(gameState is null) return null;

            return Game.FromGameState(gameState);
        }

        return null;
    }

    public async Task SaveGameState(Game? game)
    {
        if(game is not null)
        {
            var jsonString = JsonSerializer.Serialize(game.ToGameState());
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, jsonString);
        }
    }

    public async Task RemoveGameState()
    {
        await _jsRuntime.InvokeAsync<string>("sessionStorage.removeItem", StorageKey);
    }
}

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Domain;
using Molkky.Domain.Storage;
using Molkky.Web.Infrastructure;
using MudBlazor.Services;

namespace Molkky.Web.Tests;

// Base class for component tests: registers what the app registers in Program.cs, with
// localStorage in memory.
public abstract class MudBlazorTestContext : BunitContext
{
    protected FakeLocalStorage Storage { get; } = new();

    protected MudBlazorTestContext()
    {
        // MudBlazor components call into JavaScript; let every call succeed with a default result.
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton<ILocalStorage>(Storage);
        Services.AddSingleton<GameStore>();
        Services.AddSingleton<SettingsStore>();
        Services.AddSingleton<Translator>();
        Services.AddSingleton(Random.Shared);
    }

    protected string Uri => Services.GetRequiredService<NavigationManager>().Uri;

    protected void SaveGame(Game game) => Storage.Items[GameStore.Key] = StorageFormat.SaveGame(game);

    protected Game? SavedGame() => StorageFormat.LoadGame(Storage.Items.GetValueOrDefault(GameStore.Key));
}

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Domain;
using Molkky.Domain.Storage;
using Molkky.Web.Infrastructure;

namespace Molkky.Web.Tests;

// Base class for component tests: registers what the app registers in Program.cs, with
// localStorage in memory. The texts are in English: the context starts as someone who chose English
// (a fresh start is in Polish, see TranslatorTests).
public abstract class AppTestContext : BunitContext
{
    protected FakeLocalStorage Storage { get; } = new();

    protected AppTestContext()
    {
        Services.AddSingleton<ILocalStorage>(Storage);
        Services.AddSingleton<GameStore>();
        Services.AddSingleton<SettingsStore>();
        Storage.Items[Translator.StorageKey] = """{"version":1,"language":"en"}""";
        var translator = new Translator(Storage);
        translator.LoadLanguage().GetAwaiter().GetResult();
        Services.AddSingleton(translator);
        Services.AddSingleton<ThemeStore>();
        Services.AddSingleton(Random.Shared);
    }

    protected string Uri => Services.GetRequiredService<NavigationManager>().Uri;

    protected void SaveGame(Game game) => Storage.Items[GameStore.Key] = StorageFormat.SaveGame(game);

    protected Game? SavedGame() => StorageFormat.LoadGame(Storage.Items.GetValueOrDefault(GameStore.Key));
}

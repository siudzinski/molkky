using Molkky.Domain;
using Molkky.Domain.Storage;

namespace Molkky.Web.Infrastructure;

// The settings a new game starts with.
public sealed class SettingsStore(ILocalStorage storage)
{
    public const string Key = "molkky.settings";

    // The default settings when none are saved or the saved ones cannot be read.
    public async Task<GameSettings> Load() => StorageFormat.LoadSettings(await storage.GetItem(Key)) ?? GameSettings.Default;

    public Task Save(GameSettings settings) => storage.SetItem(Key, StorageFormat.SaveSettings(settings));
}

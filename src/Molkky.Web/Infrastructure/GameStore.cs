using Molkky.Domain;
using Molkky.Domain.Storage;

namespace Molkky.Web.Infrastructure;

// The game in progress (or just finished), saved after every change.
public sealed class GameStore(ILocalStorage storage)
{
    public const string Key = "molkky.game";

    // Null when there is no game, or when the saved one cannot be read: start a new one.
    public async Task<Game?> Load() => StorageFormat.LoadGame(await storage.GetItem(Key));

    public Task Save(Game game) => storage.SetItem(Key, StorageFormat.SaveGame(game));

    public Task Clear() => storage.RemoveItem(Key);
}

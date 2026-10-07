using Molkky.Web.Infrastructure;

namespace Molkky.Web.Tests;

// localStorage in memory, with every write recorded.
public sealed class FakeLocalStorage : ILocalStorage
{
    public Dictionary<string, string> Items { get; } = [];

    // The keys written or removed, in order.
    public List<string> Writes { get; } = [];

    public Task<string?> GetItem(string key) => Task.FromResult(Items.GetValueOrDefault(key));

    public Task SetItem(string key, string value)
    {
        Items[key] = value;
        Writes.Add(key);
        return Task.CompletedTask;
    }

    public Task RemoveItem(string key)
    {
        Items.Remove(key);
        Writes.Add(key);
        return Task.CompletedTask;
    }
}

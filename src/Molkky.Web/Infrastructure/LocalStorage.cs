using Microsoft.JSInterop;

namespace Molkky.Web.Infrastructure;

// String storage that outlives the tab. The app's keys start with "molkky." because every
// project on siudzinski.github.io shares one localStorage.
public interface ILocalStorage
{
    Task<string?> GetItem(string key);
    Task SetItem(string key, string value);
    Task RemoveItem(string key);
}

// The browser's localStorage. When the browser refuses (storage disabled or full), reads find
// nothing and writes are skipped: the app keeps working, it just does not remember.
public sealed class BrowserLocalStorage(IJSRuntime jsRuntime, ILogger<BrowserLocalStorage> logger) : ILocalStorage
{
    public async Task<string?> GetItem(string key)
    {
        try
        {
            return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (JSException e)
        {
            logger.LogWarning(e, "Could not read {Key} from localStorage", key);
            return null;
        }
    }

    public async Task SetItem(string key, string value)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (JSException e)
        {
            logger.LogWarning(e, "Could not save {Key} to localStorage", key);
        }
    }

    public async Task RemoveItem(string key)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (JSException e)
        {
            logger.LogWarning(e, "Could not remove {Key} from localStorage", key);
        }
    }
}

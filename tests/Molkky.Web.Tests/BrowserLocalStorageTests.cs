using Bunit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Molkky.Web.Infrastructure;

namespace Molkky.Web.Tests;

public class BrowserLocalStorageTests : BunitContext
{
    private BrowserLocalStorage Storage => new(JSInterop.JSRuntime, NullLogger<BrowserLocalStorage>.Instance);

    [Fact]
    public async Task Uses_the_browsers_localStorage()
    {
        JSInterop.Setup<string?>("localStorage.getItem", "molkky.game").SetResult("saved");
        JSInterop.SetupVoid("localStorage.setItem", "molkky.game", "new").SetVoidResult();
        JSInterop.SetupVoid("localStorage.removeItem", "molkky.game").SetVoidResult();

        Assert.Equal("saved", await Storage.GetItem("molkky.game"));
        await Storage.SetItem("molkky.game", "new");
        await Storage.RemoveItem("molkky.game");

        JSInterop.VerifyInvoke("localStorage.setItem");
        JSInterop.VerifyInvoke("localStorage.removeItem");
    }

    // E.g. storage disabled for the site, or full: the app keeps going without remembering.
    [Fact]
    public async Task A_storage_the_browser_refuses_reads_as_empty_and_does_not_throw()
    {
        JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetException(new JSException("SecurityError"));
        JSInterop.SetupVoid("localStorage.setItem", _ => true).SetException(new JSException("QuotaExceededError"));
        JSInterop.SetupVoid("localStorage.removeItem", _ => true).SetException(new JSException("SecurityError"));

        Assert.Null(await Storage.GetItem("molkky.game"));
        await Storage.SetItem("molkky.game", "new");
        await Storage.RemoveItem("molkky.game");
    }
}

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Molkky.Web.Infrastructure;
using MudBlazor.Services;

namespace Molkky.Web.Tests;

// Base class for component tests: registers what the app registers in Program.cs.
public abstract class MudBlazorTestContext : BunitContext
{
    protected MudBlazorTestContext()
    {
        // MudBlazor components call into JavaScript; let every call succeed with a default result.
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddSingleton<Translator>();
    }
}

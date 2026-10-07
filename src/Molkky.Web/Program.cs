using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Molkky.Web;
using Molkky.Web.Infrastructure;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<ILocalStorage, BrowserLocalStorage>();
builder.Services.AddSingleton<GameStore>();
builder.Services.AddSingleton<SettingsStore>();
builder.Services.AddSingleton<Translator>();
builder.Services.AddSingleton(Random.Shared);
builder.Services.AddMudServices();

await builder.Build().RunAsync();

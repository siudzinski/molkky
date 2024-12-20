using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using molkky;
using molkky.Infrastructure;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddTransient<GameSessionStorage>();
builder.Services.AddTransient<SettingsSessionStorage>();
builder.Services.AddSingleton<Translator>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();

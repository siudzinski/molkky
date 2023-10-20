using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using molkky;
using molkky.Infrastructure;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var baseAddress = builder.HostEnvironment.IsDevelopment() 
    ? new Uri(builder.HostEnvironment.BaseAddress)
    : new Uri("https://siudzinski.github.io/molkky/");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseAddress });
builder.Services.AddTransient<GameSessionStorage>();

await builder.Build().RunAsync();

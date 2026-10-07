#:sdk Microsoft.NET.Sdk.Web

// Serves the GitHub Pages publish output the way GitHub Pages does: under /molkky/, with
// 404.html (a copy of index.html, so the Blazor router takes over) for unknown paths.
//
// Publish first, then from the repo root:
//   dotnet run --file tools/serve-ghpages/serve.cs [-- <site dir> <port>]
// Defaults: <repo>/publish/wwwroot on port 5300.
//
// Use `--file`: without it `dotnet run` builds a project from the working directory instead.
// Keep this file alone in its folder: the Web SDK compiles any .razor files next to it.

using Microsoft.Extensions.FileProviders;

const string BasePath = "/molkky";

var scriptDirectory = AppContext.GetData("EntryPointFileDirectoryPath") as string;
var repoRoot = scriptDirectory is null
    ? Directory.GetCurrentDirectory()
    : Path.GetFullPath(Path.Combine(scriptDirectory, "..", ".."));
var siteRoot = Path.GetFullPath(args.Length > 0 ? args[0] : Path.Combine(repoRoot, "publish", "wwwroot"));
var port = args.Length > 1 ? args[1] : "5300";

if (!File.Exists(Path.Combine(siteRoot, "404.html")))
{
    Console.Error.WriteLine($"No 404.html in {siteRoot}.");
    Console.Error.WriteLine("Publish first: dotnet publish src/Molkky.Web -c Release -o publish -p:GHPages=true");
    return 1;
}

var builder = WebApplication.CreateSlimBuilder();
builder.WebHost.UseUrls($"http://localhost:{port}");
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
var app = builder.Build();
var site = new PhysicalFileProvider(siteRoot);

app.Use(async (context, next) =>
{
    // GitHub Pages serves the project site only under /molkky/.
    if (context.Request.Path == "/")
    {
        context.Response.Redirect($"{BasePath}/");
        return;
    }

    // Always revalidate, so a fresh publish shows up on reload.
    context.Response.Headers.CacheControl = "no-cache";
    await next();
});

app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = site, RequestPath = BasePath });
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = site,
    RequestPath = BasePath,
    // Serve .br, .dat and friends as-is; the Brotli loader decompresses in the browser.
    ServeUnknownFileTypes = true,
});

app.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(site.GetFileInfo("404.html"));
});

Console.WriteLine($"Serving {siteRoot} at http://localhost:{port}{BasePath}/");
await app.RunAsync();
return 0;

# Mölkky score keeper

Blazor WebAssembly (.NET 10) app for keeping score in Mölkky, served as a static site from GitHub Pages at https://siudzinski.github.io/molkky/. Goals, decisions, known issues and the phase checklists live in [docs/PLAN.md](docs/PLAN.md); tick its boxes in the same PR that does the work.

## Layout

- `src/Molkky.Domain`: the game rules (`Game`, `Player`, rule variants, storage records). Plain C#.
- `src/Molkky.Web`: the Blazor app. `Pages/`, `Shared/`, and `Infrastructure/` for sessionStorage (JS interop) and translations.
- `tests/Molkky.Domain.Tests`: xUnit v3 tests of the rules.
- `tests/Molkky.Web.Tests`: bUnit component tests. Derive from `MudBlazorTestContext`, which registers MudBlazor and the `Translator`.
- `tools/serve-ghpages/serve.cs`: single-file static server that serves the publish output like GitHub Pages.

## Commands (from the repo root)

- Build: `dotnet build`
- Test: `dotnet test`
- Format: `dotnet format` (CI runs `dotnet format --verify-no-changes`)
- Run: `dotnet run --project src/Molkky.Web --launch-profile http` → http://localhost:5295 (`molkky-dev` in `.claude/launch.json`)
- Publish for GitHub Pages: `dotnet publish src/Molkky.Web -c Release -o publish -p:GHPages=true`
- Serve that publish like GitHub Pages: `dotnet run --file tools/serve-ghpages/serve.cs` → http://localhost:5300/molkky/ (`molkky-ghpages` in `.claude/launch.json`; publish again after changes)

## Rules

- The domain stays free of UI: no Blazor, JS interop or MudBlazor in `Molkky.Domain`. The project has no such references and `DomainDependencyTests` guards it.
- Every rule change gets a test in `Molkky.Domain.Tests`, written first and seen red.
- Re-sorting players by lowest score after each round is a deliberate house rule, not the official fixed order. Keep it and its tests.
- Every visible string is a `required` property of `Texts` in `Infrastructure/Translations.cs`, set in both languages (a missing one fails the build). Components read it as `@Text.Name` (from `TranslatableComponentBase`), other code as `Translator.Text.Name`.
- MudBlazor is pinned at 6.19.1 until phase 4; its API changed a lot since, so follow how this repo already uses it. Persistence is sessionStorage until phase 3.
- Package versions live only in `Directory.Packages.props`; a csproj `PackageReference` has no `Version`.
- Warnings are errors (`Directory.Build.props`).

## Gotchas

- Tests run on Microsoft Testing Platform (`global.json`), not VSTest. Filter one project at a time, e.g. `dotnet test --project tests/Molkky.Domain.Tests --filter-class "*GameTests"`; a filter across the solution fails on the project where nothing matches.
- Line endings are LF everywhere (`.gitattributes`, `.editorconfig`) because CI runs on Linux.
- The GitHub Pages publish takes `<base href="/molkky/">` from the git `origin` remote and copies `index.html` to `404.html` for deep links. After touching `index.html`, routing or the publish, check that `publish/wwwroot/index.html` still has that base href and that `404.html` exists, then play it through `molkky-ghpages`.
- `serve.cs` runs with `dotnet run --file` (plain `dotnet run` picks up a project in the working directory) and stays alone in its folder (the Web SDK compiles any `.razor` files next to it).

## Done means

1. `dotnet build` with 0 warnings, `dotnet test` green, `dotnet format --verify-no-changes` clean.
2. UI changes checked in the browser preview at phone width (375 px).
3. `docs/PLAN.md` updated when the work is on the plan.

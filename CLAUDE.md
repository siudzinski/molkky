# Mölkky score keeper

Blazor WebAssembly (.NET 10) app for keeping score in Mölkky: an installable PWA, served as a static site from GitHub Pages at https://siudzinski.github.io/molkky/. Goals, decisions, known issues and the phase checklists live in [docs/PLAN.md](docs/PLAN.md); tick its boxes in the same PR that does the work.

## Layout

- `src/Molkky.Domain`: the game rules. Plain C#. `Game` (settings + starting order + throws, everything else computed by replaying them), `Player` (one player's computed state), `Rules/` (one strategy class per setting variant, and the re-sort house rule), `Storage/` (the stored JSON formats).
- `src/Molkky.Web`: the Blazor app, styled with Tailwind CSS v4.
  - `Pages/`: the routable pages.
  - `Components/`: generic UI building blocks (`Button`, `IconButton`, `Card`, `Avatar`, `SegmentedToggle`, `TextField`, `Sheet`, `LineChart`). They know nothing of the game or `Texts`: the caller passes the strings.
  - `Shared/`: the app's own pieces (`MainLayout`, `AppBar`, `ScorePad`, `Scoreboard`, `ResumeGamePrompt`, `ChangePlayersSheet`, `PlayerColors`, ...).
  - `Infrastructure/`: localStorage (JS interop), the game, settings and theme stores, and translations (Polish by default).
  - `Styles/app.css`: the Tailwind input (palette, font, base styles). The build turns it into `wwwroot/css/app.css`, which is generated and git-ignored.
- `tests/Molkky.Domain.Tests`: xUnit v3 tests of the rules.
- `tests/Molkky.Web.Tests`: bUnit component tests. Derive from `AppTestContext`, which registers what `Program.cs` does, with an in-memory `FakeLocalStorage` and bUnit's strict JS interop: a test sets up every JavaScript call its component makes (see `DocumentLanguageTests`). The context starts with English saved, so tests read the English texts; a fresh start is in Polish.
- `tools/serve-ghpages/serve.cs`: single-file static server that serves the publish output like GitHub Pages.

## Commands (from the repo root)

- Once, and after `package-lock.json` changes: `npm ci` (Node version in `.nvmrc`). Every build of the Web project, tests included, runs the Tailwind CLI from `node_modules`.
- Build: `dotnet build`
- Test: `dotnet test`
- Format: `dotnet format` (CI runs `dotnet format --verify-no-changes`)
- Run: `dotnet run --project src/Molkky.Web --launch-profile http` → http://localhost:5295 (`molkky-dev` in `.claude/launch.json`)
- Hot reload: `molkky-watch` (dotnet watch on :5295) together with `molkky-css-watch` (regenerates the CSS when a `.razor` or `.cs` file changes) in `.claude/launch.json`; the VS Code task `watch` starts both.
- Publish for GitHub Pages: `dotnet publish src/Molkky.Web -c Release -o publish -p:GHPages=true`
- Serve that publish like GitHub Pages: `dotnet run --file tools/serve-ghpages/serve.cs` → http://localhost:5300/molkky/ (`molkky-ghpages` in `.claude/launch.json`; publish again after changes)

## Rules

- The domain stays free of UI: no Blazor, JS interop or UI library in `Molkky.Domain`. The project has no such references and `DomainDependencyTests` guards it.
- Every rule change gets a test in `Molkky.Domain.Tests`, written first and seen red.
- Re-sorting players by lowest score after each round is a deliberate house rule (`Rules/LowestScoreThrowsFirst`), not the official fixed order. Keep it and its tests.
- A game stores only its settings, starting order and throws. Never store computed state (scores, round, order); change how it is computed in `Game` or `Rules/`. Undo drops the last throw (`Game.Undo`) and is saved like a throw.
- Every visible string, aria-labels included, is a `required` property of `Texts` in `Infrastructure/Translations.cs`, set in both languages (a missing one fails the build). Components read it as `@Text.Name` (from `TranslatableComponentBase`), other code as `Translator.Text.Name`.
- Colours are the palette tokens in `Styles/app.css` (`bg-lawn`, `bg-surface`, `text-ink`, `text-muted`, `bg-primary`, `bg-birch`, ...). Each token has a light and a dark value, so components use them without `dark:` variants; a new colour is a new token with both values. Player colours are the tokens `player-1` … `player-8` (with `on-player` for the initial), which `PlayerColors` maps from a palette index. The light or dark values follow `data-theme` on `<html>`, as does `dark:`.
- Tailwind generates only the classes it finds in the `.razor` and `.cs` files: write every class name whole in the source (a whole string per case of a switch or const), never assembled from parts.
- One utility per CSS property on an element: of `px-0` and `px-4` on one element, the stylesheet's order decides, not the class order. Give the component a parameter instead (like `Button`'s `IconOnly`).
- Touch targets are at least 44 px (`min-h-11 min-w-11`). An icon-only button is an `IconButton`, whose required `Label` becomes its aria-label. `AccessibilityTests` checks every screen; add a new screen there.
- Ids and radio-group names come from `ElementIds.Next`: a static counter in a generic component is one per type argument, so two of them would share names.
- Persistence is localStorage under `molkky.*` keys, each value a JSON document with a version (`{ "version": 1, ... }`): game and settings in `Molkky.Domain/Storage/StorageFormat.cs`, language in `Translator`, theme in `ThemeStore`. Missing, unreadable or other-version data loads as nothing (a fresh start), never an exception. A format change is a new version that still loads the old one, with a test that loads an old document.
- Save when something changes (a throw, an undo, a settings, language or theme change), never in `OnAfterRenderAsync`. Navigate only after the save.
- Package versions live only in `Directory.Packages.props`; a csproj `PackageReference` has no `Version`. npm packages are pinned to exact versions in `package.json`.
- Warnings are errors (`Directory.Build.props`).

## Gotchas

- Tests run on Microsoft Testing Platform (`global.json`), not VSTest. Filter one project at a time, e.g. `dotnet test --project tests/Molkky.Domain.Tests --filter-class "*GameTests"`; a filter across the solution fails on the project where nothing matches.
- Line endings are LF everywhere (`.gitattributes`, `.editorconfig`) because CI runs on Linux.
- The GitHub Pages publish takes `<base href="/molkky/">` from the git `origin` remote and copies `index.html` to `404.html` for deep links. After touching `index.html`, routing, the service worker or the publish, check that `publish/wwwroot/index.html` still has that base href and that `404.html` exists, then play it through `molkky-ghpages`, offline too (load it, stop the server, reload).
- Every project on `siudzinski.github.io` shares one origin, so one localStorage and one Cache Storage: keep the `molkky.` key prefix and the `molkky-offline-` cache prefix. To see a fresh start in the browser, clear those keys (or use a private window).
- The service worker (`wwwroot/service-worker.published.js`; development gets the empty `service-worker.js`) serves `_framework/`, which is fingerprinted, from its cache and everything else from the network first, so a reload picks up a new deploy. It caches only the extensions in `offlineAssetsInclude`: a new kind of asset goes in that list, or the app misses it offline.
- The theme is applied by the inline script at the top of `wwwroot/index.html` before Blazor starts: it reads `molkky.theme` itself, sets `data-theme` and the `theme-color` meta, and exposes `molkkyTheme.apply` for `ThemeStore`. A change to the theme document changes both, and `ThemeStoreTests` pins the stored JSON. The browser pane's colour-scheme emulation fires no `change` event, so check "System" following the system with a reload.
- `dotnet build` or `dotnet test` while `molkky-watch` runs replaces the framework files the watch serves, and the page then fails with 404s: restart `molkky-watch` after a build.
- The icons in `wwwroot/icons/*.png` are rendered from `icon.svg` and `icon-maskable.svg`: a headless Edge or Chrome screenshot at 512 px (`--headless=new --window-size=512,512 --default-background-color=00000000 --screenshot=<png> file:///<svg>`), scaled down for the smaller sizes. Render them again after changing an SVG.
- In the built-in browser pane, a screenshot taken inside a batch can show the frame before the last render: read state with `get_page_text` or DOM queries.
- `serve.cs` runs with `dotnet run --file` (plain `dotnet run` picks up a project in the working directory) and stays alone in its folder (the Web SDK compiles any `.razor` files next to it).

## Done means

1. `dotnet build` with 0 warnings, `dotnet test` green, `dotnet format --verify-no-changes` clean.
2. UI changes checked in the browser preview at phone width (375 px), in light and dark.
3. `docs/PLAN.md` updated when the work is on the plan.

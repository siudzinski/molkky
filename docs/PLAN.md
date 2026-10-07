# Upgrade plan

Living plan for modernising the Mölkky score keeper. Each phase is one branch and one PR, and the live site must keep working after every merge. Update the checkboxes in this file in the same PR that does the work.

## Goals

- Run on .NET 10.
- Keep free, simple hosting on GitHub Pages (`https://siudzinski.github.io/molkky/`).
- Modern, mobile-first UI.
- A codebase that Claude Code can extend safely: clear structure, tests, written conventions, CI gates.

## Decisions

| Date | Decision | Why |
|---|---|---|
| 2026-10-07 | Target .NET 10 | LTS, supported until Nov 2028; suits a project touched rarely. |
| 2026-10-07 | Stay on Blazor WebAssembly | The native .NET option for a static site. Uno / Avalonia-WASM / MAUI are heavier and worse on the web. |
| 2026-10-07 | UI: Tailwind CSS v4 + own Razor components; drop MudBlazor | Modern look; Tailwind is what models handle best; components live in the repo, so there is no third-party API to misremember (MudBlazor changed a lot between v6 and v9); smaller download. |
| 2026-10-07 | Keep re-sorting players by score (lowest first) after each round | Deliberate house rule, not the official fixed order. Must stay explicit and tested. |
| 2026-10-07 | Move persistence from `sessionStorage` to `localStorage` | A game survives closing the tab; settings and language are remembered. |
| 2026-10-07 | Deploy with the official Pages actions (`upload-pages-artifact` + `deploy-pages`) | Built-in token, no personal access token to expire, no `gh-pages` branch. |

## Known issues (found during review, 2026-10-07)

- **Bug:** with the "3 misses → back to zero" setting, `Player.AddPoints` sets the score to 0 but appends the *previous* total to the score history, so the endgame chart is wrong from that throw onwards. Fix test-first in phase 2.
- `wwwroot/js/sessionStorage.js` is a no-op (`window.sessionStorage` cannot be reassigned); if it ever took effect, `getItem` would recurse forever. Delete in phase 2.
- The domain stores MudBlazor colour names (`ColorProvider`) that the UI parses back with `Enum.Parse(typeof(Color), …)`. Store a palette index instead (phase 3).
- `Gameplay` and `Settings` save state in `OnAfterRenderAsync`, i.e. on every render. Save on change instead (phase 3).
- `index.html` sets `user-scalable=no`, which blocks zoom (accessibility). Remove in phase 4.

## Phase 1: .NET 10 and deploy pipeline (no behaviour change)

- [x] `net10.0`, `Microsoft.AspNetCore.Components.WebAssembly*` 10.0.12, `PublishSPAforGitHubPages.Build` 3.0.3 (pulls in the Brotli loader package).
- [x] `global.json` pinning SDK 10.0.x (`rollForward: latestFeature`).
- [x] `molkky.sln` → `molkky.slnx`.
- [x] `index.html` on the .NET 10 template: fingerprinted `blazor.webassembly#[.{fingerprint}].js`, `webassembly` preload link, import map, `OverrideHtmlAssetPlaceholders`.
- [x] MudBlazor left at 6.19.1 (replaced in phase 4).
- [x] `.github/workflows/deploy.yml` (official Pages actions) and `ci.yml` (publish on every PR).
- [x] `.claude/launch.json` with a `molkky-dev` server so Claude can preview the app (pulled forward from phase 2).
- [x] Verified locally: published output served under `/molkky/` (base href, Brotli loading, deep-link reload via `404.html`, full game to endgame, settings, language switch) and the dev server.
- [x] Repo Settings → Pages → Source: **GitHub Actions** (switched 2026-10-07; was "Deploy from a branch: gh-pages").
- [ ] Merge, confirm the deploy run succeeds and the live site works.
- [ ] Clean up: delete the `gh-pages` branch and the `DEPLOY_TOKEN` secret.

## Phase 2: structure and safety net

- [ ] Split into projects:
  - `src/Molkky.Domain`: plain C#, no Blazor or JS interop references (the compiler enforces it).
  - `src/Molkky.Web`: the Blazor app.
  - `tests/Molkky.Domain.Tests` (xUnit v3) and `tests/Molkky.Web.Tests` (bUnit).
- [ ] PascalCase root namespace (`Molkky.*`), `Directory.Build.props`, central package management (`Directory.Packages.props`), `.editorconfig`.
- [ ] Characterisation tests for the current rules first: scoring, exceeding 50 (both variants), 3 misses (both variants), elimination, last player standing wins, re-sort by lowest score after each round, play again.
- [ ] Fix the score-history bug test-first.
- [ ] Delete `wwwroot/js/sessionStorage.js`.
- [ ] `CLAUDE.md`:
  - what the app is, the folder layout, and the commands (build, test, run, format, publish);
  - the rules: the domain has no UI dependencies, every rule change gets a test, every visible string is translated;
  - what "done" means: build, tests and format pass, and UI changes are checked in the preview at phone width.
- [ ] `.claude/settings.json` allowlist for `dotnet build`, `dotnet test` and `dotnet format`.
- [ ] VS Code (the editor used for this repo): `.vscode/extensions.json` recommending C# Dev Kit, plus `tasks.json`/`launch.json` for run and test.
- [ ] CI gates on PRs: build, test, `dotnet format --verify-no-changes`, publish.
- [ ] Update workflow paths for the new project location.

## Phase 3: game model rework

- [ ] A game becomes *settings + player order + list of throws*. Scores, misses, eliminations, round, current player, winner and chart data are all computed by replaying the throws.
  - Undo = drop the last throw.
  - The score-history bug class goes away.
  - Stats come for free.
- [ ] Rule variants as small strategy classes (replaces the two `TODO inject strategy` comments in `Player.cs`). The re-sort house rule is a named, tested rule.
- [ ] Persistence:
  - `localStorage` with a versioned schema (`{ "version": 1, ... }`) so later changes can migrate old data;
  - "Resume game?" on start;
  - save after each throw, not on every render.
- [ ] Typed translations: one object per language with `required` properties, so a missing string fails the build.
- [ ] Player colours stored as a palette index, not UI-library names.

## Phase 4: UI rebuild (Tailwind)

- [ ] Tailwind CSS v4:
  - via `@tailwindcss/cli` (Node is already on dev machines and GitHub runners);
  - an MSBuild target generates the CSS before build;
  - a watch mode for development, added to `.claude/launch.json`.
- [ ] Own component set in the Web project (button, card, avatar, dialog or bottom sheet, segmented toggle, text field, app bar or menu, SVG line chart). Remove MudBlazor.
- [ ] Mobile-first layout that follows the system light/dark setting.
- [ ] Score pad laid out like the pin formation (7-9-8 / 5-11-12-6 / 3-10-4 / 1-2) plus a big "Miss" button. Instant entry with Undo replaces the confirm-every-throw dialog.
- [ ] PWA: installable and works offline.
  - Add `.br` to the offline assets in `service-worker.published.js` (required by PublishSPAforGitHubPages).
  - Self-host fonts, because the Google Fonts link does not work offline.
- [ ] Remove `user-scalable=no`.
- [ ] bUnit tests for the score pad and the scoreboard.

## Later: feature backlog

- Saved player roster
- Game history and per-player stats across games
- Custom target score
- Team play
- Shareable result card
- More languages

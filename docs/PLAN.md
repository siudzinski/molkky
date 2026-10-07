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
| 2026-10-07 | Tests on xUnit v3 with Microsoft Testing Platform (`test.runner` in `global.json`) | xunit.v3 4.x ships MTP v2 by default, and the .NET 10 SDK runs those projects through `dotnet test` only in MTP mode. |
| 2026-10-07 | LF line endings in the repo and every working tree (`.gitattributes` + `.editorconfig`) | CI runs on Linux; `dotnet format --verify-no-changes` must give the same answer on Windows. |
| 2026-10-07 | The current game rules are correct; the phase 3 model rework keeps them exactly | Phase 3 is about storage and structure. The phase 2 characterisation tests define the rules and keep their expected values. |

## Known issues (found during review, 2026-10-07)

- **Fixed in phase 2.** **Bug:** with the "3 misses → back to zero" setting, `Player.AddPoints` set the score to 0 but appended the *previous* total to the score history, so the endgame chart was wrong from that throw onwards. The history now records the score after every throw.
- **Fixed in phase 2.** `wwwroot/js/sessionStorage.js` was a no-op (`window.sessionStorage` cannot be reassigned); if it ever took effect, `getItem` would recurse forever. Deleted.
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
- [x] Merge, confirm the deploy run succeeds and the live site works (PR #1, 2026-10-07).
- [x] Clean up: delete the `gh-pages` branch and the `DEPLOY_TOKEN` secret (2026-10-07; revoking the personal access token itself is left to the account owner).

## Phase 2: structure and safety net

- [x] Split into projects:
  - `src/Molkky.Domain`: plain C#, no Blazor or JS interop references (the compiler enforces it).
  - `src/Molkky.Web`: the Blazor app.
  - `tests/Molkky.Domain.Tests` (xUnit v3) and `tests/Molkky.Web.Tests` (bUnit).
- [x] PascalCase root namespace (`Molkky.*`), `Directory.Build.props`, central package management (`Directory.Packages.props`), `.editorconfig`.
- [x] Characterisation tests for the current rules first: scoring, exceeding 50 (both variants), 3 misses (both variants), elimination, last player standing wins, re-sort by lowest score after each round, play again.
- [x] Fix the score-history bug test-first.
- [x] Delete `wwwroot/js/sessionStorage.js`.
- [x] `CLAUDE.md`:
  - what the app is, the folder layout, and the commands (build, test, run, format, publish);
  - the rules: the domain has no UI dependencies, every rule change gets a test, every visible string is translated;
  - what "done" means: build, tests and format pass, and UI changes are checked in the preview at phone width.
- [x] `.claude/settings.json` allowlist for `dotnet build`, `dotnet test` and `dotnet format`.
- [x] VS Code (the editor used for this repo): `.vscode/extensions.json` recommending C# Dev Kit, plus `tasks.json`/`launch.json` for run and test.
- [x] CI gates on PRs: build, test, `dotnet format --verify-no-changes`, publish.
- [x] Update workflow paths for the new project location.
- [x] `.gitattributes` with LF everywhere; `.claude/settings.local.json` git-ignored.
- [x] `tools/serve-ghpages/serve.cs`: .NET 10 single-file server for the publish output (under `/molkky/`, `404.html` fallback, `/` → `/molkky/`), as `molkky-ghpages` in `.claude/launch.json`.
- [x] Verified locally: 0 warnings, tests and format check green; game, settings and language survive a reload without `sessionStorage.js`; GHPages publish served under `/molkky/` (base href, Brotli loading, deep link via `404.html`) played to the endgame at phone width, with the chart dropping to 0 after 3 misses.

## Phase 3: game model rework (no rule changes)

The game rules are correct and do not change. This phase changes how a game is stored and computed, not what it computes.
- The phase 2 domain tests are the spec. Port them to the new API with every expected value unchanged, and remove or loosen none of them. Only the save/load test may change, because the storage format changes on purpose.
- Rules that live outside the domain today (shuffled starting order, at least 2 players, scores 0–12) get a test before the code around them changes.
- A test that seems to need a different expected value means the rework is wrong, not the rule.

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

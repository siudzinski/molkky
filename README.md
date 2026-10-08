# molkky

A simple **Blazor WebAssembly** application for keeping score in the Finnish game **Mölkky**.

## Features

- **Score pad laid out like the pins on the field**: a tap records the throw, Undo takes it back
- **Automatic scoring** by the Mölkky rules, with settings for going over 50 and for 3 misses in a row
- **Scoreboard** in throwing order, and a chart of every player's score at the end
- **Resume** an unfinished game after closing the app
- **English and Polish**, light and dark mode
- **Installable and works offline** (a PWA)

## Development

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and [Node.js](https://nodejs.org/) (for the Tailwind CSS build). Once, and after `package-lock.json` changes:

```bash
npm ci
```

Then:

```bash
dotnet run --project src/Molkky.Web
```

Every push to `master` is published to GitHub Pages by `.github/workflows/deploy.yml`. The modernisation roadmap lives in [docs/PLAN.md](docs/PLAN.md).

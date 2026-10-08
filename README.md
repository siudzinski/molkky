# molkky

A simple **Blazor WebAssembly** application for keeping score in the Finnish game **Mölkky**.

## Features

- **Add and manage multiple players**
- **Automatic point calculation** based on Mölkky rules, with customizable settings
- **Scoreboard** to display current game progress
- **Language support**: Available in both English and Polish
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

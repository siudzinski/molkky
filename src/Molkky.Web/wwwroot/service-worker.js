// In development the app is always fetched from the network: no offline support, so every change shows
// on the next reload. The published app gets service-worker.published.js instead (see Molkky.Web.csproj).
self.addEventListener('fetch', () => { });

// Offline support for the published app (GitHub Pages, under /molkky/). Based on Blazor's PWA template.
// Every URL here is relative to this script, so the base path never appears in it.

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall()));
self.addEventListener('activate', event => event.waitUntil(onActivate()));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

// Every project on siudzinski.github.io shares this origin's caches: name, and clean up, only molkky's.
const cacheNamePrefix = 'molkky-offline-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

// What the published app fetches. GitHub Pages does not serve Brotli itself, so the Brotli loader that
// PublishSPAforGitHubPages adds fetches the framework's .wasm and .dat files as .br, and the publish lists
// them that way in service-worker-assets.js: without .br here the app would not start offline.
const offlineAssetsInclude = [/\.html$/, /\.js$/, /\.css$/, /\.woff2$/, /\.png$/, /\.svg$/, /\.webmanifest$/, /\.br$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

const assetUrls = new Set(self.assetsManifest.assets.map(asset => new URL(asset.url, self.location).href));
const frameworkPath = new URL('_framework/', self.location).pathname;

// A slow or silent network (a weak signal on the field) should not keep the app from starting.
const networkTimeout = 3000;

async function onInstall() {
    // Take over as soon as this version is cached, instead of when every tab with the old one is closed.
    self.skipWaiting();

    const requests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    const cache = await caches.open(cacheName);
    await cache.addAll(requests);
}

async function onActivate() {
    const keys = await caches.keys();
    await Promise.all(keys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
    await self.clients.claim();
}

// _framework/ holds only fingerprinted files, which never change under the same name: the cache first.
// Everything else (index.html, CSS, fonts, icons, the manifest, the Brotli loader) keeps its name across
// deploys: the network first, so a reload after a deploy gets the new version, and the cache when the
// network fails or does not answer in time.
async function onFetch(event) {
    const request = event.request;
    if (request.method !== 'GET') return fetch(request);

    const cache = await caches.open(cacheName);
    const url = new URL(request.url);

    // Every page of the app is index.html (GitHub Pages serves it as 404.html for deep links).
    if (request.mode === 'navigate' && !assetUrls.has(url.href)) {
        return await fromNetwork('index.html') ?? await cache.match('index.html') ?? Response.error();
    }

    if (url.pathname.startsWith(frameworkPath)) {
        return await cache.match(request) ?? fetch(request);
    }

    return await fromNetwork(request) ?? await cache.match(request) ?? Response.error();
}

// The fresh response (revalidated past the HTTP cache), or null when offline or too slow.
async function fromNetwork(request) {
    try {
        return await fetch(request, { cache: 'no-cache', signal: AbortSignal.timeout(networkTimeout) });
    } catch {
        return null;
    }
}

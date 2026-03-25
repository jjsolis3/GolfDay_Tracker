const CACHE_NAME = 'golfday-v1';
const STATIC_ASSETS = [
    '/',
    '/css/site.css',
    '/js/site.js',
    'https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css',
    'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css',
    'https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js'
];

// Install: cache static assets
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(STATIC_ASSETS))
            .catch(() => {}) // Don't fail install if CDN unreachable
    );
    self.skipWaiting();
});

// Activate: delete old caches
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k)))
        )
    );
    self.clients.claim();
});

// Fetch: network-first with cache fallback for navigation; cache-first for static
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);

    // Skip non-GET, cross-origin (except CDN), and API calls
    if (request.method !== 'GET') return;

    // Cache-first for static assets (CSS/JS/images)
    if (url.pathname.match(/\.(css|js|woff2?|png|jpg|ico|svg)$/)) {
        event.respondWith(
            caches.match(request).then(cached => cached || fetch(request))
        );
        return;
    }

    // Network-first for HTML pages (keep content fresh)
    if (request.headers.get('accept')?.includes('text/html')) {
        event.respondWith(
            fetch(request).catch(() => caches.match(request) || caches.match('/'))
        );
        return;
    }
});

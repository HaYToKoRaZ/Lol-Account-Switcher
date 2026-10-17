/* ==============================================================================
   Lol Account Switcher - Service Worker (sw.js)
   PWA High-Speed Offline & Static Asset Cache
   Strategy: Cache-First for static assets, Stale-While-Revalidate for pages
   ============================================================================== */

const CACHE_NAME = 'las-cache-v4.1';
const STATIC_ASSETS = [
  './',
  'index.html',
  'tr/',
  'tr/index.html',
  'style.css',
  'script.js',
  'logo.webp',
  'logo-38.webp',
  'logo-26.webp',
  'logo.png',
  'favicon.ico',
  'app.ico',
  'apple-touch-icon.png',
  'site.webmanifest',
  'avatars/avatar-1.webp',
  'avatars/avatar-2.webp',
  'avatars/avatar-3.webp',
  'ranks/challenger.webp',
  'ranks/grandmaster.webp',
  'ranks/master.webp',
  'ranks/diamond.webp',
  'ranks/emerald.webp',
  'ranks/platinum.webp',
  'ranks/gold.webp',
  'ranks/silver.webp',
  'ranks/bronze.webp',
  'ranks/iron.webp',
  'ranks/unranked.webp'
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      return cache.addAll(STATIC_ASSETS);
    }).then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) => {
      return Promise.all(
        keys.map((key) => {
          if (key !== CACHE_NAME) {
            return caches.delete(key);
          }
        })
      );
    }).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const url = new URL(event.request.url);

  // Sadece GET istekleri önbelleğe alınır
  if (event.request.method !== 'GET') return;

  // Harici üçüncü taraf API'leri es geç
  if (url.origin !== self.location.origin && !url.hostname.includes('fonts.gstatic.com')) {
    return;
  }

  // Cache-First: Statik dosyalar için disk önbelleğini kullan
  event.respondWith(
    caches.match(event.request).then((cachedResponse) => {
      if (cachedResponse) {
        return cachedResponse;
      }
      return fetch(event.request).then((networkResponse) => {
        if (!networkResponse || networkResponse.status !== 200 || networkResponse.type !== 'basic') {
          return networkResponse;
        }
        const responseToCache = networkResponse.clone();
        caches.open(CACHE_NAME).then((cache) => {
          cache.put(event.request, responseToCache);
        });
        return networkResponse;
      });
    })
  );
});

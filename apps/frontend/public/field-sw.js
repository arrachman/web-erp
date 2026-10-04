/* Sales Lapangan service worker — cakupan hanya /app/sales/.
   Strategi: navigasi halaman field = network-first dengan fallback cache
   (shell tetap terbuka saat sinyal hilang); API tidak pernah di-cache. */
const CACHE = 'field-v1';
const SHELL = '/app/sales/field';

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE).then((cache) => cache.add(SHELL)).then(() => self.skipWaiting()),
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(self.clients.claim());
});

self.addEventListener('fetch', (event) => {
  const url = new URL(event.request.url);
  if (event.request.method !== 'GET') return;
  if (url.pathname.startsWith('/api/')) return; // API selalu jaringan
  if (!url.pathname.startsWith('/app/sales/')) return; // di luar cakupan: abaikan
  if (event.request.mode === 'navigate') {
    event.respondWith(
      fetch(event.request)
        .then((resp) => {
          const copy = resp.clone();
          caches.open(CACHE).then((cache) => cache.put(SHELL, copy));
          return resp;
        })
        .catch(() => caches.match(SHELL)),
    );
  }
});

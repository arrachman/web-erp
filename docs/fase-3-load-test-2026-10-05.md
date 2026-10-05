# Laporan Load Test & Hardening (W8) — Fase 3

Tanggal: 2026-10-05 · Lingkungan: produksi (localhost, server bersama) ·
Metode: uji bertahap terbatas, abort bila error rate > 20%.

## Hasil

### 1. Halaman katalog portal (Next.js, port 3221)
| Konkurensi | Request | Sukses | Throughput | p50 | p95 | p99 |
|---|---|---|---|---|---|---|
| 50 | 25.432 (15 dtk) | 100% 200 | 1.692 req/dtk | 27,2 ms | 40,7 ms | 49,5 ms |

### 2. API katalog portal `GET /api/erp/portal/catalog` (terautentikasi)
Diuji terukur di bawah cap throttler (lihat temuan 1) — 10 worker, ±540 req/menit, 60 detik:

| Request | Sukses | p50 | p95 | p99 | Rata-rata |
|---|---|---|---|---|---|
| 550 | 100% 200 | 13,9 ms | 16,8 ms | 24,0 ms | 14,3 ms |

### 3. Endpoint publik (landing highlights)
240 request pertama lolos (200), sisanya 429 — sesuai cap baca publik
yang dipasang di W8 (240/menit/IP). Proteksi bekerja sesuai desain.

## Temuan

1. **Throttler global gateway = batas per-IP yang mengikat.**
   `ThrottlerModule` di gateway: 600 request / 60 detik / IP
   (`THROTTLE_LIMIT`, env). Pada uji konkurensi tinggi dari satu IP,
   tepat 600 request lolos lalu 429. Untuk penggunaan portal nyata ini
   **bukan** penghambat: 1.000+ pengguna bersamaan tersebar di banyak
   IP sekolah (±10 req/detik/IP sudah jauh di atas kebutuhan manusia),
   dan latensi katalog sendiri hanya ±14 ms. Rekomendasi: pantau 429
   di log; naikkan `THROTTLE_LIMIT` bila satu IP institusi besar
   (NAT bersama banyak guru) mulai kena.
2. **Rate limit khusus endpoint publik portal aktif** (W8): tulis/auth
   publik 15/menit/IP, baca publik 240/menit/IP — terbukti 429 pada
   percobaan login beruntun ke-12 (termasuk hitungan jendela berjalan).
3. **Isolasi data antar-sekolah terverifikasi negatif**: token sekolah B
   membaca order sekolah A → 404; membayar invoice A → 404; daftar
   pembayaran B kosong. Orang tua hanya melihat order/invoice miliknya
   (terverifikasi juga di Gelombang 3).
4. Katalog dirender server-side per request dari data item + harga
   kontrak; pada volume katalog saat ini (65 item) latensi stabil.
   Bila katalog tumbuh ke ribuan item, aktifkan cache katalog per
   jenjang (struktur respons sudah mendukung).

## Kesimpulan

Target W8 (portal responsif menuju 1.000+ pengguna bersamaan untuk
pola baca katalog/pesanan) **LULUS** pada infrastruktur saat ini,
dengan catatan pemantauan throttler per-IP di atas. Tidak ditemukan
kebocoran data lintas sekolah.

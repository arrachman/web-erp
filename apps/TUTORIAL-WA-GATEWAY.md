# Tutorial Integrasi WhatsApp Gateway (wa-gateway + Backend + Frontend)

Panduan untuk mereplikasi fitur WhatsApp Althea di project lain. Paket kode:

| Zip | Isi |
| --- | --- |
| `wa-gateway.zip` | Service WhatsApp (Baileys, API meniru Fonnte) |
| `wa-api-gateway.zip` | Backend NestJS: modul WA, pairing device, monitor, schema Prisma |
| `wa-frontend.zip` | Frontend Next.js: wizard pairing QR, halaman notifikasi, banner koneksi |

## 1. Arsitektur

```
Browser (Next.js)  ->  Backend NestJS (:3203)  ->  wa-gateway (:3204)  ->  WhatsApp
                              ^                          |
                              +------ webhook status ----+
```

Aturan keras:
- Browser tidak pernah memanggil wa-gateway langsung. Semua lewat backend.
- wa-gateway hanya listen di localhost/jaringan internal.
- Account token dan device token hanya hidup di backend.
- Gunakan nomor WA Business khusus. Baileys adalah protokol tidak resmi, ada risiko ban.

## 2. Menjalankan wa-gateway

Env:

| Var | Default | Keterangan |
| --- | --- | --- |
| `WA_GATEWAY_PORT` | 3204 | Port HTTP |
| `WA_GATEWAY_ACCOUNT_TOKEN` | - | Wajib. `openssl rand -hex 32`. Harus sama dengan `SENTIWA_ACCOUNT_TOKEN` di backend |
| `WA_GATEWAY_WEBHOOK_URL` | - | Endpoint webhook backend, mis. `http://localhost:3203/api/clinic/wa/webhook` |
| `WA_GATEWAY_WEBHOOK_SECRET` | - | Opsional, dikirim sebagai header `X-Webhook-Secret` |
| `WA_GATEWAY_DATA_DIR` | `./data` | Wajib volume persisten (registry + creds sesi). Hilang = scan QR ulang |

Docker (produksi, host network):

```bash
docker build -t wa-gateway:latest wa-gateway/
docker run -d --name wa-gateway --network host --restart always \
  -e WA_GATEWAY_PORT=3204 \
  -e WA_GATEWAY_ACCOUNT_TOKEN="$TOKEN" \
  -e WA_GATEWAY_WEBHOOK_URL=http://localhost:3203/api/clinic/wa/webhook \
  -v /opt/althea-wa-gateway-data:/app/data \
  wa-gateway:latest
curl -s localhost:3204/health     # degraded:true = ada device ter-pair yang tidak connect
```

## 3. Kontrak API wa-gateway

Semua `POST`, body form-urlencoded atau JSON, header `Authorization: <token>`.
Respons HTTP 200 dengan `{status:true,...}` atau `{status:false,reason}`. Cek `status`, bukan HTTP code (kecuali validasi input = 400).

Level akun (`Authorization: <ACCOUNT_TOKEN>`):

| Endpoint | Body | Hasil |
| --- | --- | --- |
| `/get-devices` | - | `data[]: name, device, status, token, autoread` |
| `/add-device` | `name, device, autoread` | `token, device, name` (idempoten per nomor) |
| `/delete-device` | `device` | disconnect + hapus creds |

Level device (`Authorization: <DEVICE_TOKEN>`):

| Endpoint | Body | Hasil |
| --- | --- | --- |
| `/qr` | - | `url` (gambar QR). Sudah tersambung: `reason:'device already connect'`. Belum siap: `'QR belum siap, coba lagi'` (retry) |
| `/device` | - | `device_status: connect/disconnect` (untuk polling) |
| `/disconnect` | - | logout |
| `/update-device` | `name, device` | ubah label saja |
| `/send` | `target, message` | `id` |
| `/send-media` | JSON `target, file(base64), filename, mimetype, caption` | `id` (limit 20 MB) |

Webhook (gateway ke backend), JSON:
`{ id, sender, status: sent|delivered|read|failed, device, reason? }`.
`id` = message id dari `/send`. Backend memetakan ke `terkirim -> sampai -> dibaca`.

## 4. Backend (`wa-api-gateway.zip`)

Struktur utama (path relatif `apps/api-gateway`):

| Path | Peran |
| --- | --- |
| `src/clinic-wa/` | Modul inti: `ClinicWaService` (template, kirim, log, webhook), `ClinicWaController` (`/clinic/wa/*`), `providers/sentiwa.provider.ts` (client HTTP ke gateway), `providers/mock.provider.ts`, `queue/` (BullMQ), `wa-resend.service.ts`, `wa-env.util.ts` |
| `src/clinic-settings/` | Pairing device: controller `/clinic/settings/wa-devices*`, `wa-device-pairing.service.ts`, `wa-device-status.service.ts`, `sentiwa-http.helpers.ts`, DTO |
| `src/clinic-wa-monitor/` | Monitor koneksi + `GET /clinic/wa/connection-health` + email alert saat putus |
| `src/clinic-booking/wa-scheduler-trigger.controller.ts` | Trigger manual scheduler reminder |
| `prisma/schema/clinic.prisma` | Model `ClinicWaTemplate`, `ClinicWaLog` (ambil dua model ini saja) plus setting `waActiveDeviceToken`, `waSenderNumber` di `clinic_settings` |
| `prisma/migrations/*wa*` | Migrasi terkait |

Catatan: zip ini disaring per topik. File seperti `booking-notification.service.ts`, `booking-reminder.scheduler.ts`, dan `clinic-invoice.service.ts` memanggil `ClinicWaService` tetapi tidak disertakan (terlalu terikat domain klinik). Lihat cara pakainya di sana bila perlu.

Env backend:

```
SENTIWA_API_URL=http://localhost:3204
SENTIWA_ACCOUNT_TOKEN=<sama dengan WA_GATEWAY_ACCOUNT_TOKEN>
SENTIWA_WEBHOOK_URL=http://localhost:3203/api/clinic/wa/webhook
WA_QUEUE_ENABLED=true            # kirim lewat BullMQ (butuh Redis, REDIS_URL)
WA_DISCONNECT_ALERT_ENABLED=true # email alert saat device putus
# tuning anti-ban (opsional)
WA_THROTTLE_MAX=20  WA_THROTTLE_WINDOW_MS=60000  WA_WORKER_CONCURRENCY=1
WA_JITTER_MIN_MS=4000  WA_JITTER_MAX_MS=10000
```

Perilaku penting:
- Provider dipilih di `clinic-wa.module.ts`: ada `SENTIWA_ACCOUNT_TOKEN` (atau `SENTIWA_API_TOKEN`) maka `SentiWaProvider`, jika tidak maka `MockWAProvider`. Env kosong = WA berhenti kirim secara senyap, jadi cek log saat boot.
- Device token aktif dibaca dari DB (`clinic_settings.waActiveDeviceToken`), bukan env.
- Queue: 3 percobaan, backoff tetap 5 menit; throttle + jitter acak antar pesan.
- Endpoint webhook publik (tanpa JWT). Saat ini header `X-Webhook-Secret` belum divalidasi di backend. Untuk project baru, tambahkan pengecekan secret di `ClinicWaController.webhook`.
- Setelah menambah model Prisma: `npx prisma generate` lalu `npx prisma migrate deploy`.

Tabel endpoint admin (RBAC admin):

| Method | Path | Fungsi |
| --- | --- | --- |
| GET | `/clinic/settings/wa-devices` | list device + `isActive` |
| POST | `/clinic/settings/wa-devices` | tambah device |
| POST | `/clinic/settings/wa-devices/qr` | `{qrUrl?, alreadyConnected}` (`force` = disconnect dulu) |
| POST | `/clinic/settings/wa-devices/check` | `{connected, devicePhone?, deviceName?}` |
| POST | `/clinic/settings/wa-devices/activate` | simpan device aktif |
| POST | `/clinic/settings/wa-devices/update` | edit label |
| DELETE | `/clinic/settings/wa-devices/:phone` | hapus |
| GET | `/clinic/settings/wa-status` | status device aktif |
| GET/POST/PATCH/DELETE | `/clinic/wa/template[/:id]` | CRUD template (Mustache `{{var}}`) |
| GET | `/clinic/wa/log`, `/clinic/wa/stats` | log dan statistik |
| POST | `/clinic/wa/log/:id/resend`, `/clinic/wa/log/resend` | kirim ulang |
| POST | `/clinic/wa/send-test` | kirim tes |
| POST | `/clinic/wa/webhook` | penerima status dari gateway |
| GET | `/clinic/wa/connection-health` | kesehatan koneksi |

## 5. Frontend (`wa-frontend.zip`)

Path relatif `apps/web-althea`:

| Path | Peran |
| --- | --- |
| `features/admin-pengaturan/ui/tabs/notifikasi/` | Wizard pairing: `form-step`, `scan-step`, `done-step`, `wa-device-pairing-drawer`, `wa-device-edit-drawer`, `wa-connection-section` |
| `features/admin-pengaturan/api/wa-device.api.ts` | Client API pairing (`waDeviceApi`) |
| `features/admin-pengaturan/hooks/` | `use-wa-devices`, `use-wa-device-status`, `use-wa-template-recipients` |
| `features/admin-notif-wa/` | Halaman notifikasi: template editor/list, activity log, statistik, kirim tes |
| `features/wa-connection-health/` | Banner "perangkat terputus" + indikator status (dengan test) |

Alur wizard pairing:
1. Form: nama, nomor WA, autoread (default off). Submit ke `POST wa-devices` mendapat `deviceToken`.
2. Scan QR: panggil `.../qr`, tampilkan `qrUrl` di `<img>`.
   - QR di-refresh otomatis tiap sekitar 8 detik (QR WhatsApp cepat basi; scan QR basi ditolak HP).
   - Polling `.../check` tiap 4 detik (`STATUS_POLL_INTERVAL_MS` di `scan-step.tsx`).
   - `connected: true` berarti hentikan polling, panggil `activate`, tampilkan `done-step`.
   - Bersihkan interval saat drawer ditutup.
3. `alreadyConnected: true` berarti lewati QR, langsung aktivasi.

Dependensi yang tidak ikut zip dan perlu disesuaikan: `@/lib/api-client`, komponen UI umum, design tokens (`var(--teal-700)` dll), React Query provider, dan topbar admin (`desktop-topbar.tsx`, `mobile-topbar.tsx` hanya contoh tempat menaruh indikator status).

## 6. Urutan implementasi di project baru

1. Deploy wa-gateway (bagian 2), cek `/health`.
2. Backend: tambah model Prisma + migrasi, salin modul `clinic-wa`, `clinic-wa-monitor`, bagian pairing di `clinic-settings`; set env (bagian 4); pastikan Redis tersedia bila `WA_QUEUE_ENABLED=true`.
3. Frontend: salin fitur, sambungkan ke api-client, tempatkan wizard di halaman Pengaturan dan banner di layout admin.
4. Pairing lewat UI: tambah device, scan QR dari WhatsApp (Linked devices, Link a device), tunggu `connect`, aktifkan.
5. Acceptance test: kirim tes, pesan masuk, log berubah `terkirim -> sampai -> dibaca`; putuskan koneksi, banner "terputus" muncul.

## 7. Operasional

- Disconnect sementara (503/405/515/408) normal; gateway reconnect sendiri (backoff eksponensial + watchdog 60 dtk). Hanya bila creds dicabut (401/403/419) perlu scan QR ulang.
- Gateway sengaja memakai `markOnlineOnConnect:false` dan menegaskan presence offline berkala supaya notifikasi di HP pemilik tetap bunyi. Jangan diubah.
- Backup volume `WA_GATEWAY_DATA_DIR`; jangan commit isinya (creds login WA).
- Kirim massal harus lewat antrean; jangan mengandalkan throttle gateway saja.
- Detail insiden dan ketahanan koneksi: `wa-gateway/README.md` dan `wa-gateway/INSIDEN-2026-07-29.md`.

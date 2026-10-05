# @althea/wa-gateway

Self-hosted WhatsApp gateway berbasis [Baileys](https://github.com/WhiskeySockets/Baileys)
yang **meniru API Fonnte**. Tujuannya jadi _drop-in replacement_: `api-gateway` tidak
perlu diubah — cukup arahkan `SENTIWA_API_URL` ke service ini.

## Kenapa ada

Saat WhatsApp tertaut ke gateway QR (Fonnte / linked device apa pun), notifikasi di HP
(banner/suara/getar) bisa ter-_suppress_ karena WhatsApp menganggap "ada device aktif".
Fonnte tidak mengekspos kontrol untuk ini. Gateway ini menyalakan lever Baileys
`markOnlineOnConnect: false` (+ tidak kirim read receipt, tidak subscribe presence) supaya
HP tetap jadi target push notification → notif WA Business tetap masuk normal.

> ⚠️ `markOnlineOnConnect: false` **saja tidak cukup**. Pada pairing baru / reconnect
> 515, pushName belum termuat saat `connection: open`, jadi presence `unavailable`
> bawaan Baileys di-skip; lalu handler `creds.update` mengirim `<presence name>` tanpa
> type yang membuat device dianggap **online** → notif HP ter-suppress lagi. Gateway
> me-_re-assert_ `sendPresenceUpdate('unavailable')` setelah name termuat (delay ~1.5s
> pasca-`open`) dan secara berkala (60s) agar device tetap offline. Lihat
> `assertOffline()` di `src/session-manager.ts`.

## Kompatibilitas API Fonnte

Endpoint yang ditiru (semua `POST`, body `x-www-form-urlencoded`, header `Authorization: <token>`):

| Endpoint          | Auth          | Body                     | Status |
| ----------------- | ------------- | ------------------------ | ------ |
| `/get-devices`    | account token | —                        | ✅ langkah 1–2 |
| `/add-device`     | account token | `name, device, autoread` | ✅ langkah 1–2 |
| `/delete-device`  | account token | `device`                 | ✅ langkah 1–2 |
| `/qr`             | device token  | —                        | ✅ langkah 1–2 |
| `/device`         | device token  | —                        | ✅ langkah 1–2 |
| `/disconnect`     | device token  | —                        | ✅ langkah 1–2 |
| `/update-device`  | device token  | `name, device`           | ✅ langkah 1–2 |
| `/send`           | device token  | `target, message`        | ✅ langkah 3   |
| webhook emitter   | —             | (POST ke api-gateway)    | ✅ langkah 3   |

Tambahan non-Fonnte (khusus gateway ini, `GET`, tanpa auth — service hanya listen localhost):

| Endpoint  | Guna |
| --------- | ---- |
| `/`       | Healthcheck ringkas Docker/monitoring. |
| `/health` | Status koneksi **per device** + `degraded: true` bila ada device ter-pair yang tidak `connect`. Token dipotong 6 char di output. Dipakai untuk memantau WA putus tanpa membuka UI admin. |

### Webhook delivery (gateway → api-gateway)

Saat ada receipt pesan keluar, gateway POST JSON ke `WA_GATEWAY_WEBHOOK_URL`:

```json
{ "id": "<messageId>", "sender": "62xxx", "status": "delivered", "device": "62xxx" }
```

`status`: `sent` (SERVER_ACK) → `delivered` (DELIVERY_ACK) → `read` (READ). Dipetakan
api-gateway ke `terkirim` → `sampai` → `dibaca` di `clinic_wa_log`. Header
`X-Webhook-Secret` dikirim bila `WA_GATEWAY_WEBHOOK_SECRET` di-set.

## Environment

| Var                            | Default  | Keterangan |
| ------------------------------ | -------- | ---------- |
| `WA_GATEWAY_PORT`              | `3204`   | Port HTTP service. |
| `WA_GATEWAY_ACCOUNT_TOKEN`     | —        | Token level-akun. **Harus sama** dengan `SENTIWA_ACCOUNT_TOKEN` di api-gateway. Generate `openssl rand -hex 32`, simpan di Vault. |
| `WA_GATEWAY_WEBHOOK_URL`       | —        | Tujuan POST event delivery/read (langkah 3). Internal: `http://api-gateway:3203/api/clinic/wa/webhook`. |
| `WA_GATEWAY_WEBHOOK_SECRET`    | —        | Shared-secret header webhook (opsional). |
| `WA_GATEWAY_DATA_DIR`          | `./data` | Dir persisten: `registry.json` + `sessions/<token>` creds Baileys. **Mount volume di Docker.** |
| `WA_GATEWAY_LOG_LEVEL`         | `info`   | Level log service (pino). |
| `WA_GATEWAY_BAILEYS_LOG_LEVEL` | `silent` | Level log internal Baileys. |

## Ketahanan koneksi (insiden 29 Jul 2026)

**Gejala.** Halaman admin menampilkan *"Perangkat terputus"*. Container `wa-gateway`
tetap `Up` (proses hidup, HTTP tetap melayani), tapi log **berhenti total** jam 05:00 WIB
dan device tidak pernah tersambung lagi sampai di-restart manual ~4 jam kemudian.
Kredensial tidak pernah dicabut — tidak perlu scan QR ulang.

**Akar masalah — guard `start()` yang mengunci diri sendiri.** Versi lama:

```ts
if (s.sock && s.status !== 'disconnect') return s;   // ❌
```

Handler `connection: 'close'` men-set `status = 'connecting'` lalu menjadwalkan `start()`.
Bila socket **baru** sudah terpasang di `s.sock` tapi belum pernah mencapai `'open'` —
persis yang terjadi saat WhatsApp menutup koneksi beruntun (`503` → `405` → `405` dalam
~15 detik) — maka pada percobaan berikutnya `s.sock` truthy **dan** status `'connecting'`,
sehingga `start()` langsung `return` tanpa melakukan apa pun. Hasilnya: tidak ada socket
hidup, tidak ada timer, tidak ada error yang dilempar. Sesi diam permanen, senyap.

Perbaikan: hanya sesi yang benar-benar `'connect'` dianggap sehat.

```ts
if (s.sock && s.status === 'connect') return s;      // ✅
if (s.sock) this.discardSocket(s);                   // socket zombie → buang
```

`discardSocket()` melepas listener **sebelum** `end()` — kalau tidak, socket yang sedang
dibuang memancarkan `connection.update {close}` dan menjadwalkan reconnect tandingan yang
berebut dengan koneksi baru.

**Lapis pertahanan lain yang ditambahkan** (semua terverifikasi, lihat `reconnect-policy.ts`,
`wa-version.ts`, `session-watchdog.ts`):

| Lapis | Guna |
| ----- | ---- |
| Backoff eksponensial + jitter (2s → 4s → … → maks 5 menit) | Delay lama **flat 2 detik**: saat WA menolak beruntun kita justru menghantam server berulang — pola pemicu rate-limit dan risiko ban nomor. |
| Klasifikasi disconnect fatal `401/403/419` | Dulu hanya `401` yang dianggap logout. `403`/`419` masuk jalur reconnect → retry selamanya padahal kredensial sudah mati. Sekarang creds dibersihkan dan menunggu pairing ulang. |
| Timeout `connect()` 60 detik | Menjamin flag `starting` selalu dilepas, sehingga satu percobaan yang menggantung tidak memblokir semua percobaan berikutnya. |
| `getWaVersion()` — timeout 5s + cache 6 jam + fallback | `fetchLatestBaileysVersion()` menembak GitHub **tiap** reconnect tanpa timeout. Kini gagal/lambat → pakai cache atau versi bawaan Baileys; koneksi tetap jalan tanpa internet ke GitHub. |
| Watchdog 60 detik | Jaring pengaman terakhir: sesi ter-pair yang bukan `connect`, tanpa reconnect terjadwal, dan diam > 3 menit dipaksa start ulang — termasuk melepas flag `starting` yang basi. |

> Catatan: `503`/`405`/`515`/`408` adalah gangguan **sementara** dari sisi WhatsApp dan
> memang wajar muncul sesekali; yang tidak boleh terjadi adalah gateway berhenti mencoba.
> Untuk memantau tanpa membuka UI: `curl -s localhost:3204/health` → cek `degraded`.

## Cara migrasi dari Fonnte (langkah 5 — nanti)

Di `api-gateway`, ganti env (tanpa ubah kode):

```
SENTIWA_API_URL=http://wa-gateway:3204
SENTIWA_ACCOUNT_TOKEN=<sama dengan WA_GATEWAY_ACCOUNT_TOKEN>
SENTIWA_WEBHOOK_URL=http://api-gateway:3203/api/clinic/wa/webhook
```

## Dev lokal

```bash
npm run dev -w @althea/wa-gateway     # ts-node, watch manual
# atau
npm run build -w @althea/wa-gateway && npm run start -w @althea/wa-gateway
```

> ⚠️ Baileys = protokol WhatsApp Web tidak resmi. Risiko ban sama seperti Fonnte.
> Pakai nomor dedicated. `sessions/` jangan di-commit (creds login WA).

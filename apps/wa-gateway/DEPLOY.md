# wa-gateway — Deploy & Cutover dari Fonnte

> Catatan: `config/ports.json`, `infra/docker-compose.yml`, dan `.env*` **tidak bisa
> ditulis oleh agent** (permission). Snippet di bawah perlu Anda terapkan manual.

## 0. Prinsip cutover

api-gateway **tidak diubah kodenya**. Cutover = arahkan 3 env-nya ke wa-gateway:

```
SENTIWA_API_URL=http://localhost:3204                              # dari https://api.fonnte.com
SENTIWA_ACCOUNT_TOKEN=<token-acak>                                 # = WA_GATEWAY_ACCOUNT_TOKEN
SENTIWA_WEBHOOK_URL=http://localhost:3203/api/clinic/wa/webhook    # (kemungkinan sudah ada)
```

`http://localhost:...` valid karena produksi jalan `--network host` (api-gateway & wa-gateway
sama-sama di host network).

---

## 1. (SSOT) Daftarkan port 3204 di `config/ports.json`

Tambahkan di bawah entri `api-gateway` (hanya bookkeeping; runtime sudah baca `WA_GATEWAY_PORT`):

```json
"wa-gateway": {
  "name": "WA Gateway",
  "port": 3204,
  "type": "node",
  "envVar": "WA_GATEWAY_PORT",
  "description": "Self-hosted WhatsApp gateway (Baileys) — Fonnte-compatible, internal only.",
  "isActive": true
}
```

UFW: **tidak perlu** buka 3204 — diakses hanya via localhost oleh api-gateway.

---

## 2A. Dev (infra/docker-compose.yml)

Tambahkan service ini (sebelum blok `volumes:`), lalu tambah volume `wa_gateway_data`:

```yaml
  wa-gateway:
    build:
      context: ../apps/wa-gateway
      dockerfile: Dockerfile
    image: althea/wa-gateway:latest
    container_name: althea-infra-wa-gateway
    restart: unless-stopped
    ports:
      - "3204:3204"
    environment:
      WA_GATEWAY_PORT: 3204
      WA_GATEWAY_ACCOUNT_TOKEN: ${WA_GATEWAY_ACCOUNT_TOKEN:-}
      WA_GATEWAY_WEBHOOK_URL: http://api-gateway:3203/api/clinic/wa/webhook
      WA_GATEWAY_WEBHOOK_SECRET: ${WA_GATEWAY_WEBHOOK_SECRET:-}
      WA_GATEWAY_DATA_DIR: /app/data
    volumes:
      - wa_gateway_data:/app/data
```
```yaml
volumes:
  # ...existing...
  wa_gateway_data:
    name: althea_wa_gateway_data
```

Di compose, network internal pakai container name → set `SENTIWA_API_URL=http://wa-gateway:3204`
di env api-gateway (dev).

---

## 2B. Produksi (`docker run` manual, `--network host`)

Pola sama seperti web-althea/api-gateway (lihat memory `althea-deploy-procedure`).

```bash
# 1) Generate account token sekali, simpan aman (mis. ke catatan deploy).
TOKEN=$(openssl rand -hex 32)
echo "WA_GATEWAY_ACCOUNT_TOKEN=$TOKEN"

# 2) Build image dari working tree.
docker build -t wa-gateway:latest -f apps/wa-gateway/Dockerfile apps/wa-gateway/

# 3) Run (host network, restart always, volume data persisten).
docker run -d --name wa-gateway --network host --restart always \
  -e WA_GATEWAY_PORT=3204 \
  -e WA_GATEWAY_ACCOUNT_TOKEN="$TOKEN" \
  -e WA_GATEWAY_WEBHOOK_URL=http://localhost:3203/api/clinic/wa/webhook \
  -v /opt/althea-wa-gateway-data:/app/data \
  wa-gateway:latest

# 4) Cek hidup.
curl -s http://localhost:3204/    # → {"status":true,"service":"wa-gateway",...}
```

---

## 3. Cutover api-gateway (⚠️ GATED — minta konfirmasi, ada riwayat insiden deploy-be.sh)

api-gateway prod **tanpa env-file** — env via `-e` flags. Recreate container dengan env baru:

```bash
# Dump env sekarang (bersih, skip baris kosong):
docker inspect api-gateway --format '{{range .Config.Env}}{{println .}}{{end}}' \
  | grep -E '^[A-Z_]+=' > /tmp/apigw.env

# Edit /tmp/apigw.env:
#   - set SENTIWA_API_URL=http://localhost:3204   (tambah kalau belum ada)
#   - set SENTIWA_ACCOUNT_TOKEN=$TOKEN            (samakan dgn wa-gateway)
#   - pastikan SENTIWA_WEBHOOK_URL=http://localhost:3203/api/clinic/wa/webhook

# Backup → recreate (pola aman: tag backup dulu).
docker tag api-gateway:latest api-gateway:backup
docker rm -f api-gateway
docker run -d --name api-gateway --network host --restart always \
  $(sed 's/^/-e /' /tmp/apigw.env) \
  api-gateway:latest

# Verifikasi:
curl -s -o /dev/null -w '%{http_code}\n' http://localhost:3203/api/health   # 200
```
Rollback bila gagal: ulangi `docker run` dengan `/tmp/apigw.env` versi lama (SENTIWA_API_URL =
`https://api.fonnte.com`, SENTIWA_ACCOUNT_TOKEN lama).

---

## 4. Pairing + verifikasi (lewat UI Pengaturan WA yang sudah ada)

Setelah api-gateway menunjuk wa-gateway, **UI lama bekerja apa adanya** (memanggil endpoint
Fonnte-compatible kita):

1. Admin → Pengaturan → Notifikasi WA → **Tambah device** (`name=althea`, nomor WA Business).
2. **Scan QR** dari WhatsApp → Linked Devices (HP Anda). Tunggu status `connect`.
3. **Aktifkan** device → token tersimpan di `clinic_settings.waActiveDeviceToken`.
4. **Send test** ke nomor Anda → cek pesan masuk + status log `terkirim → sampai → dibaca`.

### ✅ Verifikasi inti masalah (notif HP)
Saat device **tetap tertaut & Active**, kirim pesan WA ke nomor itu dari HP lain →
banner/suara/getar **harus muncul** di iPhone XS (efek `markOnlineOnConnect:false`).
Bandingkan dengan perilaku Fonnte lama (hanya badge). Ini acceptance test utama.

---

## 5. Rollback total ke Fonnte

Recreate api-gateway dengan `SENTIWA_API_URL=https://api.fonnte.com` + token Fonnte lama.
wa-gateway boleh dibiarkan jalan (idle) atau `docker stop wa-gateway`. Data sesi tetap di volume.

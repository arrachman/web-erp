#!/usr/bin/env bash
# Deploy wa-gateway untuk ERP Bahtera Madani (server ssh.fr-labs.my.id).
#
# Topologi server ini: api-gateway ERP berjalan sebagai container di network
# `sentient_factory_isolated_net`, jadi wa-gateway ikut di network yang sama
# dan dipanggil backend sebagai http://wa-gateway:3204 (pola compose di
# DEPLOY.md §2A — BUKAN host network, karena api-gateway di sini bukan
# proses host). Webhook mengarah ke nama container api-gateway.
#
# Token akun + secret webhook dibaca dari /opt/wa-gateway-data/secrets.env
# (chmod 600, TIDAK masuk git). Data sesi (registry + creds Baileys) persisten
# di /opt/wa-gateway-data — hilang = scan QR ulang.
# Container jalan sebagai uid 1000 (rania) karena image memakai user nodejs
# (1001) yang tidak bisa menulis ke direktori milik rania tanpa sudo.
set -euo pipefail

cd "$(dirname "$0")"
docker build -t wa-gateway:latest -f Dockerfile .

# shellcheck disable=SC1091
source /opt/wa-gateway-data/secrets.env

docker rm -f wa-gateway 2>/dev/null || true
docker run -d --name wa-gateway --restart always --user 1000:1000 \
  --network sentient_factory_isolated_net \
  -p 127.0.0.1:3204:3204 \
  -e WA_GATEWAY_PORT=3204 \
  -e WA_GATEWAY_ACCOUNT_TOKEN="$WA_ACCOUNT_TOKEN" \
  -e WA_GATEWAY_WEBHOOK_URL=http://sentient-infra-api-gateway:3203/api/erp/wa/webhook \
  -e WA_GATEWAY_WEBHOOK_SECRET="$WA_WEBHOOK_SECRET" \
  -e WA_GATEWAY_DATA_DIR=/app/data \
  -v /opt/wa-gateway-data:/app/data \
  wa-gateway:latest

sleep 3
curl -s http://127.0.0.1:3204/health
echo

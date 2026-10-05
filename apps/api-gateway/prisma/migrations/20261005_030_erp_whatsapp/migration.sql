-- Fase 3 W6 — WhatsApp: tabel template + log pengiriman, seed pengaturan
-- gateway (token akun/secret di-seed terpisah dari file rahasia server, tidak
-- masuk repo), dan 3 template peristiwa awal (order diterima, order terkirim,
-- tagihan terbit).

CREATE TABLE IF NOT EXISTS "sys_wa_templates" (
  "id" BIGSERIAL PRIMARY KEY,
  "name" TEXT NOT NULL,
  "category" TEXT NOT NULL,
  "trigger_event" TEXT,
  "body" TEXT NOT NULL,
  "is_active" BOOLEAN NOT NULL DEFAULT true,
  "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ(6) NOT NULL DEFAULT now(),
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  "deleted_at" TIMESTAMPTZ(6)
);
CREATE UNIQUE INDEX IF NOT EXISTS "sys_wa_templates_name_key" ON "sys_wa_templates" ("name");
CREATE INDEX IF NOT EXISTS "sys_wa_templates_category_is_active_idx" ON "sys_wa_templates" ("category", "is_active");

CREATE TABLE IF NOT EXISTS "sys_wa_logs" (
  "id" BIGSERIAL PRIMARY KEY,
  "template_id" BIGINT REFERENCES "sys_wa_templates" ("id"),
  "recipient_type" TEXT NOT NULL,
  "recipient_phone" TEXT NOT NULL,
  "body" TEXT NOT NULL,
  "message_id" TEXT,
  "status" TEXT NOT NULL DEFAULT 'queued',
  "error_reason" TEXT,
  "reference_type" TEXT,
  "reference_id" BIGINT,
  "metadata" JSONB NOT NULL DEFAULT '{}',
  "sent_at" TIMESTAMPTZ(6),
  "delivered_at" TIMESTAMPTZ(6),
  "read_at" TIMESTAMPTZ(6),
  "failed_at" TIMESTAMPTZ(6),
  "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ(6) NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS "sys_wa_logs_status_idx" ON "sys_wa_logs" ("status");
CREATE INDEX IF NOT EXISTS "sys_wa_logs_recipient_phone_idx" ON "sys_wa_logs" ("recipient_phone");
CREATE INDEX IF NOT EXISTS "sys_wa_logs_created_at_idx" ON "sys_wa_logs" ("created_at");

-- Pengaturan gateway (module=WHATSAPP). ACCOUNT_TOKEN & WEBHOOK_SECRET diisi
-- oleh proses deploy dari file rahasia server — tidak ditulis di migrasi ini.
INSERT INTO "sys_settings" ("module", "group", "key", "name", "value", "data_type", "sort_order", "is_active", "created_at", "updated_at")
VALUES
  ('WHATSAPP', 'GATEWAY', 'GATEWAY_URL', 'URL WA Gateway', 'http://localhost:3204', 'string', 1, true, now(), now()),
  ('WHATSAPP', 'GATEWAY', 'SEND_ENABLED', 'Kirim notifikasi otomatis', 'false', 'boolean', 2, true, now(), now())
ON CONFLICT ("module", "group", "key") DO NOTHING;

-- Template peristiwa awal (bisa diedit admin di halaman WhatsApp).
INSERT INTO "sys_wa_templates" ("name", "category", "trigger_event", "body", "is_active", "created_at", "updated_at")
VALUES
  ('order_diterima', 'pesanan', 'order.created',
   'Halo {{sekolah}}, pesanan {{nomor_order}} senilai Rp{{total}} sudah kami terima dan sedang diproses. Terima kasih. — CV Bahtera Madani',
   true, now(), now()),
  ('order_terkirim', 'pengiriman', 'delivery.posted',
   'Halo {{sekolah}}, pesanan {{nomor_order}} sudah dikirim dengan Surat Jalan {{nomor_do}}. Mohon siapkan penerima untuk penandatanganan BAST. — CV Bahtera Madani',
   true, now(), now()),
  ('tagihan_terbit', 'tagihan', 'invoice.created',
   'Halo {{sekolah}}, tagihan {{nomor_invoice}} senilai Rp{{total}} untuk pesanan {{nomor_order}} sudah terbit. Jatuh tempo: {{jatuh_tempo}}. — CV Bahtera Madani',
   true, now(), now())
ON CONFLICT ("name") DO NOTHING;

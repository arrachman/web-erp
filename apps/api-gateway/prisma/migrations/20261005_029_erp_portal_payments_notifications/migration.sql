-- Fase 3 G2 (W5+W6): pembayaran portal + log notifikasi.
-- Provider pembayaran & BSP WhatsApp BELUM dipilih (plan §2 #5/#6):
-- provider v1 = 'SIMULASI' (adapter, webhook ber-HMAC) dan pengirim
-- notifikasi v1 = 'LOG' — keduanya provisional per CLAUDE.md §6,
-- diganti adapter nyata tanpa mengubah tabel/kontrak API.
CREATE TABLE IF NOT EXISTS fin_portal_payments (
  id BIGSERIAL PRIMARY KEY,
  invoice_id BIGINT NOT NULL REFERENCES sls_invoices(id),
  partner_id BIGINT NOT NULL REFERENCES md_partners(id),
  portal_account_id BIGINT REFERENCES md_portal_accounts(id),
  provider VARCHAR(40) NOT NULL DEFAULT 'SIMULASI',
  provider_ref VARCHAR(80) NOT NULL,
  va_number VARCHAR(40),
  amount NUMERIC(19,4) NOT NULL,
  status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
  expires_at TIMESTAMP,
  paid_at TIMESTAMP,
  receipt_id BIGINT,
  metadata JSONB,
  created_by_id BIGINT, updated_by_id BIGINT,
  created_at TIMESTAMP NOT NULL DEFAULT now(),
  updated_at TIMESTAMP NOT NULL DEFAULT now(),
  deleted_at TIMESTAMP
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_fin_portal_payments_ref
  ON fin_portal_payments(provider_ref) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_fin_portal_payments_invoice
  ON fin_portal_payments(invoice_id) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_fin_portal_payments_partner
  ON fin_portal_payments(partner_id) WHERE deleted_at IS NULL;

CREATE TABLE IF NOT EXISTS sys_notification_logs (
  id BIGSERIAL PRIMARY KEY,
  channel VARCHAR(20) NOT NULL DEFAULT 'WHATSAPP',
  event VARCHAR(40) NOT NULL,
  template_code VARCHAR(60),
  recipient_phone VARCHAR(40),
  recipient_name VARCHAR(200),
  partner_id BIGINT REFERENCES md_partners(id),
  portal_account_id BIGINT REFERENCES md_portal_accounts(id),
  related_doc_type VARCHAR(40),
  related_doc_id BIGINT,
  related_doc_number VARCHAR(60),
  message TEXT,
  status VARCHAR(20) NOT NULL DEFAULT 'QUEUED',
  provider VARCHAR(40) NOT NULL DEFAULT 'LOG',
  provider_message_id VARCHAR(120),
  error TEXT,
  metadata JSONB,
  created_by_id BIGINT, updated_by_id BIGINT,
  created_at TIMESTAMP NOT NULL DEFAULT now(),
  updated_at TIMESTAMP NOT NULL DEFAULT now(),
  deleted_at TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_sys_notification_logs_partner
  ON sys_notification_logs(partner_id) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_sys_notification_logs_event
  ON sys_notification_logs(event, created_at) WHERE deleted_at IS NULL;

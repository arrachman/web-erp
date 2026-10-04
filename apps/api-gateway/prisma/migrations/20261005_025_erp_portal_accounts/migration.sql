-- Fase 3 W2/W3 — Portal Sekolah: akun portal (registrasi → approval admin →
-- tertaut partner CUST-SCHOOL) + lead landing page (W1).
-- Lihat docs/fase-3-rencana-kerja.md.

CREATE TYPE "ErpPortalAccountStatus" AS ENUM ('PENDING', 'ACTIVE', 'REJECTED', 'SUSPENDED');
CREATE TYPE "ErpPortalAccountRole" AS ENUM ('KEPALA_SEKOLAH', 'BENDAHARA', 'OPERATOR');
CREATE TYPE "ErpPortalLeadStatus" AS ENUM ('NEW', 'CONVERTED', 'IGNORED');

CREATE TABLE "md_portal_accounts" (
  "id" BIGSERIAL PRIMARY KEY,
  "partner_id" BIGINT REFERENCES "md_partners" ("id"),
  "email" TEXT NOT NULL,
  "password_hash" TEXT NOT NULL,
  "full_name" TEXT NOT NULL,
  "phone" TEXT,
  "role" "ErpPortalAccountRole" NOT NULL DEFAULT 'OPERATOR',
  "status" "ErpPortalAccountStatus" NOT NULL DEFAULT 'PENDING',
  "school_name" TEXT NOT NULL,
  "npsn" TEXT,
  "jenjang" "ErpSchoolJenjang",
  "address" TEXT,
  "approved_at" TIMESTAMPTZ,
  "approved_by_id" BIGINT,
  "rejected_reason" TEXT,
  "last_login_at" TIMESTAMPTZ,
  "is_active" BOOLEAN NOT NULL DEFAULT true,
  "metadata" JSONB,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  "deleted_at" TIMESTAMPTZ
);
CREATE UNIQUE INDEX "md_portal_accounts_email_key" ON "md_portal_accounts" ("email");
CREATE INDEX "md_portal_accounts_status_idx" ON "md_portal_accounts" ("status");
CREATE INDEX "md_portal_accounts_partner_idx" ON "md_portal_accounts" ("partner_id");

CREATE TABLE "md_portal_leads" (
  "id" BIGSERIAL PRIMARY KEY,
  "school_name" TEXT NOT NULL,
  "contact_name" TEXT NOT NULL,
  "phone" TEXT,
  "email" TEXT,
  "jenjang" "ErpSchoolJenjang",
  "message" TEXT,
  "partner_id" BIGINT REFERENCES "md_partners" ("id"),
  "status" "ErpPortalLeadStatus" NOT NULL DEFAULT 'NEW',
  "metadata" JSONB,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMPTZ
);
CREATE INDEX "md_portal_leads_status_idx" ON "md_portal_leads" ("status");

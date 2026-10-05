-- Registry laporan hasil terjemahan Stimulsoft .mrt (instruksi user 2026-10-05).
-- Tabel BARU m0_reports: satu baris per JENIS laporan (1.326 jenis unik dari
-- 1.560 file .mrt; 218 file varian dilipat ke kolom variant_files; 16 file
-- junk/test tidak di-register). Berperan sebagai katalog di atas rpt_templates:
-- layout hasil terjemahan tetap di rpt_templates.template_json dan dihubungkan
-- lewat rpt_template_id / report_key pada fase terjemahan.
--
-- DEViasi nama yang disengaja: CLAUDE.md §1 mewajibkan pola DOMAIN_plural
-- (sys/adm/md/fin/...), tetapi user secara eksplisit memerintahkan nama
-- `m0_reports` (mewarisi registry legacy MySQL `m0_report`). Nama dipertahankan
-- apa adanya dan deviasi dicatat di docs/decisions/reports.md
-- (entri "Registry m0_reports — 2026-10-05") + m0-reports-design.md.
-- Seed data TIDAK di migrasi ini — dimuat terpisah & idempoten oleh
-- gen_m0_reports_seed.py (INSERT ... ON CONFLICT (code) DO NOTHING).

CREATE TABLE IF NOT EXISTS "m0_reports" (
  "id" BIGSERIAL PRIMARY KEY,
  "code" TEXT NOT NULL,
  "legacy_module" TEXT NOT NULL,
  "legacy_menu" TEXT,
  "erp_module" TEXT,
  "title" TEXT NOT NULL,
  "report_name" TEXT NOT NULL,
  "filename" TEXT NOT NULL,
  "source_rel_path" TEXT NOT NULL,
  "variant_files" JSONB NOT NULL DEFAULT '[]',
  "is_default" BOOLEAN NOT NULL DEFAULT true,
  "sql_spec" JSONB,
  "params" JSONB NOT NULL DEFAULT '[]',
  "page_setup" JSONB,
  "bands" JSONB NOT NULL DEFAULT '{}',
  "bindings" JSONB NOT NULL DEFAULT '[]',
  "functions_used" JSONB NOT NULL DEFAULT '[]',
  "flags" JSONB NOT NULL DEFAULT '{}',
  "export_formats" JSONB NOT NULL DEFAULT '{"PDF": true, "WORD": true, "EXCEL": true, "HTML": true}',
  "template_json" JSONB,
  "rpt_template_id" BIGINT,
  "report_key" TEXT,
  "translation_status" TEXT NOT NULL DEFAULT 'PENDING',
  "is_active" BOOLEAN NOT NULL DEFAULT true,
  "urutan" INTEGER NOT NULL DEFAULT 0,
  "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ(6) NOT NULL DEFAULT now(),
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  "deleted_at" TIMESTAMPTZ(6)
);
CREATE UNIQUE INDEX IF NOT EXISTS "m0_reports_code_key" ON "m0_reports" ("code");
CREATE INDEX IF NOT EXISTS "m0_reports_legacy_module_idx" ON "m0_reports" ("legacy_module");
CREATE INDEX IF NOT EXISTS "m0_reports_erp_module_idx" ON "m0_reports" ("erp_module");
CREATE INDEX IF NOT EXISTS "m0_reports_translation_status_idx" ON "m0_reports" ("translation_status");
CREATE INDEX IF NOT EXISTS "m0_reports_erp_module_urutan_idx" ON "m0_reports" ("erp_module", "urutan");
CREATE INDEX IF NOT EXISTS "m0_reports_report_key_idx" ON "m0_reports" ("report_key");

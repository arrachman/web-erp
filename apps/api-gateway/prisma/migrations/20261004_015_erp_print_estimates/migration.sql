-- Fase 2 P1 — Estimasi cetak: tabel estimasi + baris komponen biaya,
-- kode penomoran EST, aktivasi menu Production (M6) + menu Estimasi Cetak.

CREATE TYPE "ErpPrintEstimateStatus" AS ENUM ('DRAFT', 'ISSUED', 'CONVERTED', 'CANCELLED');
CREATE TYPE "ErpPrintCostComponent" AS ENUM ('KERTAS', 'TINTA', 'PLAT', 'PRE_PRESS', 'CETAK', 'FINISHING', 'MAKLOON', 'OVERHEAD', 'LAIN');

CREATE TABLE "mfg_print_estimates" (
  "id" BIGSERIAL PRIMARY KEY,
  "doc_number" TEXT NOT NULL,
  "branch_id" BIGINT NOT NULL,
  "doc_date" DATE NOT NULL,
  "fiscal_period_id" BIGINT NOT NULL,
  "customer_id" BIGINT,
  "item_id" BIGINT,
  "title" TEXT NOT NULL,
  "paper_size" TEXT,
  "page_count" INTEGER,
  "print_quantity" DECIMAL(19,4) NOT NULL,
  "color_spec" TEXT,
  "finishing" TEXT,
  "margin_percent" DECIMAL(5,2) NOT NULL DEFAULT 0,
  "total_cost" DECIMAL(19,4) NOT NULL DEFAULT 0,
  "total_price" DECIMAL(19,4) NOT NULL DEFAULT 0,
  "unit_price" DECIMAL(19,4) NOT NULL DEFAULT 0,
  "status" "ErpPrintEstimateStatus" NOT NULL DEFAULT 'DRAFT',
  "quotation_id" BIGINT,
  "notes" TEXT,
  "legacy_code" TEXT,
  "metadata" JSONB,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMPTZ,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT
);
CREATE UNIQUE INDEX "mfg_print_estimates_doc_number_key" ON "mfg_print_estimates" ("doc_number");
CREATE INDEX "mfg_print_estimates_status_idx" ON "mfg_print_estimates" ("status");
CREATE INDEX "mfg_print_estimates_customer_idx" ON "mfg_print_estimates" ("customer_id");

CREATE TABLE "mfg_print_estimate_lines" (
  "id" BIGSERIAL PRIMARY KEY,
  "estimate_id" BIGINT NOT NULL REFERENCES "mfg_print_estimates" ("id"),
  "line_no" INTEGER NOT NULL,
  "component" "ErpPrintCostComponent" NOT NULL,
  "description" TEXT,
  "item_id" BIGINT,
  "quantity" DECIMAL(19,4) NOT NULL,
  "unit_id" BIGINT,
  "unit_cost" DECIMAL(19,4) NOT NULL,
  "amount" DECIMAL(19,4) NOT NULL,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX "mfg_print_estimate_lines_estimate_idx" ON "mfg_print_estimate_lines" ("estimate_id");

-- Penomoran EST (meniru baris SQ milik quotation).
INSERT INTO "sys_document_numberings"
  ("document_code", "name", "prefix", "digit_count", "reset_policy", "next_number",
   "menu_id", "affects_ledger", "affects_inventory", "affects_cost", "notes", "legacy_code",
   "created_at", "updated_at")
SELECT 'EST', 'Estimasi Cetak', 'EST', "digit_count", "reset_policy", 1,
       "menu_id", false, false, false, "notes", NULL, now(), now()
FROM "sys_document_numberings" WHERE "document_code" = 'SQ'
  AND NOT EXISTS (SELECT 1 FROM "sys_document_numberings" WHERE "document_code" = 'EST');

-- Aktifkan root Production (anak-anaknya tidak pernah dinonaktifkan).
UPDATE "sys_menus" SET "is_active" = true WHERE "code" = 'M6';

-- Menu Estimasi Cetak di bawah M6.TX (meniru baris Work Order).
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M6.TX.EST', 'Estimasi Cetak', '/manufacturing/print-estimates', "icon", "type", "parent_id", 3, true, now(), now()
FROM "sys_menus" WHERE "code" = 'M6.TX.WO'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M6.TX.EST');

-- Role grants: salin dari menu Work Order.
INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.EST'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.WO')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.EST')
  );

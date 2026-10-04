-- A3 Dokumen pengadaan: BAST acceptance fields on delivery reports,
-- the generated-document archive, and the Paket Dokumen menu.
--
-- `sls_generated_documents` archives every rendered procurement document
-- (penawaran, surat pesanan, invoice, kuitansi, surat jalan, BAST, or a
-- combined PAKET pdf) per school + budget year, with versioning and a
-- signature audit trail. Files live in the shared ERP upload directory.

ALTER TABLE "sls_delivery_reports"
  ADD COLUMN "accepted_at" TIMESTAMPTZ(6),
  ADD COLUMN "accepted_by_name" TEXT,
  ADD COLUMN "accepted_by_title" TEXT,
  ADD COLUMN "acceptance_notes" TEXT;

CREATE TABLE "sls_generated_documents" (
  "id" BIGSERIAL NOT NULL,
  "order_id" BIGINT NOT NULL,
  "doc_type" TEXT NOT NULL,
  "variant" TEXT NOT NULL,
  "source_doc_type" TEXT NOT NULL,
  "source_id" BIGINT NOT NULL,
  "version" INTEGER NOT NULL DEFAULT 1,
  "file_name" TEXT NOT NULL,
  "stored_name" TEXT NOT NULL,
  "mime_type" TEXT NOT NULL DEFAULT 'application/pdf',
  "size_bytes" INTEGER NOT NULL DEFAULT 0,
  "status" TEXT NOT NULL DEFAULT 'GENERATED',
  "signed_by_name" TEXT,
  "signed_by_id" BIGINT,
  "signed_at" TIMESTAMPTZ(6),
  "signature_note" TEXT,
  "school_id" BIGINT,
  "budget_year" INTEGER,
  "created_by_id" BIGINT,
  "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "deleted_at" TIMESTAMPTZ(6),

  CONSTRAINT "sls_generated_documents_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "sls_generated_documents_stored_name_key"
  ON "sls_generated_documents" ("stored_name");
CREATE INDEX "sls_generated_documents_order_id_idx"
  ON "sls_generated_documents" ("order_id");
CREATE INDEX "sls_generated_documents_school_id_budget_year_idx"
  ON "sls_generated_documents" ("school_id", "budget_year");
CREATE INDEX "sls_generated_documents_doc_type_idx"
  ON "sls_generated_documents" ("doc_type");

ALTER TABLE "sls_generated_documents"
  ADD CONSTRAINT "sls_generated_documents_order_id_fkey"
  FOREIGN KEY ("order_id") REFERENCES "sls_orders" ("id")
  ON DELETE RESTRICT ON UPDATE CASCADE;

-- Menu: Sales -> Transactions -> Paket Dokumen.
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order",
   "is_active", "created_at", "updated_at")
SELECT 'M5.TX.DOCPKG', 'Paket Dokumen', '/sales/document-packages', NULL, 'ITEM',
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX'),
       17, true, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.TX.DOCPKG');

-- Role grants: copy every grant on the Sales Order menu.
INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.DOCPKG'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.SO')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.DOCPKG')
  );

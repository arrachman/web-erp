-- Per-domain transaction attachments (lampiran dokumen pendukung transaksi):
-- finance / inventory(warehouse) / purchasing / sales. Each table is generic,
-- keyed by (doc_type, doc_id) — doc_id is polymorphic across that domain's many
-- transaction tables, so NO foreign key, only an index. Files live on disk under
-- uploads/erp-transactions (bind-mounted to host). Additive — no existing tables
-- changed.

CREATE TABLE "fin_transaction_attachments" (
    "id" BIGSERIAL NOT NULL,
    "doc_type" TEXT NOT NULL,
    "doc_id" BIGINT NOT NULL,
    "file_name" TEXT NOT NULL,
    "stored_name" TEXT NOT NULL,
    "mime_type" TEXT NOT NULL,
    "size_bytes" INTEGER NOT NULL,
    "note" TEXT,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,
    CONSTRAINT "fin_transaction_attachments_pkey" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX "fin_transaction_attachments_stored_name_key" ON "fin_transaction_attachments"("stored_name");
CREATE INDEX "fin_transaction_attachments_doc_type_doc_id_idx" ON "fin_transaction_attachments"("doc_type", "doc_id");

CREATE TABLE "inv_transaction_attachments" (
    "id" BIGSERIAL NOT NULL,
    "doc_type" TEXT NOT NULL,
    "doc_id" BIGINT NOT NULL,
    "file_name" TEXT NOT NULL,
    "stored_name" TEXT NOT NULL,
    "mime_type" TEXT NOT NULL,
    "size_bytes" INTEGER NOT NULL,
    "note" TEXT,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,
    CONSTRAINT "inv_transaction_attachments_pkey" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX "inv_transaction_attachments_stored_name_key" ON "inv_transaction_attachments"("stored_name");
CREATE INDEX "inv_transaction_attachments_doc_type_doc_id_idx" ON "inv_transaction_attachments"("doc_type", "doc_id");

CREATE TABLE "pur_transaction_attachments" (
    "id" BIGSERIAL NOT NULL,
    "doc_type" TEXT NOT NULL,
    "doc_id" BIGINT NOT NULL,
    "file_name" TEXT NOT NULL,
    "stored_name" TEXT NOT NULL,
    "mime_type" TEXT NOT NULL,
    "size_bytes" INTEGER NOT NULL,
    "note" TEXT,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,
    CONSTRAINT "pur_transaction_attachments_pkey" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX "pur_transaction_attachments_stored_name_key" ON "pur_transaction_attachments"("stored_name");
CREATE INDEX "pur_transaction_attachments_doc_type_doc_id_idx" ON "pur_transaction_attachments"("doc_type", "doc_id");

CREATE TABLE "sls_transaction_attachments" (
    "id" BIGSERIAL NOT NULL,
    "doc_type" TEXT NOT NULL,
    "doc_id" BIGINT NOT NULL,
    "file_name" TEXT NOT NULL,
    "stored_name" TEXT NOT NULL,
    "mime_type" TEXT NOT NULL,
    "size_bytes" INTEGER NOT NULL,
    "note" TEXT,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,
    CONSTRAINT "sls_transaction_attachments_pkey" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX "sls_transaction_attachments_stored_name_key" ON "sls_transaction_attachments"("stored_name");
CREATE INDEX "sls_transaction_attachments_doc_type_doc_id_idx" ON "sls_transaction_attachments"("doc_type", "doc_id");

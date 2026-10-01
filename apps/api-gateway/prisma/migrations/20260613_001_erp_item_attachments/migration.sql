-- Item supporting documents (lampiran): any file attached to an item —
-- datasheet, spec, certificate, contract, etc. Generic (no kind, no primary).
-- Files live on disk under uploads/erp-items (bind-mounted to host); this table
-- is the metadata registry. Additive — no changes to existing tables.

CREATE TABLE "md_item_attachments" (
    "id" BIGSERIAL NOT NULL,
    "item_id" BIGINT NOT NULL,
    "file_name" TEXT NOT NULL,
    "stored_name" TEXT NOT NULL,
    "mime_type" TEXT NOT NULL,
    "size_bytes" INTEGER NOT NULL,
    "note" TEXT,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,

    CONSTRAINT "md_item_attachments_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_item_attachments_stored_name_key" ON "md_item_attachments"("stored_name");
CREATE INDEX "md_item_attachments_item_id_idx" ON "md_item_attachments"("item_id");

ALTER TABLE "md_item_attachments" ADD CONSTRAINT "md_item_attachments_item_id_fkey"
    FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- Item media gallery: product images (max 8, one primary) + one short video per item.
-- Files live on disk under uploads/erp-items (bind-mounted to host); this table is
-- the metadata registry. Additive — no changes to existing tables.

CREATE TYPE "ErpItemMediaKind" AS ENUM ('IMAGE', 'VIDEO');

CREATE TABLE "md_item_media" (
    "id" BIGSERIAL NOT NULL,
    "item_id" BIGINT NOT NULL,
    "kind" "ErpItemMediaKind" NOT NULL,
    "file_name" TEXT NOT NULL,
    "stored_name" TEXT NOT NULL,
    "mime_type" TEXT NOT NULL,
    "size_bytes" INTEGER NOT NULL,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "is_primary" BOOLEAN NOT NULL DEFAULT false,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,

    CONSTRAINT "md_item_media_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_item_media_stored_name_key" ON "md_item_media"("stored_name");
CREATE INDEX "md_item_media_item_id_idx" ON "md_item_media"("item_id");

ALTER TABLE "md_item_media" ADD CONSTRAINT "md_item_media_item_id_fkey"
    FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE CASCADE ON UPDATE CASCADE;

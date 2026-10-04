-- D1: school product catalog profiles (publisher / jenjang / grade /
-- curriculum / subject / HET / custom-print / stock class / channels)
-- layered on md_items, plus the missing MVP item categories.
CREATE TYPE "ErpItemCurriculum" AS ENUM ('MERDEKA', 'KURIKULUM_2013', 'KTSP_2006', 'LAINNYA');
CREATE TYPE "ErpStockClass" AS ENUM ('DAGANGAN', 'BAHAN_BAKU', 'BARANG_JADI', 'KONSINYASI');

CREATE TABLE "md_item_catalog_profiles" (
    "id" BIGSERIAL NOT NULL,
    "item_id" BIGINT NOT NULL,
    "publisher_name" TEXT,
    "jenjang" "ErpSchoolJenjang",
    "grade_level" TEXT,
    "curriculum" "ErpItemCurriculum",
    "subject" TEXT,
    "het_price" DECIMAL(19,4),
    "is_custom_print" BOOLEAN NOT NULL DEFAULT false,
    "stock_class" "ErpStockClass" NOT NULL DEFAULT 'DAGANGAN',
    "channels" JSONB,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),
    CONSTRAINT "md_item_catalog_profiles_pkey" PRIMARY KEY ("id")
);
CREATE UNIQUE INDEX "md_item_catalog_profiles_item_id_key" ON "md_item_catalog_profiles"("item_id");
CREATE INDEX "md_item_catalog_profiles_jenjang_idx" ON "md_item_catalog_profiles"("jenjang");
CREATE INDEX "md_item_catalog_profiles_stock_class_idx" ON "md_item_catalog_profiles"("stock_class");
ALTER TABLE "md_item_catalog_profiles" ADD CONSTRAINT "md_item_catalog_profiles_item_id_fkey" FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

INSERT INTO "md_item_categories" ("code", "name", "is_active", "created_at", "updated_at") VALUES
    ('LKS', 'LKS / Buku Kerja', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('PERAGA', 'Alat Peraga Pendidikan', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('SERAGAM', 'Seragam & Atribut', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('CETAK', 'Produk Cetak', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('PAKET', 'Paket / Bundel', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT ("code") DO NOTHING;

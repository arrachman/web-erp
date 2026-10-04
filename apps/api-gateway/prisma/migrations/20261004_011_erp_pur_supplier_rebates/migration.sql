-- D2: publisher/supplier rebate agreements (percent of net posted purchases
-- per calendar year, optionally scoped to one item category).
CREATE TABLE "pur_supplier_rebates" (
    "id" BIGSERIAL NOT NULL,
    "supplier_id" BIGINT NOT NULL,
    "category_id" BIGINT,
    "percent" DECIMAL(5,2) NOT NULL,
    "period_year" INTEGER NOT NULL,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),
    CONSTRAINT "pur_supplier_rebates_pkey" PRIMARY KEY ("id")
);
CREATE INDEX "pur_supplier_rebates_supplier_id_idx" ON "pur_supplier_rebates"("supplier_id");
CREATE INDEX "pur_supplier_rebates_period_year_idx" ON "pur_supplier_rebates"("period_year");
ALTER TABLE "pur_supplier_rebates" ADD CONSTRAINT "pur_supplier_rebates_supplier_id_fkey" FOREIGN KEY ("supplier_id") REFERENCES "md_partners"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE "pur_supplier_rebates" ADD CONSTRAINT "pur_supplier_rebates_category_id_fkey" FOREIGN KEY ("category_id") REFERENCES "md_item_categories"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

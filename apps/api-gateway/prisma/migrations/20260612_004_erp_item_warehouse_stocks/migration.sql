-- Per-warehouse stock level overrides (Stok Min/Maks + Min Order per Gudang).
-- md_items.min_stock/max_stock/min_order_qty remain the GLOBAL defaults;
-- a row here overrides them for a single warehouse.

CREATE TABLE "md_item_warehouse_stocks" (
    "id" BIGSERIAL NOT NULL,
    "item_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "min_stock" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "max_stock" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "min_order_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "md_item_warehouse_stocks_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_item_warehouse_stocks_item_id_warehouse_id_key" ON "md_item_warehouse_stocks"("item_id", "warehouse_id");
CREATE INDEX "md_item_warehouse_stocks_item_id_idx" ON "md_item_warehouse_stocks"("item_id");
CREATE INDEX "md_item_warehouse_stocks_warehouse_id_idx" ON "md_item_warehouse_stocks"("warehouse_id");

ALTER TABLE "md_item_warehouse_stocks" ADD CONSTRAINT "md_item_warehouse_stocks_item_id_fkey" FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "md_item_warehouse_stocks" ADD CONSTRAINT "md_item_warehouse_stocks_warehouse_id_fkey" FOREIGN KEY ("warehouse_id") REFERENCES "md_warehouses"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

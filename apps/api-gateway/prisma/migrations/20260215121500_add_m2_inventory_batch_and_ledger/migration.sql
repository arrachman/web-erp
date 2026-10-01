-- CreateTable
CREATE TABLE "m2_inventory_batch" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "item_id" TEXT NOT NULL,
    "batch_number" TEXT NOT NULL,
    "manufacturing_date" DATE,
    "expiry_date" DATE,
    "supplier_lot_number" TEXT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m2_inventory_batch_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m2_inventory_ledger" (
    "id" BIGSERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "transaction_date" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "item_id" TEXT NOT NULL,
    "warehouse_id" TEXT NOT NULL,
    "batch_id" TEXT NOT NULL,
    "transaction_type" TEXT NOT NULL,
    "reference_doc_type" TEXT,
    "reference_doc_id" TEXT,
    "reference_number" TEXT,
    "quantity_pcs" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "quantity_kg" DECIMAL(18,3) NOT NULL DEFAULT 0,
    "uom_id" TEXT NOT NULL,
    "unit_cost" DECIMAL(15,2),
    "total_value" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "user_id" TEXT,
    "notes" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m2_inventory_ledger_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "m2_inventory_batch_uuid_key" ON "m2_inventory_batch"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m2_inventory_batch_item_id_batch_number_key" ON "m2_inventory_batch"("item_id", "batch_number");

-- CreateIndex
CREATE INDEX "m2_inventory_batch_item_id_idx" ON "m2_inventory_batch"("item_id");

-- CreateIndex
CREATE INDEX "m2_inventory_batch_expiry_date_idx" ON "m2_inventory_batch"("expiry_date");

-- CreateIndex
CREATE UNIQUE INDEX "m2_inventory_ledger_uuid_key" ON "m2_inventory_ledger"("uuid");

-- CreateIndex
CREATE INDEX "m2_inventory_ledger_item_id_warehouse_id_batch_id_idx" ON "m2_inventory_ledger"("item_id", "warehouse_id", "batch_id");

-- CreateIndex
CREATE INDEX "m2_inventory_ledger_item_id_warehouse_id_batch_id_transaction_date_idx" ON "m2_inventory_ledger"("item_id", "warehouse_id", "batch_id", "transaction_date");

-- CreateIndex
CREATE INDEX "m2_inventory_ledger_transaction_date_idx" ON "m2_inventory_ledger"("transaction_date");

-- CreateIndex
CREATE INDEX "m2_inventory_ledger_reference_doc_type_reference_doc_id_idx" ON "m2_inventory_ledger"("reference_doc_type", "reference_doc_id");

-- CreateIndex
CREATE INDEX "m2_inventory_ledger_batch_id_idx" ON "m2_inventory_ledger"("batch_id");

-- AddForeignKey
ALTER TABLE "m2_inventory_batch" ADD CONSTRAINT "m2_inventory_batch_item_id_fkey" FOREIGN KEY ("item_id") REFERENCES "m1_item"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_inventory_ledger" ADD CONSTRAINT "m2_inventory_ledger_item_id_fkey" FOREIGN KEY ("item_id") REFERENCES "m1_item"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_inventory_ledger" ADD CONSTRAINT "m2_inventory_ledger_warehouse_id_fkey" FOREIGN KEY ("warehouse_id") REFERENCES "m1_warehouse"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_inventory_ledger" ADD CONSTRAINT "m2_inventory_ledger_batch_id_fkey" FOREIGN KEY ("batch_id") REFERENCES "m2_inventory_batch"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_inventory_ledger" ADD CONSTRAINT "m2_inventory_ledger_uom_id_fkey" FOREIGN KEY ("uom_id") REFERENCES "m1_uom"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_inventory_ledger" ADD CONSTRAINT "m2_inventory_ledger_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "m0_users"("uuid") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddConstraint
ALTER TABLE "m2_inventory_batch"
  ADD CONSTRAINT "m2_inventory_batch_expiry_vs_mfg_check"
  CHECK (
    "expiry_date" IS NULL
    OR "manufacturing_date" IS NULL
    OR "expiry_date" >= "manufacturing_date"
  );

-- AddConstraint
ALTER TABLE "m2_inventory_ledger"
  ADD CONSTRAINT "m2_inventory_ledger_transaction_type_check"
  CHECK (
    "transaction_type" IN (
      'INBOUND',
      'OUTBOUND',
      'ADJUSTMENT_PLUS',
      'ADJUSTMENT_MINUS',
      'SCRAP',
      'TRANSFER_IN',
      'TRANSFER_OUT',
      'RETURN_IN',
      'RETURN_OUT'
    )
  );

-- CreateView
CREATE OR REPLACE VIEW "v_m2_inventory_balance" AS
SELECT
  l."item_id",
  l."warehouse_id",
  l."batch_id",
  SUM(l."quantity_pcs") AS "balance_pcs",
  SUM(l."quantity_kg") AS "balance_kg"
FROM "m2_inventory_ledger" l
WHERE l."deleted_at" IS NULL
GROUP BY l."item_id", l."warehouse_id", l."batch_id";

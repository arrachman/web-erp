-- CreateTable
CREATE TABLE "m2_inbound" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "report_no" BIGSERIAL NOT NULL,
    "transaction_no" TEXT NOT NULL,
    "transaction_date" DATE NOT NULL DEFAULT CURRENT_DATE,
    "supplier_id" TEXT NOT NULL,
    "warehouse_id" TEXT NOT NULL,
    "notes" TEXT,
    "status" TEXT NOT NULL DEFAULT 'DRAFT',
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m2_inbound_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "m2_inbound_detail" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "inbound_id" TEXT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "item_id" TEXT NOT NULL,
    "batch_in" TEXT,
    "qty" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "expired_date" DATE,
    "item_code_snapshot" TEXT,
    "item_name_snapshot" TEXT,
    "notes" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m2_inbound_detail_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "m2_inbound_uuid_key" ON "m2_inbound"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m2_inbound_report_no_key" ON "m2_inbound"("report_no");

-- CreateIndex
CREATE UNIQUE INDEX "m2_inbound_transaction_no_key" ON "m2_inbound"("transaction_no");

-- CreateIndex
CREATE INDEX "m2_inbound_supplier_id_idx" ON "m2_inbound"("supplier_id");

-- CreateIndex
CREATE INDEX "m2_inbound_warehouse_id_idx" ON "m2_inbound"("warehouse_id");

-- CreateIndex
CREATE INDEX "m2_inbound_transaction_date_idx" ON "m2_inbound"("transaction_date");

-- CreateIndex
CREATE UNIQUE INDEX "m2_inbound_detail_uuid_key" ON "m2_inbound_detail"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m2_inbound_detail_inbound_id_line_no_key" ON "m2_inbound_detail"("inbound_id", "line_no");

-- CreateIndex
CREATE INDEX "m2_inbound_detail_inbound_id_idx" ON "m2_inbound_detail"("inbound_id");

-- CreateIndex
CREATE INDEX "m2_inbound_detail_item_id_idx" ON "m2_inbound_detail"("item_id");

-- AddForeignKey
ALTER TABLE "m2_inbound" ADD CONSTRAINT "m2_inbound_supplier_id_fkey" FOREIGN KEY ("supplier_id") REFERENCES "m1_contact"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_inbound" ADD CONSTRAINT "m2_inbound_warehouse_id_fkey" FOREIGN KEY ("warehouse_id") REFERENCES "m1_warehouse"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_inbound_detail" ADD CONSTRAINT "m2_inbound_detail_inbound_id_fkey" FOREIGN KEY ("inbound_id") REFERENCES "m2_inbound"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "m2_inbound_detail" ADD CONSTRAINT "m2_inbound_detail_item_id_fkey" FOREIGN KEY ("item_id") REFERENCES "m1_item"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

-- CreateTable
CREATE TABLE "m2_outbound_detail_batch" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "outbound_detail_id" TEXT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "batch_out" TEXT NOT NULL,
    "qty_pcs" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "qty_kg" DECIMAL(18,3) NOT NULL DEFAULT 0,
    "expired_date" DATE,
    "notes" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m2_outbound_detail_batch_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "m2_outbound_detail_batch_uuid_key" ON "m2_outbound_detail_batch"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m2_outbound_detail_batch_outbound_detail_id_line_no_key" ON "m2_outbound_detail_batch"("outbound_detail_id", "line_no");

-- CreateIndex
CREATE INDEX "m2_outbound_detail_batch_outbound_detail_id_idx" ON "m2_outbound_detail_batch"("outbound_detail_id");

-- AddForeignKey
ALTER TABLE "m2_outbound_detail_batch"
ADD CONSTRAINT "m2_outbound_detail_batch_outbound_detail_id_fkey"
FOREIGN KEY ("outbound_detail_id") REFERENCES "m2_outbound_detail"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

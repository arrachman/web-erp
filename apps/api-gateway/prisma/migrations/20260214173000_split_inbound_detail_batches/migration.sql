-- CreateTable
CREATE TABLE "m2_inbound_detail_batch" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "inbound_detail_id" TEXT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "batch_in" TEXT NOT NULL,
    "qty" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "expired_date" DATE,
    "notes" TEXT,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m2_inbound_detail_batch_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "m2_inbound_detail_batch_uuid_key" ON "m2_inbound_detail_batch"("uuid");

-- CreateIndex
CREATE UNIQUE INDEX "m2_inbound_detail_batch_inbound_detail_id_line_no_key" ON "m2_inbound_detail_batch"("inbound_detail_id", "line_no");

-- CreateIndex
CREATE INDEX "m2_inbound_detail_batch_inbound_detail_id_idx" ON "m2_inbound_detail_batch"("inbound_detail_id");

-- Migrate existing inline batch columns into batch rows (if any)
INSERT INTO "m2_inbound_detail_batch" (
  "uuid",
  "inbound_detail_id",
  "line_no",
  "batch_in",
  "qty",
  "expired_date",
  "notes",
  "created_at",
  "created_by",
  "updated_at",
  "updated_by",
  "deleted_at",
  "deleted_by"
)
SELECT
  CONCAT('ibd-batch-', d."uuid"),
  d."uuid",
  1,
  d."batch_in",
  d."qty",
  d."expired_date",
  NULL,
  d."created_at",
  d."created_by",
  d."updated_at",
  d."updated_by",
  d."deleted_at",
  d."deleted_by"
FROM "m2_inbound_detail" d
WHERE d."batch_in" IS NOT NULL;

-- Drop inline batch columns from detail table
ALTER TABLE "m2_inbound_detail" DROP COLUMN "batch_in";
ALTER TABLE "m2_inbound_detail" DROP COLUMN "expired_date";

-- AddForeignKey
ALTER TABLE "m2_inbound_detail_batch" ADD CONSTRAINT "m2_inbound_detail_batch_inbound_detail_id_fkey" FOREIGN KEY ("inbound_detail_id") REFERENCES "m2_inbound_detail"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

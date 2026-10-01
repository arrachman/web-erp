-- AlterTable
ALTER TABLE "m0_users"
ADD COLUMN "warehouse_id" TEXT;

-- CreateIndex
CREATE INDEX "m0_users_warehouse_id_idx" ON "m0_users"("warehouse_id");

-- AddForeignKey
ALTER TABLE "m0_users"
ADD CONSTRAINT "m0_users_warehouse_id_fkey"
FOREIGN KEY ("warehouse_id") REFERENCES "m1_warehouse"("uuid")
ON DELETE SET NULL
ON UPDATE CASCADE;

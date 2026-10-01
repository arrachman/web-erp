-- AlterTable: add bps_code column to md_cities
ALTER TABLE "md_cities" ADD COLUMN "bps_code" TEXT;

-- CreateIndex
CREATE UNIQUE INDEX "md_cities_bps_code_key" ON "md_cities"("bps_code");

-- CreateIndex
CREATE INDEX "md_cities_bps_code_idx" ON "md_cities"("bps_code");

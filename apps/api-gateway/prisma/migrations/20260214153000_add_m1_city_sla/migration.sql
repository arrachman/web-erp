-- CreateTable
CREATE TABLE "m1_city_sla" (
    "id" SERIAL NOT NULL,
    "uuid" TEXT NOT NULL,
    "city_id" TEXT NOT NULL,
    "std_lead_time_days" INTEGER NOT NULL DEFAULT 0,
    "std_return_do_days" INTEGER NOT NULL DEFAULT 0,
    "created_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" TEXT,
    "updated_at" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" TEXT,
    "deleted_at" TIMESTAMP(3),
    "deleted_by" TEXT,

    CONSTRAINT "m1_city_sla_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "m1_city_sla_uuid_key" ON "m1_city_sla"("uuid");

-- CreateIndex
CREATE INDEX "m1_city_sla_city_id_idx" ON "m1_city_sla"("city_id");

-- AddForeignKey
ALTER TABLE "m1_city_sla" ADD CONSTRAINT "m1_city_sla_city_id_fkey" FOREIGN KEY ("city_id") REFERENCES "m1_city"("uuid") ON DELETE RESTRICT ON UPDATE CASCADE;

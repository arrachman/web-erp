-- Fase 2 P7 — Pengiriman & armada: master kendaraan + trip pengiriman dengan
-- stops per Delivery Order. Acceptance di stop menulis BAST (Delivery Report)
-- sehingga tahap Order Hub bergerak ke DITERIMA. Kendaraan contoh bertanda
-- provisional (data armada asli menyusul dari klien).

CREATE TYPE "ErpVehicleStatus" AS ENUM ('ACTIVE', 'MAINTENANCE', 'INACTIVE');
CREATE TYPE "ErpDeliveryTripStatus" AS ENUM ('DRAFT', 'MUAT', 'BERANGKAT', 'SELESAI', 'BATAL');
CREATE TYPE "ErpTripStopStatus" AS ENUM ('MENUNGGU', 'TIBA', 'GAGAL');

CREATE TABLE "sls_vehicles" (
  "id" BIGSERIAL PRIMARY KEY,
  "code" TEXT NOT NULL UNIQUE,
  "name" TEXT NOT NULL,
  "plate_no" TEXT,
  "vehicle_type" TEXT NOT NULL DEFAULT 'BOX',
  "capacity_kg" DECIMAL(19,4),
  "status" "ErpVehicleStatus" NOT NULL DEFAULT 'ACTIVE',
  "legacy_code" TEXT,
  "notes" TEXT,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMPTZ,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT
);

CREATE TABLE "sls_delivery_trips" (
  "id" BIGSERIAL PRIMARY KEY,
  "doc_number" TEXT NOT NULL UNIQUE,
  "branch_id" BIGINT NOT NULL,
  "vehicle_id" BIGINT NOT NULL REFERENCES "sls_vehicles" ("id"),
  "driver_name" TEXT,
  "trip_date" DATE NOT NULL,
  "status" "ErpDeliveryTripStatus" NOT NULL DEFAULT 'DRAFT',
  "departed_at" TIMESTAMPTZ,
  "completed_at" TIMESTAMPTZ,
  "fuel_cost" DECIMAL(19,4) NOT NULL DEFAULT 0,
  "toll_cost" DECIMAL(19,4) NOT NULL DEFAULT 0,
  "other_cost" DECIMAL(19,4) NOT NULL DEFAULT 0,
  "total_cost" DECIMAL(19,4) NOT NULL DEFAULT 0,
  "notes" TEXT,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMPTZ,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT
);
CREATE INDEX "sls_delivery_trips_date_idx" ON "sls_delivery_trips" ("trip_date");

CREATE TABLE "sls_delivery_trip_stops" (
  "id" BIGSERIAL PRIMARY KEY,
  "trip_id" BIGINT NOT NULL REFERENCES "sls_delivery_trips" ("id"),
  "delivery_order_id" BIGINT NOT NULL REFERENCES "sls_delivery_orders" ("id"),
  "sequence_no" INTEGER NOT NULL,
  "status" "ErpTripStopStatus" NOT NULL DEFAULT 'MENUNGGU',
  "arrived_at" TIMESTAMPTZ,
  "receiver_name" TEXT,
  "receiver_title" TEXT,
  "failure_note" TEXT,
  "notes" TEXT,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX "sls_delivery_trip_stops_trip_idx" ON "sls_delivery_trip_stops" ("trip_id");
CREATE INDEX "sls_delivery_trip_stops_do_idx" ON "sls_delivery_trip_stops" ("delivery_order_id");

-- Penomoran TRP (meniru baris DO).
INSERT INTO "sys_document_numberings"
  ("document_code", "name", "prefix", "digit_count", "reset_policy", "next_number",
   "menu_id", "affects_ledger", "affects_inventory", "affects_cost", "notes", "legacy_code",
   "created_at", "updated_at")
SELECT 'TRP', 'Trip Pengiriman', 'TRP', "digit_count", "reset_policy", 1,
       "menu_id", false, false, false, "notes", NULL, now(), now()
FROM "sys_document_numberings" WHERE "document_code" = 'DO'
  AND NOT EXISTS (SELECT 1 FROM "sys_document_numberings" WHERE "document_code" = 'TRP');

-- Kendaraan contoh provisional (menunggu daftar armada asli dari klien).
INSERT INTO "sls_vehicles" ("code", "name", "plate_no", "vehicle_type", "capacity_kg", "legacy_code")
VALUES
  ('VH-BOX-01', '[PROVISIONAL] Truk Box CDD', 'PROVISIONAL-1', 'BOX', 2000, 'provisional'),
  ('VH-PICKUP-01', '[PROVISIONAL] Pick Up Bak', 'PROVISIONAL-2', 'PICKUP', 800, 'provisional')
ON CONFLICT ("code") DO NOTHING;

-- Menu Trip Pengiriman di bawah M5.TX (meniru baris Delivery Order).
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.TX.TRIP', 'Trip Pengiriman', '/sales/delivery-trips', "icon", "type", "parent_id", "sort_order" + 1, true, now(), now()
FROM "sys_menus" WHERE "code" = 'M5.TX.DO'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.TX.TRIP');

INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.TRIP'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.DO')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.TRIP')
  );

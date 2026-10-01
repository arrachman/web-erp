-- CreateEnum
CREATE TYPE "ErpPartnerSubCategoryType" AS ENUM ('CUSTOMER', 'SUPPLIER', 'SALESMAN');


-- CreateTable
CREATE TABLE "md_brands" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_brands_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_materials" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_materials_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_item_models" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_item_models_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_sizes" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_sizes_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_sections" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_sections_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_item_types" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_item_types_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_product_classes" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_product_classes_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_banks" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_banks_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_expeditions" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_expeditions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_other_costs" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_other_costs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_commissions" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "amount" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_commissions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_item_transaction_types" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "direction" TEXT NOT NULL DEFAULT 'IN',
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_item_transaction_types_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_countries" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "iso_code" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_countries_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_provinces" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "country_id" BIGINT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_provinces_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_cities" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "province_id" BIGINT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_cities_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_areas" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "city_id" BIGINT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_areas_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_item_locations" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "warehouse_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_item_locations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_partner_sub_categories" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "type" "ErpPartnerSubCategoryType" NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_partner_sub_categories_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_price_categories" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_price_categories_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_price_category_details" (
    "id" BIGSERIAL NOT NULL,
    "price_category_id" BIGINT NOT NULL,
    "min_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "max_qty" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4) NOT NULL DEFAULT 0,
    "price_adjustment" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_price_category_details_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_transaction_notes" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_transaction_notes_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_transaction_note_details" (
    "id" BIGSERIAL NOT NULL,
    "transaction_note_id" BIGINT NOT NULL,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "content" TEXT NOT NULL,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_transaction_note_details_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_item_permissions" (
    "id" BIGSERIAL NOT NULL,
    "item_id" BIGINT NOT NULL,
    "role_id" BIGINT NOT NULL,
    "can_view" BOOLEAN NOT NULL DEFAULT true,
    "can_sell" BOOLEAN NOT NULL DEFAULT true,
    "can_buy" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_item_permissions_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "md_brands_code_key" ON "md_brands"("code");

-- CreateIndex
CREATE INDEX "md_brands_legacy_code_idx" ON "md_brands"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_materials_code_key" ON "md_materials"("code");

-- CreateIndex
CREATE INDEX "md_materials_legacy_code_idx" ON "md_materials"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_item_models_code_key" ON "md_item_models"("code");

-- CreateIndex
CREATE INDEX "md_item_models_legacy_code_idx" ON "md_item_models"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_sizes_code_key" ON "md_sizes"("code");

-- CreateIndex
CREATE INDEX "md_sizes_legacy_code_idx" ON "md_sizes"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_sections_code_key" ON "md_sections"("code");

-- CreateIndex
CREATE INDEX "md_sections_legacy_code_idx" ON "md_sections"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_item_types_code_key" ON "md_item_types"("code");

-- CreateIndex
CREATE INDEX "md_item_types_legacy_code_idx" ON "md_item_types"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_product_classes_code_key" ON "md_product_classes"("code");

-- CreateIndex
CREATE INDEX "md_product_classes_legacy_code_idx" ON "md_product_classes"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_banks_code_key" ON "md_banks"("code");

-- CreateIndex
CREATE INDEX "md_banks_legacy_code_idx" ON "md_banks"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_expeditions_code_key" ON "md_expeditions"("code");

-- CreateIndex
CREATE INDEX "md_expeditions_legacy_code_idx" ON "md_expeditions"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_other_costs_code_key" ON "md_other_costs"("code");

-- CreateIndex
CREATE INDEX "md_other_costs_legacy_code_idx" ON "md_other_costs"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_commissions_code_key" ON "md_commissions"("code");

-- CreateIndex
CREATE INDEX "md_commissions_legacy_code_idx" ON "md_commissions"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_item_transaction_types_code_key" ON "md_item_transaction_types"("code");

-- CreateIndex
CREATE INDEX "md_item_transaction_types_legacy_code_idx" ON "md_item_transaction_types"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_countries_code_key" ON "md_countries"("code");

-- CreateIndex
CREATE INDEX "md_countries_legacy_code_idx" ON "md_countries"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_provinces_code_key" ON "md_provinces"("code");

-- CreateIndex
CREATE INDEX "md_provinces_country_id_idx" ON "md_provinces"("country_id");

-- CreateIndex
CREATE INDEX "md_provinces_legacy_code_idx" ON "md_provinces"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_cities_code_key" ON "md_cities"("code");

-- CreateIndex
CREATE INDEX "md_cities_province_id_idx" ON "md_cities"("province_id");

-- CreateIndex
CREATE INDEX "md_cities_legacy_code_idx" ON "md_cities"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_areas_code_key" ON "md_areas"("code");

-- CreateIndex
CREATE INDEX "md_areas_city_id_idx" ON "md_areas"("city_id");

-- CreateIndex
CREATE INDEX "md_areas_legacy_code_idx" ON "md_areas"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_item_locations_code_key" ON "md_item_locations"("code");

-- CreateIndex
CREATE INDEX "md_item_locations_warehouse_id_idx" ON "md_item_locations"("warehouse_id");

-- CreateIndex
CREATE INDEX "md_item_locations_legacy_code_idx" ON "md_item_locations"("legacy_code");

-- CreateIndex
CREATE INDEX "md_partner_sub_categories_type_idx" ON "md_partner_sub_categories"("type");

-- CreateIndex
CREATE INDEX "md_partner_sub_categories_legacy_code_idx" ON "md_partner_sub_categories"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_partner_sub_categories_code_type_key" ON "md_partner_sub_categories"("code", "type");

-- CreateIndex
CREATE UNIQUE INDEX "md_price_categories_code_key" ON "md_price_categories"("code");

-- CreateIndex
CREATE INDEX "md_price_categories_legacy_code_idx" ON "md_price_categories"("legacy_code");

-- CreateIndex
CREATE INDEX "md_price_category_details_price_category_id_idx" ON "md_price_category_details"("price_category_id");

-- CreateIndex
CREATE UNIQUE INDEX "md_transaction_notes_code_key" ON "md_transaction_notes"("code");

-- CreateIndex
CREATE INDEX "md_transaction_notes_legacy_code_idx" ON "md_transaction_notes"("legacy_code");

-- CreateIndex
CREATE INDEX "md_transaction_note_details_transaction_note_id_idx" ON "md_transaction_note_details"("transaction_note_id");

-- CreateIndex
CREATE INDEX "md_item_permissions_item_id_idx" ON "md_item_permissions"("item_id");

-- CreateIndex
CREATE INDEX "md_item_permissions_role_id_idx" ON "md_item_permissions"("role_id");

-- CreateIndex
CREATE INDEX "md_item_permissions_legacy_code_idx" ON "md_item_permissions"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_item_permissions_item_id_role_id_key" ON "md_item_permissions"("item_id", "role_id");

-- CreateIndex
CREATE INDEX "m2_inventory_ledger_item_id_warehouse_id_batch_id_transacti_idx" ON "m2_inventory_ledger"("item_id", "warehouse_id", "batch_id", "transaction_date_tz");

-- RenameForeignKey
-- SKIPPED drift: ALTER TABLE "clinic_psikolog_service" RENAME CONSTRAINT "clinic_psikolog_service_psikolog_fk" TO "clinic_psikolog_service_psikolog_user_id_fkey";
-- RenameForeignKey
-- SKIPPED drift: ALTER TABLE "clinic_psikolog_service" RENAME CONSTRAINT "clinic_psikolog_service_service_fk" TO "clinic_psikolog_service_service_id_fkey";
-- AddForeignKey
-- SKIPPED drift: ALTER TABLE "m0_manager_insight" ADD CONSTRAINT "m0_manager_insight_manager_user_id_fkey" FOREIGN KEY ("manager_user_id") REFERENCES "m0_users"("id") ON DELETE SET NULL ON UPDATE CASCADE;
-- AddForeignKey
-- SKIPPED drift: ALTER TABLE "m0_manager_risk" ADD CONSTRAINT "m0_manager_risk_manager_user_id_fkey" FOREIGN KEY ("manager_user_id") REFERENCES "m0_users"("id") ON DELETE SET NULL ON UPDATE CASCADE;
-- AddForeignKey
-- SKIPPED drift: ALTER TABLE "clinic_psikolog_profile" ADD CONSTRAINT "clinic_psikolog_profile_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "m0_users"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
-- AddForeignKey
-- SKIPPED drift: ALTER TABLE "clinic_booking" ADD CONSTRAINT "clinic_booking_client_id_fkey" FOREIGN KEY ("client_id") REFERENCES "clinic_client"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
-- AddForeignKey
-- SKIPPED drift: ALTER TABLE "clinic_booking" ADD CONSTRAINT "clinic_booking_service_id_fkey" FOREIGN KEY ("service_id") REFERENCES "clinic_service"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
-- AddForeignKey
-- SKIPPED drift: ALTER TABLE "clinic_booking" ADD CONSTRAINT "clinic_booking_psikolog_user_id_fkey" FOREIGN KEY ("psikolog_user_id") REFERENCES "m0_users"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
-- AddForeignKey
-- SKIPPED drift: ALTER TABLE "clinic_booking" ADD CONSTRAINT "clinic_booking_room_id_fkey" FOREIGN KEY ("room_id") REFERENCES "clinic_room"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
-- AddForeignKey
-- SKIPPED drift: ALTER TABLE "clinic_wa_log" ADD CONSTRAINT "clinic_wa_log_template_id_fkey" FOREIGN KEY ("template_id") REFERENCES "clinic_wa_template"("id") ON DELETE SET NULL ON UPDATE CASCADE;
-- AddForeignKey
-- SKIPPED drift: ALTER TABLE "clinic_payment" ADD CONSTRAINT "clinic_payment_booking_id_fkey" FOREIGN KEY ("booking_id") REFERENCES "clinic_booking"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
-- AddForeignKey
ALTER TABLE "md_provinces" ADD CONSTRAINT "md_provinces_country_id_fkey" FOREIGN KEY ("country_id") REFERENCES "md_countries"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_cities" ADD CONSTRAINT "md_cities_province_id_fkey" FOREIGN KEY ("province_id") REFERENCES "md_provinces"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_areas" ADD CONSTRAINT "md_areas_city_id_fkey" FOREIGN KEY ("city_id") REFERENCES "md_cities"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_price_category_details" ADD CONSTRAINT "md_price_category_details_price_category_id_fkey" FOREIGN KEY ("price_category_id") REFERENCES "md_price_categories"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_transaction_note_details" ADD CONSTRAINT "md_transaction_note_details_transaction_note_id_fkey" FOREIGN KEY ("transaction_note_id") REFERENCES "md_transaction_notes"("id") ON DELETE CASCADE ON UPDATE CASCADE;


-- Senti MDP — MES anchor (eam/mdp/mes_*): 10 tables, 5 enums, FKs intra-MDP.
-- Additive only. Source: apps/web-mdp/db-design/entities-mes.md

-- CreateEnum
CREATE TYPE "MdpMesOrderStatus" AS ENUM ('RELEASED', 'IN_PROGRESS', 'PAUSED', 'COMPLETED', 'CLOSED', 'CANCELLED');

-- CreateEnum
CREATE TYPE "MdpMesOperationStatus" AS ENUM ('PENDING', 'IN_PROGRESS', 'COMPLETED', 'SKIPPED');

-- CreateEnum
CREATE TYPE "MdpMesPostingStatus" AS ENUM ('PENDING', 'POSTED', 'FAILED');

-- CreateEnum
CREATE TYPE "MdpReasonCodeCategory" AS ENUM ('DOWNTIME', 'SCRAP', 'DELAY', 'QUALITY', 'OTHER');

-- CreateEnum
CREATE TYPE "MdpDowntimeType" AS ENUM ('PLANNED', 'UNPLANNED');

-- CreateTable
CREATE TABLE "eam_assets" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "erp_fixed_asset_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "eam_assets_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "eam_work_centers" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "asset_id" BIGINT,
    "ideal_cycle_seconds" DECIMAL(19,4),
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "eam_work_centers_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mdp_shifts" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "start_time" TEXT NOT NULL,
    "end_time" TEXT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mdp_shifts_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mdp_reason_codes" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "category" "MdpReasonCodeCategory" NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mdp_reason_codes_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mes_production_orders" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "erp_work_order_id" BIGINT,
    "item_id" BIGINT NOT NULL,
    "work_center_id" BIGINT,
    "planned_qty" DECIMAL(19,4) NOT NULL,
    "produced_good_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "produced_scrap_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "uom_code" TEXT,
    "status" "MdpMesOrderStatus" NOT NULL DEFAULT 'RELEASED',
    "planned_start_at" TIMESTAMPTZ(6),
    "planned_end_at" TIMESTAMPTZ(6),
    "actual_start_at" TIMESTAMPTZ(6),
    "actual_end_at" TIMESTAMPTZ(6),
    "branch_id" BIGINT,
    "notes" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mes_production_orders_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mes_operations" (
    "id" BIGSERIAL NOT NULL,
    "production_order_id" BIGINT NOT NULL,
    "sequence" INTEGER NOT NULL,
    "name" TEXT NOT NULL,
    "work_center_id" BIGINT NOT NULL,
    "status" "MdpMesOperationStatus" NOT NULL DEFAULT 'PENDING',
    "planned_qty" DECIMAL(19,4),
    "good_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "scrap_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "started_at" TIMESTAMPTZ(6),
    "completed_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mes_operations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mes_production_logs" (
    "id" BIGSERIAL NOT NULL,
    "production_order_id" BIGINT NOT NULL,
    "operation_id" BIGINT,
    "shift_id" BIGINT,
    "operator_id" BIGINT,
    "good_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "scrap_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "rework_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "scrap_reason_id" BIGINT,
    "started_at" TIMESTAMPTZ(6) NOT NULL,
    "ended_at" TIMESTAMPTZ(6),
    "posting_status" "MdpMesPostingStatus" NOT NULL DEFAULT 'PENDING',
    "erp_production_entry_id" BIGINT,
    "posted_at" TIMESTAMPTZ(6),
    "notes" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mes_production_logs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mes_material_consumptions" (
    "id" BIGSERIAL NOT NULL,
    "production_order_id" BIGINT NOT NULL,
    "operation_id" BIGINT,
    "item_id" BIGINT NOT NULL,
    "qty" DECIMAL(19,4) NOT NULL,
    "uom_code" TEXT,
    "source_bin_id" BIGINT,
    "posting_status" "MdpMesPostingStatus" NOT NULL DEFAULT 'PENDING',
    "consumed_at" TIMESTAMPTZ(6) NOT NULL,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mes_material_consumptions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mes_downtime_events" (
    "id" BIGSERIAL NOT NULL,
    "production_order_id" BIGINT,
    "operation_id" BIGINT,
    "work_center_id" BIGINT NOT NULL,
    "asset_id" BIGINT,
    "reason_id" BIGINT NOT NULL,
    "type" "MdpDowntimeType" NOT NULL DEFAULT 'UNPLANNED',
    "started_at" TIMESTAMPTZ(6) NOT NULL,
    "ended_at" TIMESTAMPTZ(6),
    "duration_seconds" DECIMAL(19,4),
    "reported_by_id" BIGINT,
    "notes" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mes_downtime_events_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mes_labor_logs" (
    "id" BIGSERIAL NOT NULL,
    "operation_id" BIGINT NOT NULL,
    "operator_id" BIGINT NOT NULL,
    "shift_id" BIGINT,
    "started_at" TIMESTAMPTZ(6) NOT NULL,
    "ended_at" TIMESTAMPTZ(6),
    "duration_seconds" DECIMAL(19,4),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mes_labor_logs_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "eam_assets_code_key" ON "eam_assets"("code");

-- CreateIndex
CREATE INDEX "eam_assets_erp_fixed_asset_id_idx" ON "eam_assets"("erp_fixed_asset_id");

-- CreateIndex
CREATE UNIQUE INDEX "eam_work_centers_code_key" ON "eam_work_centers"("code");

-- CreateIndex
CREATE INDEX "eam_work_centers_asset_id_idx" ON "eam_work_centers"("asset_id");

-- CreateIndex
CREATE UNIQUE INDEX "mdp_shifts_code_key" ON "mdp_shifts"("code");

-- CreateIndex
CREATE UNIQUE INDEX "mdp_reason_codes_code_key" ON "mdp_reason_codes"("code");

-- CreateIndex
CREATE UNIQUE INDEX "mes_production_orders_code_key" ON "mes_production_orders"("code");

-- CreateIndex
CREATE INDEX "mes_production_orders_erp_work_order_id_idx" ON "mes_production_orders"("erp_work_order_id");

-- CreateIndex
CREATE INDEX "mes_production_orders_item_id_idx" ON "mes_production_orders"("item_id");

-- CreateIndex
CREATE INDEX "mes_production_orders_work_center_id_idx" ON "mes_production_orders"("work_center_id");

-- CreateIndex
CREATE INDEX "mes_production_orders_branch_id_idx" ON "mes_production_orders"("branch_id");

-- CreateIndex
CREATE INDEX "mes_production_orders_status_idx" ON "mes_production_orders"("status");

-- CreateIndex
CREATE INDEX "mes_operations_production_order_id_idx" ON "mes_operations"("production_order_id");

-- CreateIndex
CREATE INDEX "mes_operations_work_center_id_idx" ON "mes_operations"("work_center_id");

-- CreateIndex
CREATE INDEX "mes_production_logs_production_order_id_idx" ON "mes_production_logs"("production_order_id");

-- CreateIndex
CREATE INDEX "mes_production_logs_operation_id_idx" ON "mes_production_logs"("operation_id");

-- CreateIndex
CREATE INDEX "mes_production_logs_shift_id_idx" ON "mes_production_logs"("shift_id");

-- CreateIndex
CREATE INDEX "mes_production_logs_operator_id_idx" ON "mes_production_logs"("operator_id");

-- CreateIndex
CREATE INDEX "mes_production_logs_scrap_reason_id_idx" ON "mes_production_logs"("scrap_reason_id");

-- CreateIndex
CREATE INDEX "mes_production_logs_posting_status_idx" ON "mes_production_logs"("posting_status");

-- CreateIndex
CREATE INDEX "mes_material_consumptions_production_order_id_idx" ON "mes_material_consumptions"("production_order_id");

-- CreateIndex
CREATE INDEX "mes_material_consumptions_operation_id_idx" ON "mes_material_consumptions"("operation_id");

-- CreateIndex
CREATE INDEX "mes_material_consumptions_item_id_idx" ON "mes_material_consumptions"("item_id");

-- CreateIndex
CREATE INDEX "mes_material_consumptions_source_bin_id_idx" ON "mes_material_consumptions"("source_bin_id");

-- CreateIndex
CREATE INDEX "mes_material_consumptions_posting_status_idx" ON "mes_material_consumptions"("posting_status");

-- CreateIndex
CREATE INDEX "mes_downtime_events_production_order_id_idx" ON "mes_downtime_events"("production_order_id");

-- CreateIndex
CREATE INDEX "mes_downtime_events_operation_id_idx" ON "mes_downtime_events"("operation_id");

-- CreateIndex
CREATE INDEX "mes_downtime_events_work_center_id_idx" ON "mes_downtime_events"("work_center_id");

-- CreateIndex
CREATE INDEX "mes_downtime_events_asset_id_idx" ON "mes_downtime_events"("asset_id");

-- CreateIndex
CREATE INDEX "mes_downtime_events_reason_id_idx" ON "mes_downtime_events"("reason_id");

-- CreateIndex
CREATE INDEX "mes_labor_logs_operation_id_idx" ON "mes_labor_logs"("operation_id");

-- CreateIndex
CREATE INDEX "mes_labor_logs_operator_id_idx" ON "mes_labor_logs"("operator_id");

-- CreateIndex
CREATE INDEX "mes_labor_logs_shift_id_idx" ON "mes_labor_logs"("shift_id");

-- AddForeignKey
ALTER TABLE "eam_work_centers" ADD CONSTRAINT "eam_work_centers_asset_id_fkey" FOREIGN KEY ("asset_id") REFERENCES "eam_assets"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_production_orders" ADD CONSTRAINT "mes_production_orders_work_center_id_fkey" FOREIGN KEY ("work_center_id") REFERENCES "eam_work_centers"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_operations" ADD CONSTRAINT "mes_operations_production_order_id_fkey" FOREIGN KEY ("production_order_id") REFERENCES "mes_production_orders"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_operations" ADD CONSTRAINT "mes_operations_work_center_id_fkey" FOREIGN KEY ("work_center_id") REFERENCES "eam_work_centers"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_production_logs" ADD CONSTRAINT "mes_production_logs_production_order_id_fkey" FOREIGN KEY ("production_order_id") REFERENCES "mes_production_orders"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_production_logs" ADD CONSTRAINT "mes_production_logs_operation_id_fkey" FOREIGN KEY ("operation_id") REFERENCES "mes_operations"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_production_logs" ADD CONSTRAINT "mes_production_logs_shift_id_fkey" FOREIGN KEY ("shift_id") REFERENCES "mdp_shifts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_production_logs" ADD CONSTRAINT "mes_production_logs_scrap_reason_id_fkey" FOREIGN KEY ("scrap_reason_id") REFERENCES "mdp_reason_codes"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_material_consumptions" ADD CONSTRAINT "mes_material_consumptions_production_order_id_fkey" FOREIGN KEY ("production_order_id") REFERENCES "mes_production_orders"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_material_consumptions" ADD CONSTRAINT "mes_material_consumptions_operation_id_fkey" FOREIGN KEY ("operation_id") REFERENCES "mes_operations"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_downtime_events" ADD CONSTRAINT "mes_downtime_events_production_order_id_fkey" FOREIGN KEY ("production_order_id") REFERENCES "mes_production_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_downtime_events" ADD CONSTRAINT "mes_downtime_events_operation_id_fkey" FOREIGN KEY ("operation_id") REFERENCES "mes_operations"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_downtime_events" ADD CONSTRAINT "mes_downtime_events_work_center_id_fkey" FOREIGN KEY ("work_center_id") REFERENCES "eam_work_centers"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_downtime_events" ADD CONSTRAINT "mes_downtime_events_asset_id_fkey" FOREIGN KEY ("asset_id") REFERENCES "eam_assets"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_downtime_events" ADD CONSTRAINT "mes_downtime_events_reason_id_fkey" FOREIGN KEY ("reason_id") REFERENCES "mdp_reason_codes"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_labor_logs" ADD CONSTRAINT "mes_labor_logs_operation_id_fkey" FOREIGN KEY ("operation_id") REFERENCES "mes_operations"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mes_labor_logs" ADD CONSTRAINT "mes_labor_logs_shift_id_fkey" FOREIGN KEY ("shift_id") REFERENCES "mdp_shifts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

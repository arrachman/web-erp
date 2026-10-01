-- MDP CMMS module — additive DDL (mnt_ tables only, 0 DROP).
-- Extracted from prisma migrate diff; warehouse drift/DROP excluded per
-- apps/web-mdp CLAUDE.md migration discipline. Source: mdp-cmms.prisma.

-- CreateEnum
CREATE TYPE "MdpMntWorkOrderType" AS ENUM ('CORRECTIVE', 'PREVENTIVE', 'PREDICTIVE', 'INSPECTION');

-- CreateEnum
CREATE TYPE "MdpMntWorkOrderStatus" AS ENUM ('OPEN', 'SCHEDULED', 'IN_PROGRESS', 'ON_HOLD', 'COMPLETED', 'CANCELLED');

-- CreateEnum
CREATE TYPE "MdpMntPriority" AS ENUM ('LOW', 'MEDIUM', 'HIGH', 'URGENT');

-- CreateEnum
CREATE TYPE "MdpMntPmTriggerType" AS ENUM ('TIME_BASED', 'METER_BASED');

-- CreateEnum
CREATE TYPE "MdpMntFailureCodeType" AS ENUM ('FAILURE', 'CAUSE', 'REMEDY');

-- CreateEnum
CREATE TYPE "MdpMntPostingStatus" AS ENUM ('PENDING', 'POSTED', 'FAILED');

-- CreateTable
CREATE TABLE "mnt_failure_codes" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "type" "MdpMntFailureCodeType" NOT NULL,
    "description" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mnt_failure_codes_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mnt_pm_schedules" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "asset_id" BIGINT,
    "work_center_id" BIGINT,
    "trigger_type" "MdpMntPmTriggerType" NOT NULL DEFAULT 'TIME_BASED',
    "interval_days" INTEGER,
    "meter_type" TEXT,
    "meter_interval" DECIMAL(19,4),
    "last_service_at" TIMESTAMPTZ(6),
    "next_due_at" TIMESTAMPTZ(6),
    "task_description" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mnt_pm_schedules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mnt_work_orders" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "type" "MdpMntWorkOrderType" NOT NULL DEFAULT 'CORRECTIVE',
    "status" "MdpMntWorkOrderStatus" NOT NULL DEFAULT 'OPEN',
    "priority" "MdpMntPriority" NOT NULL DEFAULT 'MEDIUM',
    "asset_id" BIGINT,
    "work_center_id" BIGINT,
    "pm_schedule_id" BIGINT,
    "failure_code_id" BIGINT,
    "description" TEXT,
    "scheduled_start_at" TIMESTAMPTZ(6),
    "scheduled_end_at" TIMESTAMPTZ(6),
    "actual_start_at" TIMESTAMPTZ(6),
    "actual_end_at" TIMESTAMPTZ(6),
    "downtime_minutes" DECIMAL(19,4),
    "reported_by_id" BIGINT,
    "assigned_to_id" BIGINT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mnt_work_orders_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mnt_spare_parts" (
    "id" BIGSERIAL NOT NULL,
    "work_order_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "qty" DECIMAL(19,4) NOT NULL,
    "uom_code" TEXT,
    "posting_status" "MdpMntPostingStatus" NOT NULL DEFAULT 'PENDING',
    "erp_stock_movement_id" BIGINT,
    "posted_at" TIMESTAMPTZ(6),
    "notes" TEXT,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mnt_spare_parts_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "mnt_failure_codes_code_key" ON "mnt_failure_codes"("code");

-- CreateIndex
CREATE INDEX "mnt_failure_codes_type_idx" ON "mnt_failure_codes"("type");

-- CreateIndex
CREATE UNIQUE INDEX "mnt_pm_schedules_code_key" ON "mnt_pm_schedules"("code");

-- CreateIndex
CREATE INDEX "mnt_pm_schedules_asset_id_idx" ON "mnt_pm_schedules"("asset_id");

-- CreateIndex
CREATE INDEX "mnt_pm_schedules_trigger_type_idx" ON "mnt_pm_schedules"("trigger_type");

-- CreateIndex
CREATE INDEX "mnt_pm_schedules_next_due_at_idx" ON "mnt_pm_schedules"("next_due_at");

-- CreateIndex
CREATE UNIQUE INDEX "mnt_work_orders_code_key" ON "mnt_work_orders"("code");

-- CreateIndex
CREATE INDEX "mnt_work_orders_type_idx" ON "mnt_work_orders"("type");

-- CreateIndex
CREATE INDEX "mnt_work_orders_status_idx" ON "mnt_work_orders"("status");

-- CreateIndex
CREATE INDEX "mnt_work_orders_priority_idx" ON "mnt_work_orders"("priority");

-- CreateIndex
CREATE INDEX "mnt_work_orders_asset_id_idx" ON "mnt_work_orders"("asset_id");

-- CreateIndex
CREATE INDEX "mnt_work_orders_pm_schedule_id_idx" ON "mnt_work_orders"("pm_schedule_id");

-- CreateIndex
CREATE INDEX "mnt_work_orders_failure_code_id_idx" ON "mnt_work_orders"("failure_code_id");

-- CreateIndex
CREATE INDEX "mnt_work_orders_assigned_to_id_idx" ON "mnt_work_orders"("assigned_to_id");

-- CreateIndex
CREATE INDEX "mnt_spare_parts_work_order_id_idx" ON "mnt_spare_parts"("work_order_id");

-- CreateIndex
CREATE INDEX "mnt_spare_parts_item_id_idx" ON "mnt_spare_parts"("item_id");

-- CreateIndex
CREATE INDEX "mnt_spare_parts_posting_status_idx" ON "mnt_spare_parts"("posting_status");

-- AddForeignKey
ALTER TABLE "mnt_work_orders" ADD CONSTRAINT "mnt_work_orders_pm_schedule_id_fkey" FOREIGN KEY ("pm_schedule_id") REFERENCES "mnt_pm_schedules"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mnt_work_orders" ADD CONSTRAINT "mnt_work_orders_failure_code_id_fkey" FOREIGN KEY ("failure_code_id") REFERENCES "mnt_failure_codes"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mnt_spare_parts" ADD CONSTRAINT "mnt_spare_parts_work_order_id_fkey" FOREIGN KEY ("work_order_id") REFERENCES "mnt_work_orders"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

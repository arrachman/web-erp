-- Senti MDP WMS (additive): wms_tasks/handling_units/picks/movements + 4 MdpWms* enums.
-- Extracted from prisma migrate diff, keeping ONLY wms_*/MdpWms* statements;
-- all warehouse DROP/drift discarded. 0 DROP. FKs reference only wms_* tables.

CREATE TYPE "MdpWmsTaskType" AS ENUM ('PUTAWAY', 'PICK', 'MOVE', 'COUNT', 'REPLENISH');

CREATE TYPE "MdpWmsTaskStatus" AS ENUM ('OPEN', 'IN_PROGRESS', 'COMPLETED', 'CANCELLED');

CREATE TYPE "MdpWmsPostingStatus" AS ENUM ('PENDING', 'POSTED', 'FAILED');

CREATE TYPE "MdpWmsHandlingUnitStatus" AS ENUM ('OPEN', 'CLOSED', 'SHIPPED');

CREATE TABLE "wms_tasks" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "type" "MdpWmsTaskType" NOT NULL,
    "status" "MdpWmsTaskStatus" NOT NULL DEFAULT 'OPEN',
    "item_id" BIGINT,
    "qty" DECIMAL(19,4),
    "uom_code" TEXT,
    "source_bin_id" BIGINT,
    "dest_bin_id" BIGINT,
    "production_order_id" BIGINT,
    "erp_reference_type" TEXT,
    "erp_reference_id" BIGINT,
    "assigned_to_id" BIGINT,
    "priority" INTEGER NOT NULL DEFAULT 0,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "wms_tasks_pkey" PRIMARY KEY ("id")
);

CREATE TABLE "wms_handling_units" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "status" "MdpWmsHandlingUnitStatus" NOT NULL DEFAULT 'OPEN',
    "current_bin_id" BIGINT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "wms_handling_units_pkey" PRIMARY KEY ("id")
);

CREATE TABLE "wms_picks" (
    "id" BIGSERIAL NOT NULL,
    "task_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "qty_requested" DECIMAL(19,4) NOT NULL,
    "qty_picked" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "source_bin_id" BIGINT,
    "handling_unit_id" BIGINT,
    "status" "MdpWmsTaskStatus" NOT NULL DEFAULT 'OPEN',
    "notes" TEXT,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "wms_picks_pkey" PRIMARY KEY ("id")
);

CREATE TABLE "wms_movements" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "task_id" BIGINT,
    "item_id" BIGINT NOT NULL,
    "qty" DECIMAL(19,4) NOT NULL,
    "uom_code" TEXT,
    "from_bin_id" BIGINT,
    "to_bin_id" BIGINT,
    "handling_unit_id" BIGINT,
    "moved_at" TIMESTAMPTZ(6) NOT NULL,
    "moved_by_id" BIGINT,
    "posting_status" "MdpWmsPostingStatus" NOT NULL DEFAULT 'PENDING',
    "erp_stock_movement_id" BIGINT,
    "posted_at" TIMESTAMPTZ(6),
    "notes" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "wms_movements_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "wms_tasks_code_key" ON "wms_tasks"("code");
CREATE INDEX "wms_tasks_type_idx" ON "wms_tasks"("type");
CREATE INDEX "wms_tasks_status_idx" ON "wms_tasks"("status");
CREATE INDEX "wms_tasks_item_id_idx" ON "wms_tasks"("item_id");
CREATE INDEX "wms_tasks_source_bin_id_idx" ON "wms_tasks"("source_bin_id");
CREATE INDEX "wms_tasks_dest_bin_id_idx" ON "wms_tasks"("dest_bin_id");
CREATE INDEX "wms_tasks_production_order_id_idx" ON "wms_tasks"("production_order_id");
CREATE INDEX "wms_tasks_assigned_to_id_idx" ON "wms_tasks"("assigned_to_id");
CREATE UNIQUE INDEX "wms_handling_units_code_key" ON "wms_handling_units"("code");
CREATE INDEX "wms_handling_units_current_bin_id_idx" ON "wms_handling_units"("current_bin_id");
CREATE INDEX "wms_picks_task_id_idx" ON "wms_picks"("task_id");
CREATE INDEX "wms_picks_item_id_idx" ON "wms_picks"("item_id");
CREATE INDEX "wms_picks_source_bin_id_idx" ON "wms_picks"("source_bin_id");
CREATE INDEX "wms_picks_handling_unit_id_idx" ON "wms_picks"("handling_unit_id");
CREATE UNIQUE INDEX "wms_movements_code_key" ON "wms_movements"("code");
CREATE INDEX "wms_movements_task_id_idx" ON "wms_movements"("task_id");
CREATE INDEX "wms_movements_item_id_idx" ON "wms_movements"("item_id");
CREATE INDEX "wms_movements_from_bin_id_idx" ON "wms_movements"("from_bin_id");
CREATE INDEX "wms_movements_to_bin_id_idx" ON "wms_movements"("to_bin_id");
CREATE INDEX "wms_movements_handling_unit_id_idx" ON "wms_movements"("handling_unit_id");
CREATE INDEX "wms_movements_posting_status_idx" ON "wms_movements"("posting_status");
ALTER TABLE "wms_picks" ADD CONSTRAINT "wms_picks_task_id_fkey" FOREIGN KEY ("task_id") REFERENCES "wms_tasks"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE "wms_picks" ADD CONSTRAINT "wms_picks_handling_unit_id_fkey" FOREIGN KEY ("handling_unit_id") REFERENCES "wms_handling_units"("id") ON DELETE SET NULL ON UPDATE CASCADE;
ALTER TABLE "wms_movements" ADD CONSTRAINT "wms_movements_task_id_fkey" FOREIGN KEY ("task_id") REFERENCES "wms_tasks"("id") ON DELETE SET NULL ON UPDATE CASCADE;
ALTER TABLE "wms_movements" ADD CONSTRAINT "wms_movements_handling_unit_id_fkey" FOREIGN KEY ("handling_unit_id") REFERENCES "wms_handling_units"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- MDP QMS module — additive DDL (qms_ tables only, 0 DROP).
-- Extracted from prisma migrate diff; warehouse drift/DROP excluded per
-- apps/web-mdp CLAUDE.md migration discipline. Source: mdp-qms.prisma.

-- CreateEnum
CREATE TYPE "MdpQmsInspectionType" AS ENUM ('INCOMING', 'IN_PROCESS', 'FINAL');

-- CreateEnum
CREATE TYPE "MdpQmsInspectionVerdict" AS ENUM ('PENDING', 'PASS', 'FAIL');

-- CreateEnum
CREATE TYPE "MdpQmsCharacteristicType" AS ENUM ('VARIABLE', 'ATTRIBUTE');

-- CreateEnum
CREATE TYPE "MdpQmsResultStatus" AS ENUM ('PASS', 'FAIL', 'NA');

-- CreateEnum
CREATE TYPE "MdpQmsNcrSeverity" AS ENUM ('MINOR', 'MAJOR', 'CRITICAL');

-- CreateEnum
CREATE TYPE "MdpQmsNcrStatus" AS ENUM ('OPEN', 'UNDER_REVIEW', 'CONTAINED', 'CLOSED', 'CANCELLED');

-- CreateEnum
CREATE TYPE "MdpQmsDisposition" AS ENUM ('PENDING', 'USE_AS_IS', 'REWORK', 'REPAIR', 'SCRAP', 'RETURN_TO_SUPPLIER');

-- CreateEnum
CREATE TYPE "MdpQmsCapaType" AS ENUM ('CORRECTIVE', 'PREVENTIVE');

-- CreateEnum
CREATE TYPE "MdpQmsCapaStatus" AS ENUM ('OPEN', 'IN_PROGRESS', 'IMPLEMENTED', 'VERIFIED', 'CLOSED', 'CANCELLED');

-- CreateTable
CREATE TABLE "qms_inspection_plans" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "type" "MdpQmsInspectionType" NOT NULL,
    "item_id" BIGINT,
    "operation_id" BIGINT,
    "description" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "qms_inspection_plans_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "qms_inspection_characteristics" (
    "id" BIGSERIAL NOT NULL,
    "plan_id" BIGINT NOT NULL,
    "sequence" INTEGER NOT NULL DEFAULT 0,
    "name" TEXT NOT NULL,
    "characteristic_type" "MdpQmsCharacteristicType" NOT NULL DEFAULT 'VARIABLE',
    "uom_code" TEXT,
    "nominal" DECIMAL(19,4),
    "lower_limit" DECIMAL(19,4),
    "upper_limit" DECIMAL(19,4),
    "notes" TEXT,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "qms_inspection_characteristics_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "qms_inspections" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "plan_id" BIGINT,
    "type" "MdpQmsInspectionType" NOT NULL,
    "item_id" BIGINT,
    "production_order_id" BIGINT,
    "lot_code" TEXT,
    "lot_size" DECIMAL(19,4),
    "sample_size" DECIMAL(19,4),
    "result" "MdpQmsInspectionVerdict" NOT NULL DEFAULT 'PENDING',
    "inspected_at" TIMESTAMPTZ(6) NOT NULL,
    "inspected_by_id" BIGINT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "qms_inspections_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "qms_inspection_results" (
    "id" BIGSERIAL NOT NULL,
    "inspection_id" BIGINT NOT NULL,
    "characteristic_id" BIGINT,
    "measured_value" DECIMAL(19,4),
    "status" "MdpQmsResultStatus" NOT NULL DEFAULT 'PASS',
    "notes" TEXT,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "qms_inspection_results_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "qms_nonconformances" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "description" TEXT,
    "severity" "MdpQmsNcrSeverity" NOT NULL DEFAULT 'MINOR',
    "status" "MdpQmsNcrStatus" NOT NULL DEFAULT 'OPEN',
    "disposition" "MdpQmsDisposition" NOT NULL DEFAULT 'PENDING',
    "source_type" TEXT,
    "item_id" BIGINT,
    "production_order_id" BIGINT,
    "inspection_id" BIGINT,
    "qty_affected" DECIMAL(19,4),
    "erp_reference_type" TEXT,
    "erp_reference_id" BIGINT,
    "detected_at" TIMESTAMPTZ(6) NOT NULL,
    "detected_by_id" BIGINT,
    "closed_at" TIMESTAMPTZ(6),
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "qms_nonconformances_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "qms_capa_actions" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "nonconformance_id" BIGINT,
    "type" "MdpQmsCapaType" NOT NULL DEFAULT 'CORRECTIVE',
    "status" "MdpQmsCapaStatus" NOT NULL DEFAULT 'OPEN',
    "description" TEXT,
    "root_cause" TEXT,
    "action_plan" TEXT,
    "assigned_to_id" BIGINT,
    "due_date" TIMESTAMPTZ(6),
    "completed_at" TIMESTAMPTZ(6),
    "verified_by_id" BIGINT,
    "verified_at" TIMESTAMPTZ(6),
    "effectiveness" TEXT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "qms_capa_actions_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "qms_inspection_plans_code_key" ON "qms_inspection_plans"("code");

-- CreateIndex
CREATE INDEX "qms_inspection_plans_type_idx" ON "qms_inspection_plans"("type");

-- CreateIndex
CREATE INDEX "qms_inspection_plans_item_id_idx" ON "qms_inspection_plans"("item_id");

-- CreateIndex
CREATE INDEX "qms_inspection_plans_operation_id_idx" ON "qms_inspection_plans"("operation_id");

-- CreateIndex
CREATE INDEX "qms_inspection_characteristics_plan_id_idx" ON "qms_inspection_characteristics"("plan_id");

-- CreateIndex
CREATE UNIQUE INDEX "qms_inspections_code_key" ON "qms_inspections"("code");

-- CreateIndex
CREATE INDEX "qms_inspections_plan_id_idx" ON "qms_inspections"("plan_id");

-- CreateIndex
CREATE INDEX "qms_inspections_type_idx" ON "qms_inspections"("type");

-- CreateIndex
CREATE INDEX "qms_inspections_result_idx" ON "qms_inspections"("result");

-- CreateIndex
CREATE INDEX "qms_inspections_item_id_idx" ON "qms_inspections"("item_id");

-- CreateIndex
CREATE INDEX "qms_inspections_production_order_id_idx" ON "qms_inspections"("production_order_id");

-- CreateIndex
CREATE INDEX "qms_inspection_results_inspection_id_idx" ON "qms_inspection_results"("inspection_id");

-- CreateIndex
CREATE INDEX "qms_inspection_results_characteristic_id_idx" ON "qms_inspection_results"("characteristic_id");

-- CreateIndex
CREATE INDEX "qms_inspection_results_status_idx" ON "qms_inspection_results"("status");

-- CreateIndex
CREATE UNIQUE INDEX "qms_nonconformances_code_key" ON "qms_nonconformances"("code");

-- CreateIndex
CREATE INDEX "qms_nonconformances_status_idx" ON "qms_nonconformances"("status");

-- CreateIndex
CREATE INDEX "qms_nonconformances_severity_idx" ON "qms_nonconformances"("severity");

-- CreateIndex
CREATE INDEX "qms_nonconformances_disposition_idx" ON "qms_nonconformances"("disposition");

-- CreateIndex
CREATE INDEX "qms_nonconformances_item_id_idx" ON "qms_nonconformances"("item_id");

-- CreateIndex
CREATE INDEX "qms_nonconformances_production_order_id_idx" ON "qms_nonconformances"("production_order_id");

-- CreateIndex
CREATE INDEX "qms_nonconformances_inspection_id_idx" ON "qms_nonconformances"("inspection_id");

-- CreateIndex
CREATE UNIQUE INDEX "qms_capa_actions_code_key" ON "qms_capa_actions"("code");

-- CreateIndex
CREATE INDEX "qms_capa_actions_nonconformance_id_idx" ON "qms_capa_actions"("nonconformance_id");

-- CreateIndex
CREATE INDEX "qms_capa_actions_type_idx" ON "qms_capa_actions"("type");

-- CreateIndex
CREATE INDEX "qms_capa_actions_status_idx" ON "qms_capa_actions"("status");

-- CreateIndex
CREATE INDEX "qms_capa_actions_assigned_to_id_idx" ON "qms_capa_actions"("assigned_to_id");

-- AddForeignKey
ALTER TABLE "qms_inspection_characteristics" ADD CONSTRAINT "qms_inspection_characteristics_plan_id_fkey" FOREIGN KEY ("plan_id") REFERENCES "qms_inspection_plans"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "qms_inspections" ADD CONSTRAINT "qms_inspections_plan_id_fkey" FOREIGN KEY ("plan_id") REFERENCES "qms_inspection_plans"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "qms_inspection_results" ADD CONSTRAINT "qms_inspection_results_inspection_id_fkey" FOREIGN KEY ("inspection_id") REFERENCES "qms_inspections"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "qms_inspection_results" ADD CONSTRAINT "qms_inspection_results_characteristic_id_fkey" FOREIGN KEY ("characteristic_id") REFERENCES "qms_inspection_characteristics"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "qms_nonconformances" ADD CONSTRAINT "qms_nonconformances_inspection_id_fkey" FOREIGN KEY ("inspection_id") REFERENCES "qms_inspections"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "qms_capa_actions" ADD CONSTRAINT "qms_capa_actions_nonconformance_id_fkey" FOREIGN KEY ("nonconformance_id") REFERENCES "qms_nonconformances"("id") ON DELETE SET NULL ON UPDATE CASCADE;

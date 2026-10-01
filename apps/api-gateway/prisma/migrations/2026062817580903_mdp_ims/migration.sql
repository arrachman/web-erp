-- MDP IMS module — additive DDL (module tables only, 0 DROP).
-- Source: mdp-ims.prisma. Extracted per apps/web-mdp migration discipline.


-- CreateEnum
CREATE TYPE "MdpEhsIncidentType" AS ENUM ('INJURY', 'NEAR_MISS', 'PROPERTY_DAMAGE', 'ENVIRONMENTAL', 'SECURITY', 'OTHER');

-- CreateEnum
CREATE TYPE "MdpEhsSeverity" AS ENUM ('MINOR', 'MODERATE', 'MAJOR', 'FATAL');

-- CreateEnum
CREATE TYPE "MdpEhsIncidentStatus" AS ENUM ('REPORTED', 'UNDER_INVESTIGATION', 'ACTION_PENDING', 'CLOSED', 'CANCELLED');

-- CreateEnum
CREATE TYPE "MdpEhsAuditType" AS ENUM ('SAFETY', 'ENVIRONMENTAL', 'QUALITY', 'FIVE_S', 'INTERNAL', 'EXTERNAL');

-- CreateEnum
CREATE TYPE "MdpEhsAuditStatus" AS ENUM ('PLANNED', 'IN_PROGRESS', 'COMPLETED', 'CANCELLED');

-- CreateEnum
CREATE TYPE "MdpEhsPermitType" AS ENUM ('HOT_WORK', 'CONFINED_SPACE', 'WORKING_AT_HEIGHT', 'ELECTRICAL', 'EXCAVATION', 'CHEMICAL', 'OTHER');

-- CreateEnum
CREATE TYPE "MdpEhsPermitStatus" AS ENUM ('REQUESTED', 'APPROVED', 'ACTIVE', 'CLOSED', 'EXPIRED', 'REJECTED', 'CANCELLED');

-- CreateTable
CREATE TABLE "ehs_incidents" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "type" "MdpEhsIncidentType" NOT NULL,
    "severity" "MdpEhsSeverity" NOT NULL DEFAULT 'MINOR',
    "status" "MdpEhsIncidentStatus" NOT NULL DEFAULT 'REPORTED',
    "asset_id" BIGINT,
    "work_center_id" BIGINT,
    "location" TEXT,
    "description" TEXT,
    "occurred_at" TIMESTAMPTZ(6) NOT NULL,
    "reported_by_id" BIGINT,
    "investigated_by_id" BIGINT,
    "root_cause" TEXT,
    "corrective_action" TEXT,
    "closed_at" TIMESTAMPTZ(6),
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "ehs_incidents_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "ehs_audits" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "type" "MdpEhsAuditType" NOT NULL,
    "status" "MdpEhsAuditStatus" NOT NULL DEFAULT 'PLANNED',
    "scope" TEXT,
    "work_center_id" BIGINT,
    "auditor_id" BIGINT,
    "scheduled_at" TIMESTAMPTZ(6),
    "conducted_at" TIMESTAMPTZ(6),
    "score" DECIMAL(19,4),
    "findings" TEXT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "ehs_audits_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "ehs_permits" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "type" "MdpEhsPermitType" NOT NULL,
    "status" "MdpEhsPermitStatus" NOT NULL DEFAULT 'REQUESTED',
    "asset_id" BIGINT,
    "work_center_id" BIGINT,
    "location" TEXT,
    "requested_by_id" BIGINT,
    "approved_by_id" BIGINT,
    "valid_from" TIMESTAMPTZ(6),
    "valid_to" TIMESTAMPTZ(6),
    "description" TEXT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "ehs_permits_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "ehs_incidents_code_key" ON "ehs_incidents"("code");

-- CreateIndex
CREATE INDEX "ehs_incidents_type_idx" ON "ehs_incidents"("type");

-- CreateIndex
CREATE INDEX "ehs_incidents_severity_idx" ON "ehs_incidents"("severity");

-- CreateIndex
CREATE INDEX "ehs_incidents_status_idx" ON "ehs_incidents"("status");

-- CreateIndex
CREATE INDEX "ehs_incidents_asset_id_idx" ON "ehs_incidents"("asset_id");

-- CreateIndex
CREATE UNIQUE INDEX "ehs_audits_code_key" ON "ehs_audits"("code");

-- CreateIndex
CREATE INDEX "ehs_audits_type_idx" ON "ehs_audits"("type");

-- CreateIndex
CREATE INDEX "ehs_audits_status_idx" ON "ehs_audits"("status");

-- CreateIndex
CREATE UNIQUE INDEX "ehs_permits_code_key" ON "ehs_permits"("code");

-- CreateIndex
CREATE INDEX "ehs_permits_type_idx" ON "ehs_permits"("type");

-- CreateIndex
CREATE INDEX "ehs_permits_status_idx" ON "ehs_permits"("status");

-- CreateIndex
CREATE INDEX "ehs_permits_asset_id_idx" ON "ehs_permits"("asset_id");

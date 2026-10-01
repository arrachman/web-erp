-- MDP PRTS module — additive DDL (module tables only, 0 DROP).
-- Source: mdp-prts.prisma. Extracted per apps/web-mdp migration discipline.


-- CreateEnum
CREATE TYPE "MdpPrtIssueType" AS ENUM ('QUALITY', 'MACHINE', 'SAFETY', 'MATERIAL', 'PROCESS', 'OTHER');

-- CreateEnum
CREATE TYPE "MdpPrtSeverity" AS ENUM ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL');

-- CreateEnum
CREATE TYPE "MdpPrtIssueStatus" AS ENUM ('OPEN', 'ACKNOWLEDGED', 'IN_PROGRESS', 'RESOLVED', 'CLOSED', 'CANCELLED');

-- CreateEnum
CREATE TYPE "MdpPrtEscalationStatus" AS ENUM ('PENDING', 'ACKNOWLEDGED', 'RESOLVED');

-- CreateTable
CREATE TABLE "prt_issues" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "type" "MdpPrtIssueType" NOT NULL,
    "severity" "MdpPrtSeverity" NOT NULL DEFAULT 'MEDIUM',
    "status" "MdpPrtIssueStatus" NOT NULL DEFAULT 'OPEN',
    "source" TEXT,
    "asset_id" BIGINT,
    "work_center_id" BIGINT,
    "production_order_id" BIGINT,
    "description" TEXT,
    "reported_by_id" BIGINT,
    "assigned_to_id" BIGINT,
    "raised_at" TIMESTAMPTZ(6) NOT NULL,
    "resolved_at" TIMESTAMPTZ(6),
    "resolution" TEXT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "prt_issues_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "prt_escalations" (
    "id" BIGSERIAL NOT NULL,
    "issue_id" BIGINT NOT NULL,
    "level" INTEGER NOT NULL DEFAULT 1,
    "escalated_to_id" BIGINT,
    "escalated_at" TIMESTAMPTZ(6) NOT NULL,
    "due_at" TIMESTAMPTZ(6),
    "status" "MdpPrtEscalationStatus" NOT NULL DEFAULT 'PENDING',
    "reason" TEXT,
    "notes" TEXT,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "prt_escalations_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "prt_issues_code_key" ON "prt_issues"("code");

-- CreateIndex
CREATE INDEX "prt_issues_type_idx" ON "prt_issues"("type");

-- CreateIndex
CREATE INDEX "prt_issues_severity_idx" ON "prt_issues"("severity");

-- CreateIndex
CREATE INDEX "prt_issues_status_idx" ON "prt_issues"("status");

-- CreateIndex
CREATE INDEX "prt_issues_asset_id_idx" ON "prt_issues"("asset_id");

-- CreateIndex
CREATE INDEX "prt_issues_production_order_id_idx" ON "prt_issues"("production_order_id");

-- CreateIndex
CREATE INDEX "prt_issues_assigned_to_id_idx" ON "prt_issues"("assigned_to_id");

-- CreateIndex
CREATE INDEX "prt_escalations_issue_id_idx" ON "prt_escalations"("issue_id");

-- CreateIndex
CREATE INDEX "prt_escalations_status_idx" ON "prt_escalations"("status");

-- AddForeignKey
ALTER TABLE "prt_escalations" ADD CONSTRAINT "prt_escalations_issue_id_fkey" FOREIGN KEY ("issue_id") REFERENCES "prt_issues"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

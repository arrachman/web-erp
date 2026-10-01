-- MDP DMS module — additive DDL (module tables only, 0 DROP).
-- Source: mdp-dms.prisma. Extracted per apps/web-mdp migration discipline.

-- CreateEnum
CREATE TYPE "MdpDmsCategory" AS ENUM ('SOP', 'WORK_INSTRUCTION', 'DRAWING', 'POLICY', 'FORM', 'RECORD', 'OTHER');

-- CreateEnum
CREATE TYPE "MdpDmsDocStatus" AS ENUM ('DRAFT', 'IN_REVIEW', 'APPROVED', 'RELEASED', 'OBSOLETE');

-- CreateEnum
CREATE TYPE "MdpDmsRevisionStatus" AS ENUM ('DRAFT', 'IN_REVIEW', 'APPROVED', 'SUPERSEDED');

-- CreateTable
CREATE TABLE "dms_documents" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "category" "MdpDmsCategory",
    "status" "MdpDmsDocStatus" NOT NULL DEFAULT 'DRAFT',
    "current_revision" TEXT,
    "owner_id" BIGINT,
    "description" TEXT,
    "effective_at" TIMESTAMPTZ(6),
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "dms_documents_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "dms_revisions" (
    "id" BIGSERIAL NOT NULL,
    "document_id" BIGINT NOT NULL,
    "revision_code" TEXT NOT NULL,
    "status" "MdpDmsRevisionStatus" NOT NULL DEFAULT 'DRAFT',
    "file_path" TEXT,
    "change_summary" TEXT,
    "approved_by_id" BIGINT,
    "approved_at" TIMESTAMPTZ(6),
    "notes" TEXT,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "dms_revisions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "dms_acknowledgements" (
    "id" BIGSERIAL NOT NULL,
    "document_id" BIGINT NOT NULL,
    "revision_id" BIGINT,
    "user_id" BIGINT NOT NULL,
    "acknowledged_at" TIMESTAMPTZ(6) NOT NULL,
    "notes" TEXT,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "dms_acknowledgements_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "dms_documents_code_key" ON "dms_documents"("code");

-- CreateIndex
CREATE INDEX "dms_documents_category_idx" ON "dms_documents"("category");

-- CreateIndex
CREATE INDEX "dms_documents_status_idx" ON "dms_documents"("status");

-- CreateIndex
CREATE INDEX "dms_revisions_document_id_idx" ON "dms_revisions"("document_id");

-- CreateIndex
CREATE INDEX "dms_revisions_status_idx" ON "dms_revisions"("status");

-- CreateIndex
CREATE INDEX "dms_acknowledgements_document_id_idx" ON "dms_acknowledgements"("document_id");

-- CreateIndex
CREATE INDEX "dms_acknowledgements_revision_id_idx" ON "dms_acknowledgements"("revision_id");

-- CreateIndex
CREATE INDEX "dms_acknowledgements_user_id_idx" ON "dms_acknowledgements"("user_id");

-- AddForeignKey
ALTER TABLE "dms_revisions" ADD CONSTRAINT "dms_revisions_document_id_fkey" FOREIGN KEY ("document_id") REFERENCES "dms_documents"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "dms_acknowledgements" ADD CONSTRAINT "dms_acknowledgements_document_id_fkey" FOREIGN KEY ("document_id") REFERENCES "dms_documents"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "dms_acknowledgements" ADD CONSTRAINT "dms_acknowledgements_revision_id_fkey" FOREIGN KEY ("revision_id") REFERENCES "dms_revisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

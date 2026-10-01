-- CreateTable
CREATE TABLE "md_classes" (
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

    CONSTRAINT "md_classes_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "md_classes_code_key" ON "md_classes"("code");

-- CreateIndex
CREATE INDEX "md_classes_legacy_code_idx" ON "md_classes"("legacy_code");

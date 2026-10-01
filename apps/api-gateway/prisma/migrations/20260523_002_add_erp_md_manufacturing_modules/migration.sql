-- CreateTable
CREATE TABLE "md_production_categories" (
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

    CONSTRAINT "md_production_categories_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_point_categories" (
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

    CONSTRAINT "md_point_categories_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_miscellaneous" (
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

    CONSTRAINT "md_miscellaneous_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_sub_classes" (
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

    CONSTRAINT "md_sub_classes_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_work_estimates" (
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

    CONSTRAINT "md_work_estimates_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_labors" (
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

    CONSTRAINT "md_labors_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_machines" (
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

    CONSTRAINT "md_machines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_designers" (
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

    CONSTRAINT "md_designers_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_production_activities" (
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

    CONSTRAINT "md_production_activities_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_production_routes" (
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

    CONSTRAINT "md_production_routes_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "md_production_categories_code_key" ON "md_production_categories"("code");

-- CreateIndex
CREATE INDEX "md_production_categories_legacy_code_idx" ON "md_production_categories"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_point_categories_code_key" ON "md_point_categories"("code");

-- CreateIndex
CREATE INDEX "md_point_categories_legacy_code_idx" ON "md_point_categories"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_miscellaneous_code_key" ON "md_miscellaneous"("code");

-- CreateIndex
CREATE INDEX "md_miscellaneous_legacy_code_idx" ON "md_miscellaneous"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_sub_classes_code_key" ON "md_sub_classes"("code");

-- CreateIndex
CREATE INDEX "md_sub_classes_legacy_code_idx" ON "md_sub_classes"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_work_estimates_code_key" ON "md_work_estimates"("code");

-- CreateIndex
CREATE INDEX "md_work_estimates_legacy_code_idx" ON "md_work_estimates"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_labors_code_key" ON "md_labors"("code");

-- CreateIndex
CREATE INDEX "md_labors_legacy_code_idx" ON "md_labors"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_machines_code_key" ON "md_machines"("code");

-- CreateIndex
CREATE INDEX "md_machines_legacy_code_idx" ON "md_machines"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_designers_code_key" ON "md_designers"("code");

-- CreateIndex
CREATE INDEX "md_designers_legacy_code_idx" ON "md_designers"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_production_activities_code_key" ON "md_production_activities"("code");

-- CreateIndex
CREATE INDEX "md_production_activities_legacy_code_idx" ON "md_production_activities"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_production_routes_code_key" ON "md_production_routes"("code");

-- CreateIndex
CREATE INDEX "md_production_routes_legacy_code_idx" ON "md_production_routes"("legacy_code");

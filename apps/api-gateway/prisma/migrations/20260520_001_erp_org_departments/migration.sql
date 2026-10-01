-- ERP Organization masters: Department & SubDepartment
-- Additive only; mirrors shape of md_cost_centers / md_subdivisions.

CREATE TABLE "md_departments" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "parent_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_departments_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_departments_code_key" ON "md_departments"("code");
CREATE INDEX "md_departments_parent_id_idx" ON "md_departments"("parent_id");
CREATE INDEX "md_departments_legacy_code_idx" ON "md_departments"("legacy_code");

ALTER TABLE "md_departments" ADD CONSTRAINT "md_departments_parent_id_fkey"
    FOREIGN KEY ("parent_id") REFERENCES "md_departments"("id")
    ON DELETE SET NULL ON UPDATE CASCADE;

CREATE TABLE "md_sub_departments" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "department_id" BIGINT NOT NULL,
    "parent_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_sub_departments_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_sub_departments_code_key" ON "md_sub_departments"("code");
CREATE INDEX "md_sub_departments_department_id_idx" ON "md_sub_departments"("department_id");
CREATE INDEX "md_sub_departments_parent_id_idx" ON "md_sub_departments"("parent_id");
CREATE INDEX "md_sub_departments_legacy_code_idx" ON "md_sub_departments"("legacy_code");

ALTER TABLE "md_sub_departments" ADD CONSTRAINT "md_sub_departments_department_id_fkey"
    FOREIGN KEY ("department_id") REFERENCES "md_departments"("id")
    ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "md_sub_departments" ADD CONSTRAINT "md_sub_departments_parent_id_fkey"
    FOREIGN KEY ("parent_id") REFERENCES "md_sub_departments"("id")
    ON DELETE SET NULL ON UPDATE CASCADE;

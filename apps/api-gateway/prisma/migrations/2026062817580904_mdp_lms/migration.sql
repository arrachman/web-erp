-- MDP LMS module — additive DDL (module tables only, 0 DROP).
-- Source: mdp-lms.prisma. Extracted per apps/web-mdp migration discipline.


-- CreateEnum
CREATE TYPE "MdpLmsCourseCategory" AS ENUM ('SAFETY', 'QUALITY', 'TECHNICAL', 'ONBOARDING', 'COMPLIANCE', 'OTHER');

-- CreateEnum
CREATE TYPE "MdpLmsCourseStatus" AS ENUM ('DRAFT', 'ACTIVE', 'ARCHIVED');

-- CreateEnum
CREATE TYPE "MdpLmsEnrollmentStatus" AS ENUM ('ENROLLED', 'IN_PROGRESS', 'COMPLETED', 'FAILED', 'EXPIRED');

-- CreateTable
CREATE TABLE "lms_courses" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "category" "MdpLmsCourseCategory",
    "status" "MdpLmsCourseStatus" NOT NULL DEFAULT 'DRAFT',
    "description" TEXT,
    "duration_hours" DECIMAL(19,4),
    "is_mandatory" BOOLEAN NOT NULL DEFAULT false,
    "validity_months" INTEGER,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "lms_courses_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "lms_enrollments" (
    "id" BIGSERIAL NOT NULL,
    "course_id" BIGINT NOT NULL,
    "user_id" BIGINT NOT NULL,
    "status" "MdpLmsEnrollmentStatus" NOT NULL DEFAULT 'ENROLLED',
    "progress_pct" DECIMAL(19,4),
    "enrolled_at" TIMESTAMPTZ(6) NOT NULL,
    "completed_at" TIMESTAMPTZ(6),
    "score" DECIMAL(19,4),
    "certificate_code" TEXT,
    "expires_at" TIMESTAMPTZ(6),
    "notes" TEXT,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "lms_enrollments_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "lms_competencies" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "category" TEXT,
    "description" TEXT,
    "required_course_id" BIGINT,
    "level" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "lms_competencies_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "lms_courses_code_key" ON "lms_courses"("code");

-- CreateIndex
CREATE INDEX "lms_courses_category_idx" ON "lms_courses"("category");

-- CreateIndex
CREATE INDEX "lms_courses_status_idx" ON "lms_courses"("status");

-- CreateIndex
CREATE INDEX "lms_enrollments_course_id_idx" ON "lms_enrollments"("course_id");

-- CreateIndex
CREATE INDEX "lms_enrollments_user_id_idx" ON "lms_enrollments"("user_id");

-- CreateIndex
CREATE INDEX "lms_enrollments_status_idx" ON "lms_enrollments"("status");

-- CreateIndex
CREATE UNIQUE INDEX "lms_competencies_code_key" ON "lms_competencies"("code");

-- CreateIndex
CREATE INDEX "lms_competencies_required_course_id_idx" ON "lms_competencies"("required_course_id");

-- AddForeignKey
ALTER TABLE "lms_enrollments" ADD CONSTRAINT "lms_enrollments_course_id_fkey" FOREIGN KEY ("course_id") REFERENCES "lms_courses"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "lms_competencies" ADD CONSTRAINT "lms_competencies_required_course_id_fkey" FOREIGN KEY ("required_course_id") REFERENCES "lms_courses"("id") ON DELETE SET NULL ON UPDATE CASCADE;

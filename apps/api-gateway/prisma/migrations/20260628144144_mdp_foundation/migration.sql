-- Senti MDP foundation (additive): work calendars + nav SSOT + thin access map.
-- Extracted from `prisma migrate diff` (datasource→datamodel), keeping ONLY the
-- new mdp_* tables. ALL warehouse DROP/drift statements were discarded by hand
-- (live DB is not fully schema-managed — see prisma-live-db-not-schema-managed).
-- 0 DROP. FKs reference only mdp_menus (intra-MDP). No new enum types.

-- CreateTable
CREATE TABLE "mdp_work_calendars" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "description" TEXT,
    "work_center_id" BIGINT,
    "shift_id" BIGINT,
    "planned_minutes_per_day" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "working_days_per_week" INTEGER NOT NULL DEFAULT 7,
    "effective_from" TIMESTAMPTZ(6),
    "effective_to" TIMESTAMPTZ(6),
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mdp_work_calendars_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mdp_menus" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "parent_id" BIGINT,
    "path" TEXT,
    "icon" TEXT,
    "module_key" TEXT,
    "sequence" INTEGER NOT NULL DEFAULT 0,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mdp_menus_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mdp_role_menus" (
    "id" BIGSERIAL NOT NULL,
    "role_id" BIGINT NOT NULL,
    "menu_id" BIGINT NOT NULL,
    "can_view" BOOLEAN NOT NULL DEFAULT true,
    "can_edit" BOOLEAN NOT NULL DEFAULT false,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mdp_role_menus_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "mdp_work_calendars_code_key" ON "mdp_work_calendars"("code");

-- CreateIndex
CREATE INDEX "mdp_work_calendars_work_center_id_idx" ON "mdp_work_calendars"("work_center_id");

-- CreateIndex
CREATE INDEX "mdp_work_calendars_shift_id_idx" ON "mdp_work_calendars"("shift_id");

-- CreateIndex
CREATE UNIQUE INDEX "mdp_menus_code_key" ON "mdp_menus"("code");

-- CreateIndex
CREATE INDEX "mdp_menus_parent_id_idx" ON "mdp_menus"("parent_id");

-- CreateIndex
CREATE INDEX "mdp_menus_module_key_idx" ON "mdp_menus"("module_key");

-- CreateIndex
CREATE INDEX "mdp_role_menus_role_id_idx" ON "mdp_role_menus"("role_id");

-- CreateIndex
CREATE INDEX "mdp_role_menus_menu_id_idx" ON "mdp_role_menus"("menu_id");

-- CreateIndex
CREATE UNIQUE INDEX "mdp_role_menus_role_id_menu_id_key" ON "mdp_role_menus"("role_id", "menu_id");

-- AddForeignKey
ALTER TABLE "mdp_menus" ADD CONSTRAINT "mdp_menus_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "mdp_menus"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mdp_role_menus" ADD CONSTRAINT "mdp_role_menus_menu_id_fkey" FOREIGN KEY ("menu_id") REFERENCES "mdp_menus"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

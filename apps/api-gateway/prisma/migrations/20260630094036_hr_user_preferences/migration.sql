-- Senti HR: per-user appearance preferences (Setting → Tampilan).
-- 1:1 port of adm_user_preferences, scoped to HR / platform sf_token users.
-- Additive only (0 DROP). PK = platform user id (m0_users.id).
-- UI tweaks (primary/density/fontScale/sidebar/sidebarMenu/urlRouting) ride in
-- `metadata` JSON; theme/language are first-class columns (mirrors ERP).

CREATE TABLE IF NOT EXISTS "hr_user_preferences" (
    "user_id"    INTEGER              NOT NULL,
    "theme"      TEXT,
    "language"   VARCHAR(10),
    "metadata"   JSONB,
    "created_at" TIMESTAMPTZ(6)       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6)       NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "hr_user_preferences_pkey" PRIMARY KEY ("user_id")
);

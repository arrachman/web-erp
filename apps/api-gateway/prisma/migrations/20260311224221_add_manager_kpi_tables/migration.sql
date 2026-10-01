CREATE TABLE IF NOT EXISTS "m0_manager_insight" (
  "id" SERIAL PRIMARY KEY,
  "manager_user_id" INTEGER NULL REFERENCES "m0_users"("id") ON DELETE SET NULL,
  "title" TEXT NOT NULL,
  "question" TEXT NULL,
  "status" TEXT NOT NULL,
  "insight_created_at" TIMESTAMPTZ(3) NOT NULL,
  "decision_at" TIMESTAMPTZ(3) NULL,
  "decision_note" TEXT NULL,
  "created_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "idx_m0_manager_insight_created" ON "m0_manager_insight" ("insight_created_at");
CREATE INDEX IF NOT EXISTS "idx_m0_manager_insight_status_created" ON "m0_manager_insight" ("status", "insight_created_at");
CREATE INDEX IF NOT EXISTS "idx_m0_manager_insight_manager_created" ON "m0_manager_insight" ("manager_user_id", "insight_created_at");
CREATE UNIQUE INDEX IF NOT EXISTS "m0_manager_insight_manager_user_id_title_insight_created_at_key"
  ON "m0_manager_insight" ("manager_user_id", "title", "insight_created_at");
CREATE UNIQUE INDEX IF NOT EXISTS "m0_manager_insight_manager_user_id_title_insight_created_at_key"
  ON "m0_manager_insight" ("manager_user_id", "title", "insight_created_at");

CREATE TABLE IF NOT EXISTS "m0_manager_risk" (
  "id" SERIAL PRIMARY KEY,
  "manager_user_id" INTEGER NULL REFERENCES "m0_users"("id") ON DELETE SET NULL,
  "title" TEXT NOT NULL,
  "domain" TEXT NOT NULL,
  "severity" TEXT NOT NULL,
  "status" TEXT NOT NULL,
  "opened_at" TIMESTAMPTZ(3) NOT NULL,
  "resolved_at" TIMESTAMPTZ(3) NULL,
  "created_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "idx_m0_manager_risk_severity_status" ON "m0_manager_risk" ("severity", "status");
CREATE INDEX IF NOT EXISTS "idx_m0_manager_risk_opened" ON "m0_manager_risk" ("opened_at");
CREATE UNIQUE INDEX IF NOT EXISTS "m0_manager_risk_title_opened_at_key"
  ON "m0_manager_risk" ("title", "opened_at");
CREATE UNIQUE INDEX IF NOT EXISTS "m0_manager_risk_title_opened_at_key"
  ON "m0_manager_risk" ("title", "opened_at");

CREATE TABLE IF NOT EXISTS "m0_manager_data_freshness" (
  "id" SERIAL PRIMARY KEY,
  "domain" TEXT NOT NULL,
  "dataset_name" TEXT NOT NULL,
  "sla_minutes" INTEGER NOT NULL,
  "last_refresh_at" TIMESTAMPTZ(3) NOT NULL,
  "created_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT "m0_manager_data_freshness_domain_dataset_name_key" UNIQUE ("domain", "dataset_name")
);

CREATE INDEX IF NOT EXISTS "idx_m0_manager_data_freshness_refresh" ON "m0_manager_data_freshness" ("last_refresh_at");

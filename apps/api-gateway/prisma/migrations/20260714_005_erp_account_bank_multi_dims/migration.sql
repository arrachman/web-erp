-- Account CoA: bank FK (md_banks) + multi-dim scope (branch/location/division)
-- Additive only; is_control_account column retained (seed/AR-AP semantics).

ALTER TABLE "md_accounts" ADD COLUMN IF NOT EXISTS "bank_id" BIGINT;

CREATE INDEX IF NOT EXISTS "md_accounts_bank_id_idx" ON "md_accounts"("bank_id");

DO $$ BEGIN
  ALTER TABLE "md_accounts"
    ADD CONSTRAINT "md_accounts_bank_id_fkey"
    FOREIGN KEY ("bank_id") REFERENCES "md_banks"("id")
    ON DELETE SET NULL ON UPDATE CASCADE;
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

CREATE TABLE IF NOT EXISTS "md_account_dim_branches" (
    "id" BIGSERIAL NOT NULL,
    "account_id" BIGINT NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "md_account_dim_branches_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "md_account_dim_branches_account_id_branch_id_key"
  ON "md_account_dim_branches"("account_id", "branch_id");
CREATE INDEX IF NOT EXISTS "md_account_dim_branches_branch_id_idx"
  ON "md_account_dim_branches"("branch_id");

DO $$ BEGIN
  ALTER TABLE "md_account_dim_branches"
    ADD CONSTRAINT "md_account_dim_branches_account_id_fkey"
    FOREIGN KEY ("account_id") REFERENCES "md_accounts"("id")
    ON DELETE CASCADE ON UPDATE CASCADE;
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
  ALTER TABLE "md_account_dim_branches"
    ADD CONSTRAINT "md_account_dim_branches_branch_id_fkey"
    FOREIGN KEY ("branch_id") REFERENCES "md_branches"("id")
    ON DELETE RESTRICT ON UPDATE CASCADE;
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

CREATE TABLE IF NOT EXISTS "md_account_dim_locations" (
    "id" BIGSERIAL NOT NULL,
    "account_id" BIGINT NOT NULL,
    "location_id" BIGINT NOT NULL,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "md_account_dim_locations_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "md_account_dim_locations_account_id_location_id_key"
  ON "md_account_dim_locations"("account_id", "location_id");
CREATE INDEX IF NOT EXISTS "md_account_dim_locations_location_id_idx"
  ON "md_account_dim_locations"("location_id");

DO $$ BEGIN
  ALTER TABLE "md_account_dim_locations"
    ADD CONSTRAINT "md_account_dim_locations_account_id_fkey"
    FOREIGN KEY ("account_id") REFERENCES "md_accounts"("id")
    ON DELETE CASCADE ON UPDATE CASCADE;
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
  ALTER TABLE "md_account_dim_locations"
    ADD CONSTRAINT "md_account_dim_locations_location_id_fkey"
    FOREIGN KEY ("location_id") REFERENCES "md_locations"("id")
    ON DELETE RESTRICT ON UPDATE CASCADE;
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

CREATE TABLE IF NOT EXISTS "md_account_dim_divisions" (
    "id" BIGSERIAL NOT NULL,
    "account_id" BIGINT NOT NULL,
    "division_id" BIGINT NOT NULL,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "md_account_dim_divisions_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "md_account_dim_divisions_account_id_division_id_key"
  ON "md_account_dim_divisions"("account_id", "division_id");
CREATE INDEX IF NOT EXISTS "md_account_dim_divisions_division_id_idx"
  ON "md_account_dim_divisions"("division_id");

DO $$ BEGIN
  ALTER TABLE "md_account_dim_divisions"
    ADD CONSTRAINT "md_account_dim_divisions_account_id_fkey"
    FOREIGN KEY ("account_id") REFERENCES "md_accounts"("id")
    ON DELETE CASCADE ON UPDATE CASCADE;
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

DO $$ BEGIN
  ALTER TABLE "md_account_dim_divisions"
    ADD CONSTRAINT "md_account_dim_divisions_division_id_fkey"
    FOREIGN KEY ("division_id") REFERENCES "md_divisions"("id")
    ON DELETE RESTRICT ON UPDATE CASCADE;
EXCEPTION
  WHEN duplicate_object THEN NULL;
END $$;

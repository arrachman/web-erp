BEGIN;

-- P0 timezone migration (phase 1, non-destructive):
-- 1) Add new timestamptz shadow columns
-- 2) Backfill existing rows with explicit source-timezone assumption
-- 3) Keep shadow columns in sync for new writes via triggers
-- 4) Add indexes for future read-path cutover
--
-- IMPORTANT:
-- - This migration does NOT drop or rename old columns.
-- - If historical data is local GMT+7 (not UTC), change the return value
--   in fn_p0_source_timezone() before running in production.

CREATE OR REPLACE FUNCTION public.fn_p0_source_timezone()
RETURNS TEXT
LANGUAGE sql
IMMUTABLE
AS $$
  SELECT 'UTC'::TEXT
$$;

DO $$
DECLARE
  v_source_timezone TEXT := public.fn_p0_source_timezone();
BEGIN
  -- 1) Add shadow columns
  ALTER TABLE public."m0_session"
    ADD COLUMN IF NOT EXISTS "expires_at_tz" TIMESTAMPTZ(3);

  ALTER TABLE public."m2_inventory_ledger"
    ADD COLUMN IF NOT EXISTS "transaction_date_tz" TIMESTAMPTZ(3);

  ALTER TABLE public."m0_users"
    ADD COLUMN IF NOT EXISTS "last_login_tz" TIMESTAMPTZ(3);

  -- 2) Backfill existing rows (idempotent)
  EXECUTE format(
    'UPDATE public."m0_session"
       SET "expires_at_tz" = ("expires_at" AT TIME ZONE %L)
     WHERE "expires_at" IS NOT NULL
       AND "expires_at_tz" IS NULL',
    v_source_timezone
  );

  EXECUTE format(
    'UPDATE public."m2_inventory_ledger"
       SET "transaction_date_tz" = ("transaction_date" AT TIME ZONE %L)
     WHERE "transaction_date" IS NOT NULL
       AND "transaction_date_tz" IS NULL',
    v_source_timezone
  );

  EXECUTE format(
    'UPDATE public."m0_users"
       SET "last_login_tz" = ("last_login" AT TIME ZONE %L)
     WHERE "last_login" IS NOT NULL
       AND "last_login_tz" IS NULL',
    v_source_timezone
  );
END $$;

-- 3) Sync trigger function: old timestamp columns -> new timestamptz columns
CREATE OR REPLACE FUNCTION public.fn_sync_p0_timestamptz_shadow()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
  IF TG_TABLE_NAME = 'm0_session' THEN
    IF TG_OP = 'INSERT' OR NEW."expires_at" IS DISTINCT FROM OLD."expires_at" THEN
      NEW."expires_at_tz" := CASE
        WHEN NEW."expires_at" IS NULL THEN NULL
        ELSE NEW."expires_at" AT TIME ZONE public.fn_p0_source_timezone()
      END;
    END IF;
    RETURN NEW;
  ELSIF TG_TABLE_NAME = 'm2_inventory_ledger' THEN
    IF TG_OP = 'INSERT' OR NEW."transaction_date" IS DISTINCT FROM OLD."transaction_date" THEN
      NEW."transaction_date_tz" := CASE
        WHEN NEW."transaction_date" IS NULL THEN NULL
        ELSE NEW."transaction_date" AT TIME ZONE public.fn_p0_source_timezone()
      END;
    END IF;
    RETURN NEW;
  ELSIF TG_TABLE_NAME = 'm0_users' THEN
    IF TG_OP = 'INSERT' OR NEW."last_login" IS DISTINCT FROM OLD."last_login" THEN
      NEW."last_login_tz" := CASE
        WHEN NEW."last_login" IS NULL THEN NULL
        ELSE NEW."last_login" AT TIME ZONE public.fn_p0_source_timezone()
      END;
    END IF;
    RETURN NEW;
  END IF;

  RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS tr_sync_m0_session_expires_at_tz ON public."m0_session";
CREATE TRIGGER tr_sync_m0_session_expires_at_tz
BEFORE INSERT OR UPDATE OF "expires_at" ON public."m0_session"
FOR EACH ROW
EXECUTE FUNCTION public.fn_sync_p0_timestamptz_shadow();

DROP TRIGGER IF EXISTS tr_sync_m2_inventory_ledger_transaction_date_tz ON public."m2_inventory_ledger";
CREATE TRIGGER tr_sync_m2_inventory_ledger_transaction_date_tz
BEFORE INSERT OR UPDATE OF "transaction_date" ON public."m2_inventory_ledger"
FOR EACH ROW
EXECUTE FUNCTION public.fn_sync_p0_timestamptz_shadow();

DROP TRIGGER IF EXISTS tr_sync_m0_users_last_login_tz ON public."m0_users";
CREATE TRIGGER tr_sync_m0_users_last_login_tz
BEFORE INSERT OR UPDATE OF "last_login" ON public."m0_users"
FOR EACH ROW
EXECUTE FUNCTION public.fn_sync_p0_timestamptz_shadow();

-- 4) Indexes for read-path migration
CREATE INDEX IF NOT EXISTS "m0_session_expires_at_tz_idx"
  ON public."m0_session" ("expires_at_tz");

CREATE INDEX IF NOT EXISTS "m2_inventory_ledger_transaction_date_tz_idx"
  ON public."m2_inventory_ledger" ("transaction_date_tz");

CREATE INDEX IF NOT EXISTS "m0_users_last_login_tz_idx"
  ON public."m0_users" ("last_login_tz");

COMMIT;

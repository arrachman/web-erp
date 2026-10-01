-- Idempotent fix: original migration timestamp was earlier than
-- 20260214195500_add_warehouse_id_to_users. Shadow DB replay in timestamp
-- order caused "column warehouse_id does not exist".
--
-- Strategy: add constraint only if column exists AND constraint absent.
-- Safe to replay across shadow + production environments.
-- See .planning/ADRs/003 — clinic-* roles can have NULL warehouse_id.
DO $$
BEGIN
  IF EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_name = 'm0_users' AND column_name = 'warehouse_id'
  ) AND NOT EXISTS (
    SELECT 1 FROM pg_constraint
    WHERE conname = 'chk_m0_users_active_requires_warehouse'
  ) THEN
    EXECUTE 'ALTER TABLE "m0_users"
      ADD CONSTRAINT "chk_m0_users_active_requires_warehouse"
      CHECK (NOT "is_active" OR "warehouse_id" IS NOT NULL)';
  END IF;
END $$;

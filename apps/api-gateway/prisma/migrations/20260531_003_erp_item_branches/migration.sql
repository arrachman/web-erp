-- ERP item branches (legacy MyERP+ item master "Branch" tab): per-item rows of
-- (Cabang, Cost Center). branch_id -> md_branches (Cabang), cost_center_id ->
-- md_cost_centers (Cost Center). Both required per row (cost center mandatory).
-- Complements the single home branch_id/cost_center_id on md_items (those stay the
-- primary/default dimension). One branch per item (unique item+branch). Additive,
-- idempotent. 0 DROP.

CREATE TABLE IF NOT EXISTS "md_item_branches" (
  "id"             BIGSERIAL PRIMARY KEY,
  "item_id"        BIGINT NOT NULL,
  "branch_id"      BIGINT NOT NULL,
  "cost_center_id" BIGINT NOT NULL,
  "created_at"     TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "updated_at"     TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "created_by_id"  BIGINT,
  "updated_by_id"  BIGINT,
  CONSTRAINT "md_item_branches_item_branch_key" UNIQUE ("item_id", "branch_id")
);

CREATE INDEX IF NOT EXISTS "md_item_branches_item_id_idx"        ON "md_item_branches"("item_id");
CREATE INDEX IF NOT EXISTS "md_item_branches_branch_id_idx"      ON "md_item_branches"("branch_id");
CREATE INDEX IF NOT EXISTS "md_item_branches_cost_center_id_idx" ON "md_item_branches"("cost_center_id");

-- AddForeignKey (intra-domain md — enforced). Cascade on item delete; restrict the
-- referenced cabang/cost center masters (they soft-delete via deleted_at, never hard-drop).
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'md_item_branches_item_id_fkey'
  ) THEN
    ALTER TABLE "md_item_branches"
      ADD CONSTRAINT "md_item_branches_item_id_fkey"
        FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE CASCADE ON UPDATE CASCADE;
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'md_item_branches_branch_id_fkey'
  ) THEN
    ALTER TABLE "md_item_branches"
      ADD CONSTRAINT "md_item_branches_branch_id_fkey"
        FOREIGN KEY ("branch_id") REFERENCES "md_branches"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'md_item_branches_cost_center_id_fkey'
  ) THEN
    ALTER TABLE "md_item_branches"
      ADD CONSTRAINT "md_item_branches_cost_center_id_fkey"
        FOREIGN KEY ("cost_center_id") REFERENCES "md_cost_centers"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
  END IF;
END $$;

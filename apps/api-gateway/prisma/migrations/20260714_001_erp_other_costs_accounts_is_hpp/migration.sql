-- Add default GL accounts and HPP allocation flag to Other Costs.
-- Additive only: nullable account columns keep existing data valid.

ALTER TABLE "md_other_costs"
  ADD COLUMN "debit_account_id" BIGINT,
  ADD COLUMN "credit_account_id" BIGINT,
  ADD COLUMN "is_hpp" BOOLEAN NOT NULL DEFAULT false;

CREATE INDEX "md_other_costs_debit_account_id_idx" ON "md_other_costs"("debit_account_id");
CREATE INDEX "md_other_costs_credit_account_id_idx" ON "md_other_costs"("credit_account_id");

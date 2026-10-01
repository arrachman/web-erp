-- Enforce Other Costs default GL accounts against md_accounts.
-- Same-domain md_* references are protected by DB foreign keys.

ALTER TABLE "md_other_costs"
  ADD CONSTRAINT "md_other_costs_debit_account_id_fkey"
  FOREIGN KEY ("debit_account_id") REFERENCES "md_accounts"("id")
  ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT "md_other_costs_credit_account_id_fkey"
  FOREIGN KEY ("credit_account_id") REFERENCES "md_accounts"("id")
  ON DELETE RESTRICT ON UPDATE CASCADE;

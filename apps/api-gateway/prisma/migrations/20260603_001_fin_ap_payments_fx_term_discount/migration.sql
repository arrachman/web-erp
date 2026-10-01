-- fin_ap_payments: add 4 optional columns for VP/VPP FX gain/loss + term discount.
-- Flagged in db-design/entities-m4-purchasing.md (payment reuse section) and
-- entities-m2-finance.md. All nullable — no existing rows affected.
-- (§2.32: hand-written SQL + prisma migrate deploy, not migrate dev)

ALTER TABLE fin_ap_payments
  ADD COLUMN IF NOT EXISTS fx_gain_loss_amount     NUMERIC(19,4),
  ADD COLUMN IF NOT EXISTS fx_gain_loss_account_id BIGINT,
  ADD COLUMN IF NOT EXISTS term_discount_amount    NUMERIC(19,4),
  ADD COLUMN IF NOT EXISTS term_discount_account_id BIGINT;

COMMENT ON COLUMN fin_ap_payments.fx_gain_loss_amount      IS 'Realized FX gain/loss on settlement (vp*selisihkurs)';
COMMENT ON COLUMN fin_ap_payments.fx_gain_loss_account_id  IS 'GL account for FX gain/loss (rekselisihkurs)';
COMMENT ON COLUMN fin_ap_payments.term_discount_amount     IS 'Early-payment term discount (vp*diskontermin)';
COMMENT ON COLUMN fin_ap_payments.term_discount_account_id IS 'GL account for term discount (rekdiskontermin)';

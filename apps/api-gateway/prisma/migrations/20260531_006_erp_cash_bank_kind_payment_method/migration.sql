-- Bank Masuk (RM) parity: discriminate cash vs bank on the shared
-- fin_cash_bank_transactions table + capture Cara Bayar (payment method).
-- Additive only: new enum + 2 columns + 1 index. No DROP, no data rewrite.
-- Existing CR/CD rows default to CASH (correct — they are Kas transactions).

-- 1. Cash-vs-bank discriminator enum (CASH=Kas CR/CD, BANK=Bank RM/SM).
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'ErpCashBankKind') THEN
    CREATE TYPE "ErpCashBankKind" AS ENUM ('CASH', 'BANK');
  END IF;
END
$$;

-- 2. Columns on the transaction header.
ALTER TABLE "fin_cash_bank_transactions"
  ADD COLUMN IF NOT EXISTS "kind" "ErpCashBankKind" NOT NULL DEFAULT 'CASH';

ALTER TABLE "fin_cash_bank_transactions"
  ADD COLUMN IF NOT EXISTS "payment_method" "ErpPaymentMethod";

-- 3. Index for kind/direction/status filtered lists (Kas vs Bank Masuk).
CREATE INDEX IF NOT EXISTS "fin_cash_bank_transactions_kind_direction_status_idx"
  ON "fin_cash_bank_transactions" ("kind", "direction", "status");

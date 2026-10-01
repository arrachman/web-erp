-- Add REVALUATION to ErpJournalType (additive, non-destructive).
-- Backs FIN.RV (Revaluasi Valas) journal-family transactions. Ordered before
-- CLOSING to mirror the Prisma enum declaration. Idempotent guard so the migration
-- is safe to re-run on environments where the value already exists.
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_enum
    WHERE enumlabel = 'REVALUATION'
      AND enumtypid = 'public."ErpJournalType"'::regtype
  ) THEN
    ALTER TYPE "ErpJournalType" ADD VALUE 'REVALUATION' BEFORE 'CLOSING';
  END IF;
END
$$;

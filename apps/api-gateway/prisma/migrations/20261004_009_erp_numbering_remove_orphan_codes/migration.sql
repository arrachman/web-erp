-- Document numbering: remove the three orphaned seed rows `INV`, `RCP`,
-- `PAY` (owner approval 2026-10-04). Verified before removal: no service
-- in either API repo looks these codes up (sales invoices use `SI`, AR
-- receipts use `IP`, AP payments use `VP`), the frontend never requests
-- them, and no existing document number starts with INV/RCP (the one AR
-- receipt was imported with the literal number PAY-09/0002, later
-- renumbered to IP000001, and never referenced this config row).
-- Soft-delete, mirroring the API's own delete behavior, so the rows stay
-- recoverable.

UPDATE "sys_document_numberings"
SET "deleted_at" = NOW(),
    "updated_at" = NOW()
WHERE "document_code" IN ('INV', 'RCP', 'PAY')
  AND "deleted_at" IS NULL;

-- Document numbering: split Proforma Invoice out of the shared `PI`
-- sequence (owner request 2026-10-04, "PI dipisah"). Purchase Invoice
-- keeps DOC_CODE `PI`; the proforma-invoices service now uses the new
-- code `PFI` (code change in erp-sls-proforma-invoices, both repos).
-- The `PI` row is renamed to match its single remaining user.

INSERT INTO "sys_document_numberings"
  ("document_code", "name", "prefix", "digit_count", "reset_policy", "next_number", "updated_at")
SELECT 'PFI', 'Proforma Invoice (PFI)', 'PFI', 6, 'YEARLY', 1, NOW()
WHERE NOT EXISTS (
  SELECT 1 FROM "sys_document_numberings" WHERE "document_code" = 'PFI'
);

UPDATE "sys_document_numberings"
SET "name" = 'Purchase Invoice (PI)',
    "updated_at" = NOW()
WHERE "document_code" = 'PI'
  AND "name" <> 'Purchase Invoice (PI)';

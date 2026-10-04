-- Document numbering: register every transaction DOC_CODE the API
-- services look up, so ALL transaction numbers are generated from
-- `sys_document_numberings` (Admin > Initial Setup > Document Numbering)
-- instead of each service's `count + 1` fallback. Requested by the owner
-- on 2026-10-04 ("terapkan ke semua nomor transaksi, terapkan juga di
-- backend api gateway"). The live gateway reads this table at runtime,
-- so the rows take effect without a restart.
--
-- Conventions follow the existing seed (`seedDocumentNumberings`):
-- digit_count 6, reset_policy YEARLY, next_number 1. The prefix preserves
-- each service's current FALLBACK_PREFIX so generated formats do not
-- change (e.g. ADJUSTMENT_JOURNAL -> AJ, RECEIPT_GIRO -> RG).
-- `PI` stays one shared sequence for Purchase Invoice and Proforma
-- Invoice (both services use DOC_CODE 'PI'); INV/RCP/PAY rows from the
-- original seed have no service user and are left untouched.

INSERT INTO "sys_document_numberings"
  ("document_code", "name", "prefix", "digit_count", "reset_policy", "next_number", "updated_at")
SELECT v.document_code, v.name, v.prefix, 6, 'YEARLY', 1, NOW()
FROM (VALUES
  ('ADJUSTMENT_JOURNAL', 'Adjustment Journal', 'AJ'),
  ('AS', 'Customer Advance (AS)', 'AS'),
  ('BS', 'Bid Selection (BS)', 'BS'),
  ('CLOSING_JOURNAL', 'Closing Journal', 'CJ'),
  ('DC', 'Daily Check (DC)', 'DC'),
  ('DNR', 'Debit Note Return (DNR)', 'DNR'),
  ('DNRI', 'Debit Note Return Issue (DNRI)', 'DNRI'),
  ('DO', 'Delivery Order (DO)', 'DO'),
  ('DOI', 'Delivery Order Issue (DOI)', 'DOI'),
  ('DR', 'Delivery Report (DR)', 'DR'),
  ('FX_REVALUATION', 'FX Revaluation Journal', 'RV'),
  ('GRI', 'Goods Receipt Issue (GRI)', 'GRI'),
  ('IB', 'Opening Stock (IB)', 'IB'),
  ('IP', 'AR Receipt (IP)', 'IP'),
  ('MEMORIAL_JOURNAL', 'Memorial Journal', 'JM'),
  ('MR', 'Material Request (MR)', 'MR'),
  ('OPENING_BALANCE', 'Opening Balance Journal', 'CB'),
  ('PA', 'Price Adjustment (PA)', 'PA'),
  ('PI', 'Purchase Invoice / Proforma Invoice (PI)', 'PI'),
  ('PII', 'Purchase Invoice Issue (PII)', 'PII'),
  ('PL', 'Packing List (PL)', 'PL'),
  ('PR', 'Purchase Requisition (PR)', 'PR'),
  ('PRT', 'Return to Vendor (PRT)', 'PRT'),
  ('RECEIPT_GIRO', 'Receipt Giro', 'RG'),
  ('RECEIPT_GIRO_CLEARING', 'Receipt Giro Clearing', 'RGC'),
  ('RF', 'Stock Issue (RF)', 'RF'),
  ('RFQ', 'Request for Quotation (RFQ)', 'RFQ'),
  ('RNR', 'Sales Return Receipt (RNR)', 'RNR'),
  ('RNRI', 'Sales Return Receipt Issue (RNRI)', 'RNRI'),
  ('RP', 'Freight Receivable (RP)', 'RP'),
  ('RS', 'Transfer Receipt (RS)', 'RS'),
  ('RW', 'Weighbridge Ticket (RW)', 'RW'),
  ('SA', 'Stock Adjustment (SA)', 'SA'),
  ('SEND_GIRO', 'Send Giro', 'SG'),
  ('SEND_GIRO_CLEARING', 'Send Giro Clearing', 'SGC'),
  ('SI', 'Sales Invoice (SI)', 'SI'),
  ('SIE', 'Sales Invoice Swap (SIE)', 'SIE'),
  ('SII', 'Sales Invoice Issue (SII)', 'SII'),
  ('SP', 'Stock Count (SP)', 'SP'),
  ('SQ', 'Sales Quotation (SQ)', 'SQ'),
  ('SR', 'Sales Return (SR)', 'SR'),
  ('TS', 'Stock Transfer (TS)', 'TS'),
  ('VP', 'AP Payment (VP)', 'VP')
) AS v(document_code, name, prefix)
WHERE NOT EXISTS (
  SELECT 1 FROM "sys_document_numberings" n
  WHERE n."document_code" = v.document_code
);

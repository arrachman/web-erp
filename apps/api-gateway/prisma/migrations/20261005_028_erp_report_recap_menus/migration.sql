-- Item sidebar untuk laporan rekap G4 dan purchase-suggestion.
-- GROUP tetap path NULL. Grant disalin dari item hub modul yang sama dengan
-- can_view/can_print/can_export ditulis eksplisit (default kolom FALSE).

-- M3
INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M3.RPT.FUEL_REFILLS_BY_STATUS', 'Rekap RF per Status', '/warehouse/reports/fuel-refills-by-status', 'ITEM', p."id", 100, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M3.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M3.RPT.FUEL_REFILLS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M3.RPT.DAILY_CHECKS_BY_STATUS', 'Rekap Daily Check per Status', '/warehouse/reports/daily-checks-by-status', 'ITEM', p."id", 101, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M3.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M3.RPT.DAILY_CHECKS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M3.RPT.PURCHASE_SUGGESTION', 'Saran Pembelian', '/warehouse/reports/purchase-suggestion', 'ITEM', p."id", 102, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M3.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M3.RPT.PURCHASE_SUGGESTION');

-- M4
INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.RPT.PURCHASE_REQUISITIONS_BY_STATUS', 'Rekap PR per Status', '/purchasing/reports/purchase-requisitions-by-status', 'ITEM', p."id", 100, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M4.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.RPT.PURCHASE_REQUISITIONS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.RPT.RFQS_BY_STATUS', 'Rekap RFQ per Status', '/purchasing/reports/rfqs-by-status', 'ITEM', p."id", 101, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M4.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.RPT.RFQS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.RPT.BID_COMPARISONS_BY_STATUS', 'Rekap BS per Status', '/purchasing/reports/bid-comparisons-by-status', 'ITEM', p."id", 102, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M4.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.RPT.BID_COMPARISONS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.RPT.PURCHASE_ORDERS_BY_STATUS', 'Rekap PO per Status', '/purchasing/reports/purchase-orders-by-status', 'ITEM', p."id", 103, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M4.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.RPT.PURCHASE_ORDERS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.RPT.GOODS_RECEIPTS_BY_STATUS', 'Rekap GRN per Status', '/purchasing/reports/goods-receipts-by-status', 'ITEM', p."id", 104, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M4.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.RPT.GOODS_RECEIPTS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.RPT.PURCHASE_INVOICES_BY_STATUS', 'Rekap PI per Status', '/purchasing/reports/purchase-invoices-by-status', 'ITEM', p."id", 105, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M4.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.RPT.PURCHASE_INVOICES_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.RPT.RETURN_SHIPMENTS_BY_STATUS', 'Rekap DNR per Status', '/purchasing/reports/return-shipments-by-status', 'ITEM', p."id", 106, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M4.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.RPT.RETURN_SHIPMENTS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.RPT.PURCHASE_RETURNS_BY_STATUS', 'Rekap PRT per Status', '/purchasing/reports/purchase-returns-by-status', 'ITEM', p."id", 107, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M4.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.RPT.PURCHASE_RETURNS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.RPT.VENDOR_PAYMENTS_BY_STATUS', 'Rekap Pembayaran Vendor per Status', '/purchasing/reports/vendor-payments-by-status', 'ITEM', p."id", 108, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M4.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.RPT.VENDOR_PAYMENTS_BY_STATUS');

-- M5
INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.QUOTATIONS_BY_STATUS', 'Rekap SQ per Status', '/sales/reports/quotations-by-status', 'ITEM', p."id", 100, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.QUOTATIONS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.ORDERS_BY_STATUS', 'Rekap SO per Status', '/sales/reports/orders-by-status', 'ITEM', p."id", 101, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.ORDERS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.PROFORMA_INVOICES_BY_STATUS', 'Rekap Proforma Invoice per Status', '/sales/reports/proforma-invoices-by-status', 'ITEM', p."id", 102, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.PROFORMA_INVOICES_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.PACKING_LISTS_BY_STATUS', 'Rekap Packing List per Status', '/sales/reports/packing-lists-by-status', 'ITEM', p."id", 103, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.PACKING_LISTS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.DELIVERY_ORDERS_BY_STATUS', 'Rekap Surat Jalan per Status', '/sales/reports/delivery-orders-by-status', 'ITEM', p."id", 104, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.DELIVERY_ORDERS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.DELIVERY_REPORTS_BY_STATUS', 'Rekap Laporan Pengiriman per Status', '/sales/reports/delivery-reports-by-status', 'ITEM', p."id", 105, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.DELIVERY_REPORTS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.INVOICES_BY_STATUS', 'Rekap Faktur Penjualan per Status', '/sales/reports/invoices-by-status', 'ITEM', p."id", 106, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.INVOICES_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.RETURNS_BY_STATUS', 'Rekap Retur Penjualan per Status', '/sales/reports/returns-by-status', 'ITEM', p."id", 107, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.RETURNS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.RETURN_RECEIPTS_BY_STATUS', 'Rekap Tanda Terima Retur per Status', '/sales/reports/return-receipts-by-status', 'ITEM', p."id", 108, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.RETURN_RECEIPTS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.CUSTOMER_ADVANCES_BY_STATUS', 'Rekap Uang Muka Pelanggan per Status', '/sales/reports/customer-advances-by-status', 'ITEM', p."id", 109, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.CUSTOMER_ADVANCES_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.FREIGHT_RECEIVABLES_BY_STATUS', 'Rekap Piutang Angkutan per Status', '/sales/reports/freight-receivables-by-status', 'ITEM', p."id", 110, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.FREIGHT_RECEIVABLES_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.OPENING_AR_BALANCE_BY_STATUS', 'Rekap Saldo Awal Piutang per Status', '/sales/reports/opening-ar-balance-by-status', 'ITEM', p."id", 111, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.OPENING_AR_BALANCE_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.AR_RECEIPTS_BY_STATUS', 'Rekap Penerimaan AR per Status', '/sales/reports/ar-receipts-by-status', 'ITEM', p."id", 112, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.AR_RECEIPTS_BY_STATUS');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.INVOICE_SWAPS_BY_STATUS', 'Rekap Tukar Faktur per Status', '/sales/reports/invoice-swaps-by-status', 'ITEM', p."id", 113, true, now(), now()
FROM "sys_menus" p WHERE p."code" = 'M5.RPT'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.INVOICE_SWAPS_BY_STATUS');

INSERT INTO "adm_role_menus" ("menu_id", "role_id", "can_view", "can_print", "can_export")
SELECT m."id", g."role_id", true, true, true
FROM "sys_menus" m
JOIN "sys_menus" hub ON hub."code" = 'M3.RPT.HUB'
JOIN "adm_role_menus" g ON g."menu_id" = hub."id" AND g."can_view" = true
WHERE m."code" IN ('M3.RPT.FUEL_REFILLS_BY_STATUS', 'M3.RPT.DAILY_CHECKS_BY_STATUS', 'M3.RPT.PURCHASE_SUGGESTION')
ON CONFLICT DO NOTHING;

INSERT INTO "adm_role_menus" ("menu_id", "role_id", "can_view", "can_print", "can_export")
SELECT m."id", g."role_id", true, true, true
FROM "sys_menus" m
JOIN "sys_menus" hub ON hub."code" = 'M4.RPT.HUB'
JOIN "adm_role_menus" g ON g."menu_id" = hub."id" AND g."can_view" = true
WHERE m."code" IN ('M4.RPT.PURCHASE_REQUISITIONS_BY_STATUS', 'M4.RPT.RFQS_BY_STATUS', 'M4.RPT.BID_COMPARISONS_BY_STATUS', 'M4.RPT.PURCHASE_ORDERS_BY_STATUS', 'M4.RPT.GOODS_RECEIPTS_BY_STATUS', 'M4.RPT.PURCHASE_INVOICES_BY_STATUS', 'M4.RPT.RETURN_SHIPMENTS_BY_STATUS', 'M4.RPT.PURCHASE_RETURNS_BY_STATUS', 'M4.RPT.VENDOR_PAYMENTS_BY_STATUS')
ON CONFLICT DO NOTHING;

INSERT INTO "adm_role_menus" ("menu_id", "role_id", "can_view", "can_print", "can_export")
SELECT m."id", g."role_id", true, true, true
FROM "sys_menus" m
JOIN "sys_menus" hub ON hub."code" = 'M5.RPT.HUB'
JOIN "adm_role_menus" g ON g."menu_id" = hub."id" AND g."can_view" = true
WHERE m."code" IN ('M5.RPT.QUOTATIONS_BY_STATUS', 'M5.RPT.ORDERS_BY_STATUS', 'M5.RPT.PROFORMA_INVOICES_BY_STATUS', 'M5.RPT.PACKING_LISTS_BY_STATUS', 'M5.RPT.DELIVERY_ORDERS_BY_STATUS', 'M5.RPT.DELIVERY_REPORTS_BY_STATUS', 'M5.RPT.INVOICES_BY_STATUS', 'M5.RPT.RETURNS_BY_STATUS', 'M5.RPT.RETURN_RECEIPTS_BY_STATUS', 'M5.RPT.CUSTOMER_ADVANCES_BY_STATUS', 'M5.RPT.FREIGHT_RECEIVABLES_BY_STATUS', 'M5.RPT.OPENING_AR_BALANCE_BY_STATUS', 'M5.RPT.AR_RECEIPTS_BY_STATUS', 'M5.RPT.INVOICE_SWAPS_BY_STATUS')
ON CONFLICT DO NOTHING;

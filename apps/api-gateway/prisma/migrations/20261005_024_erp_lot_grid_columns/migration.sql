-- Fase 2 T1 (lanjutan): kolom lot pada grid transaksi.
-- GRN (grid 121 / PUR.GRN): metadata lot per baris — ditulis grid ke
-- customFields baris, diteruskan form-model ke payload API (lotNumber dkk).
-- DO (grid 21 / SLS.DO): lotId pilihan manual; kosong = FEFO otomatis.
INSERT INTO sys_transaction_grid_columns
  (grid_id, sort_order, header_text, data_field, width, is_visible, is_required, is_editable, kind, data_type)
SELECT 121, 12, 'Lot/Batch', 'lotNumber', 150, true, false, true, 'STANDARD', 'TEXT'
WHERE NOT EXISTS (SELECT 1 FROM sys_transaction_grid_columns WHERE grid_id = 121 AND data_field = 'lotNumber');
INSERT INTO sys_transaction_grid_columns
  (grid_id, sort_order, header_text, data_field, width, is_visible, is_required, is_editable, kind, data_type)
SELECT 121, 13, 'Lot Supplier', 'supplierLotNo', 140, true, false, true, 'STANDARD', 'TEXT'
WHERE NOT EXISTS (SELECT 1 FROM sys_transaction_grid_columns WHERE grid_id = 121 AND data_field = 'supplierLotNo');
INSERT INTO sys_transaction_grid_columns
  (grid_id, sort_order, header_text, data_field, width, is_visible, is_required, is_editable, kind, data_type)
SELECT 121, 14, 'Tgl Produksi', 'manufactureDate', 120, true, false, true, 'STANDARD', 'DATE'
WHERE NOT EXISTS (SELECT 1 FROM sys_transaction_grid_columns WHERE grid_id = 121 AND data_field = 'manufactureDate');
INSERT INTO sys_transaction_grid_columns
  (grid_id, sort_order, header_text, data_field, width, is_visible, is_required, is_editable, kind, data_type)
SELECT 121, 15, 'Kadaluarsa', 'expiryDate', 120, true, false, true, 'STANDARD', 'DATE'
WHERE NOT EXISTS (SELECT 1 FROM sys_transaction_grid_columns WHERE grid_id = 121 AND data_field = 'expiryDate');
INSERT INTO sys_transaction_grid_columns
  (grid_id, sort_order, header_text, data_field, width, is_visible, is_required, is_editable, kind, data_type)
SELECT 21, 15, 'Lot ID', 'lotId', 110, true, false, true, 'STANDARD', 'TEXT'
WHERE NOT EXISTS (SELECT 1 FROM sys_transaction_grid_columns WHERE grid_id = 21 AND data_field = 'lotId');

-- Wave G1 (report .mrt): grup menu Laporan Master Data + hub "Semua Laporan"
-- mengikuti pola FIN.RPT (migrasi 20261005_016/028), plus seed format laporan
-- terpusat di sys_settings group 'company' (design D7, pengganti m0_setting).
-- Idempotent: aman dijalankan ulang.

-- 1) Grup M1.RPT di bawah modul M1 (id 12)
INSERT INTO sys_menus (code, title, path, type, parent_id, sort_order, is_active, created_at, updated_at)
SELECT 'M1.RPT', 'Reports', NULL, 'GROUP', 12, 7, true, now(), now()
WHERE NOT EXISTS (SELECT 1 FROM sys_menus WHERE code = 'M1.RPT' AND deleted_at IS NULL);

-- 2) Item hub "Semua Laporan" -> /master/reports (satu menu, combo box jenis laporan)
INSERT INTO sys_menus (code, title, path, type, parent_id, sort_order, is_active, created_at, updated_at)
SELECT 'M1.RPT.HUB', 'Semua Laporan', '/master/reports', 'ITEM', g.id, 0, true, now(), now()
FROM sys_menus g
WHERE g.code = 'M1.RPT'
  AND NOT EXISTS (SELECT 1 FROM sys_menus WHERE code = 'M1.RPT.HUB' AND deleted_at IS NULL);

-- 3) Grants: clone dari grup sibling M1.FIN (pola migrasi 028)
INSERT INTO adm_role_menus (menu_id, role_id, can_view, can_print, can_export)
SELECT m.id, g.role_id, true, true, true
FROM sys_menus m
JOIN adm_role_menus g
  ON g.menu_id = (SELECT id FROM sys_menus WHERE code = 'M1.FIN')
 AND g.can_view = true
WHERE m.code IN ('M1.RPT', 'M1.RPT.HUB')
ON CONFLICT DO NOTHING;

-- 4) Format settings terpusat (report_format_*)
INSERT INTO sys_settings (module, "group", key, name, value, data_type, sort_order, is_active, created_at, updated_at)
SELECT 'system', 'company', v.key, v.name, v.value, 'string', 90, true, now(), now()
FROM (VALUES
  ('report_format_nominal', 'Format Nominal (pola .NET)', '#,##0'),
  ('report_format_qty', 'Format Kuantitas (pola .NET)', '#,##0.##'),
  ('report_format_nominal_decimals', 'Desimal Nominal', '0'),
  ('report_format_qty_decimals', 'Desimal Kuantitas', '2'),
  ('report_format_negative', 'Gaya Negatif (minus/parentheses)', 'parentheses'),
  ('report_format_group_separator', 'Pemisah Ribuan', '.'),
  ('report_format_decimal_separator', 'Pemisah Desimal', ','),
  ('report_format_date', 'Format Tanggal (pola .NET)', 'dd/MM/yyyy')
) AS v(key, name, value)
WHERE NOT EXISTS (SELECT 1 FROM sys_settings s WHERE s."group" = 'company' AND s.key = v.key AND s.deleted_at IS NULL);

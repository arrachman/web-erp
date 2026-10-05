-- Fase 3 W5 (keputusan user 2026-10-05): pembayaran transfer manual +
-- konfirmasi admin. Menu halaman antrean konfirmasi di grup FIN.TX,
-- grants disalin dari BANK-RECEIPT.
INSERT INTO sys_menus (code, title, path, icon, type, parent_id, sort_order, is_active, created_at, updated_at)
SELECT 'FIN.TX.PAYCONF', 'Konfirmasi Pembayaran', '/finance/payment-confirmations', 'calculator', 'ITEM', 362,
       (SELECT COALESCE(MAX(sort_order), 0) + 1 FROM sys_menus WHERE parent_id = 362), true, now(), now()
WHERE NOT EXISTS (SELECT 1 FROM sys_menus WHERE code = 'FIN.TX.PAYCONF');

INSERT INTO adm_role_menus (role_id, menu_id, can_view, can_create, can_edit, can_delete, can_approve, can_print, can_export, can_import, is_favorite)
SELECT g.role_id, (SELECT id FROM sys_menus WHERE code = 'FIN.TX.PAYCONF'), g.can_view, g.can_create, g.can_edit, g.can_delete, g.can_approve, g.can_print, g.can_export, g.can_import, g.is_favorite
FROM adm_role_menus g
WHERE g.menu_id = (SELECT id FROM sys_menus WHERE code = 'BANK-RECEIPT')
  AND NOT EXISTS (
    SELECT 1 FROM adm_role_menus x
    WHERE x.role_id = g.role_id AND x.menu_id = (SELECT id FROM sys_menus WHERE code = 'FIN.TX.PAYCONF')
  );

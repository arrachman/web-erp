-- Remove the dead Cash/Bank Transfer menu (CASHBANK-TRANSFER).
-- The page was a non-functional skeleton: no backend endpoint exists for
-- /fin/cashbank-transfers (the cash/bank engine only serves CR/CD/RM/SM).
-- Frontend page/form/client are removed in the same change.
DELETE FROM adm_role_menus WHERE menu_id = (SELECT id FROM sys_menus WHERE code = 'CASHBANK-TRANSFER');
DELETE FROM sys_menus WHERE code = 'CASHBANK-TRANSFER';

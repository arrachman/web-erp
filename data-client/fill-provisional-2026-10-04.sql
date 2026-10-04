BEGIN;
-- Pengisian PROVISIONAL 2026-10-04 (aturan CLAUDE.md §6 baru): semua baris baru
-- bertanda legacy_code='provisional' / metadata origin provisional.
-- 1) Supplier/penerbit provisional (tipe SUP=2; provider tipe SUP-SERVICE=31)
INSERT INTO md_partners(code,name,partner_type_id,legacy_code,updated_at)
SELECT 'PROV-SUP-01','[PROVISIONAL] Penerbit Buku Cendekia',2,'provisional',now()
WHERE NOT EXISTS (SELECT 1 FROM md_partners WHERE code='PROV-SUP-01');
INSERT INTO md_partners(code,name,partner_type_id,legacy_code,updated_at)
SELECT 'PROV-SUP-02','[PROVISIONAL] Penerbit Pelita Ilmu',2,'provisional',now()
WHERE NOT EXISTS (SELECT 1 FROM md_partners WHERE code='PROV-SUP-02');
INSERT INTO md_partners(code,name,partner_type_id,legacy_code,updated_at)
SELECT 'PROV-SUP-03','[PROVISIONAL] Distributor ATK Sentosa',2,'provisional',now()
WHERE NOT EXISTS (SELECT 1 FROM md_partners WHERE code='PROV-SUP-03');
INSERT INTO md_partners(code,name,partner_type_id,legacy_code,updated_at)
SELECT 'PROV-SUP-04','[PROVISIONAL] Supplier Konsumsi Barokah',2,'provisional',now()
WHERE NOT EXISTS (SELECT 1 FROM md_partners WHERE code='PROV-SUP-04');
INSERT INTO md_partners(code,name,partner_type_id,legacy_code,updated_at)
SELECT 'PROV-SUP-05','[PROVISIONAL] Furnitur Sekolah Jaya',2,'provisional',now()
WHERE NOT EXISTS (SELECT 1 FROM md_partners WHERE code='PROV-SUP-05');
INSERT INTO md_partners(code,name,partner_type_id,legacy_code,updated_at)
SELECT 'PROV-SUP-06','[PROVISIONAL] Provider Internet Nusantara',31,'provisional',now()
WHERE NOT EXISTS (SELECT 1 FROM md_partners WHERE code='PROV-SUP-06');

-- 2) Link vendor per kategori (hanya yang masih kosong)
UPDATE md_items SET vendor_id=(SELECT id FROM md_partners WHERE code='PROV-SUP-01')
WHERE vendor_id IS NULL AND category_id IN (133,135,138);
UPDATE md_items SET vendor_id=(SELECT id FROM md_partners WHERE code='PROV-SUP-03')
WHERE vendor_id IS NULL AND category_id IN (105,136,137,139);
UPDATE md_items SET vendor_id=(SELECT id FROM md_partners WHERE code='PROV-SUP-04')
WHERE vendor_id IS NULL AND category_id=109;
UPDATE md_items SET vendor_id=(SELECT id FROM md_partners WHERE code='PROV-SUP-05')
WHERE vendor_id IS NULL AND category_id=134;
UPDATE md_items SET vendor_id=(SELECT id FROM md_partners WHERE code='PROV-SUP-06')
WHERE id=834;

-- 3) Harga beli estimasi 75% harga jual (dibulatkan ke 250) bila masih 0/kosong
UPDATE md_items SET purchase_price=round(sale_price*0.75/250)*250
WHERE (purchase_price IS NULL OR purchase_price=0) AND sale_price>0;

-- 4) Stok min/max provisional per kategori (hanya yang masih 0/kosong)
UPDATE md_items SET min_stock=20, max_stock=100 WHERE coalesce(min_stock,0)=0 AND category_id IN (133,135);
UPDATE md_items SET min_stock=10, max_stock=50 WHERE coalesce(min_stock,0)=0 AND category_id IN (105,109,137);
UPDATE md_items SET min_stock=2, max_stock=10 WHERE coalesce(min_stock,0)=0 AND category_id IN (134,136);
UPDATE md_items SET min_stock=5, max_stock=25 WHERE coalesce(min_stock,0)=0 AND category_id=138;
UPDATE md_items SET min_stock=1, max_stock=5 WHERE coalesce(min_stock,0)=0 AND category_id=139;

-- 5) Akun penjualan kategori yang masih kosong (LKS/PERAGA/SERAGAM/CETAK/PAKET)
UPDATE md_item_categories SET sales_account_id=523 WHERE id=135 AND sales_account_id IS NULL;
UPDATE md_item_categories SET sales_account_id=525 WHERE id IN (136,137) AND sales_account_id IS NULL;
UPDATE md_item_categories SET sales_account_id=259 WHERE id IN (138,139) AND sales_account_id IS NULL;

-- 6) Rabat provisional 2026
INSERT INTO pur_supplier_rebates(supplier_id,category_id,percent,period_year,notes,is_active,metadata,created_at,updated_at)
SELECT id,133,5.00,2026,'PROVISIONAL - menunggu data klien',true,'{"origin":"provisional"}',now(),now()
FROM md_partners WHERE code='PROV-SUP-01'
AND NOT EXISTS (SELECT 1 FROM pur_supplier_rebates r JOIN md_partners p ON p.id=r.supplier_id WHERE p.code='PROV-SUP-01' AND r.period_year=2026);
INSERT INTO pur_supplier_rebates(supplier_id,category_id,percent,period_year,notes,is_active,metadata,created_at,updated_at)
SELECT id,NULL,5.00,2026,'PROVISIONAL - menunggu data klien',true,'{"origin":"provisional"}',now(),now()
FROM md_partners WHERE code='PROV-SUP-02'
AND NOT EXISTS (SELECT 1 FROM pur_supplier_rebates r JOIN md_partners p ON p.id=r.supplier_id WHERE p.code='PROV-SUP-02' AND r.period_year=2026);
INSERT INTO pur_supplier_rebates(supplier_id,category_id,percent,period_year,notes,is_active,metadata,created_at,updated_at)
SELECT id,NULL,3.00,2026,'PROVISIONAL - menunggu data klien',true,'{"origin":"provisional"}',now(),now()
FROM md_partners WHERE code='PROV-SUP-03'
AND NOT EXISTS (SELECT 1 FROM pur_supplier_rebates r JOIN md_partners p ON p.id=r.supplier_id WHERE p.code='PROV-SUP-03' AND r.period_year=2026);
COMMIT;

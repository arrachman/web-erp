-- Setup CV Bahtera Madani: cabang, lokasi, gudang, akun pendapatan, wiring data-client. Tag: legacy_code='data-client'
begin;
update sys_settings set value='CV Bahtera Madani', updated_at=now() where "group"='company' and key='name';
update sys_settings set value='CV Bahtera Madani', updated_at=now() where "group"='bank-accounts' and key='default_account_name';

insert into md_branches(code,name,legacy_code,updated_at) values ('BM','CV Bahtera Madani','data-client',now());
insert into md_locations(code,name,branch_id,legacy_code,updated_at)
  select 'BM-GDG-01','Gudang Bahtera Madani',id,'data-client',now() from md_branches where code='BM';
insert into md_warehouses(code,name,location_id,legacy_code,updated_at)
  select 'BM-GDG-01-UTM','Gudang Utama Bahtera Madani',id,'data-client',now() from md_locations where code='BM-GDG-01';

insert into md_accounts(code,name,type,kind,normal_balance,parent_id,level,legacy_code,updated_at)
select v.code,v.name,'REVENUE','POSTABLE','CREDIT',(select id from md_accounts where code='4101.00.000'),4,'data-client',now()
from (values ('4104.01.001','Penjualan Buku & Bahan Ajar'),('4105.01.001','Penjualan Furnitur Sekolah'),('4106.01.001','Penjualan ATK & Perlengkapan Sekolah')) v(code,name);

update md_item_categories set sales_account_id=(select id from md_accounts where code='4104.01.001'),
  sales_return_account_id=(select id from md_accounts where code='4110.01.001'), sales_discount_account_id=(select id from md_accounts where code='4111.01.001')
  where legacy_code='data-client' and code='BUKU';
update md_item_categories set sales_account_id=(select id from md_accounts where code='4105.01.001'),
  sales_return_account_id=(select id from md_accounts where code='4110.01.001'), sales_discount_account_id=(select id from md_accounts where code='4111.01.001')
  where legacy_code='data-client' and code='FURN';

update md_items i set sales_account_id=(select id from md_accounts where code=
    case c.code when 'BUKU' then '4104.01.001' when 'FURN' then '4105.01.001' else '4106.01.001' end), updated_at=now()
  from md_item_categories c where c.id=i.category_id and i.legacy_code='data-client';

update md_partners set branch_id=(select id from md_branches where code='BM'), updated_at=now() where legacy_code='data-client';
update md_partners set receivable_account_id=(select id from md_accounts where code='1120.01.001') where legacy_code='data-client' and code like 'CUST-%';

update sls_invoices set branch_id=(select id from md_branches where code='BM'),
  location_id=(select id from md_locations where code='BM-GDG-01'),
  warehouse_id=(select id from md_warehouses where code='BM-GDG-01-UTM'),
  receivable_account_id=(select id from md_accounts where code='1120.01.001'), updated_at=now()
  where legacy_code='data-client';
update sls_invoice_lines set warehouse_id=(select id from md_warehouses where code='BM-GDG-01-UTM')
  where invoice_id in (select id from sls_invoices where legacy_code='data-client');
update fin_ar_receipts set branch_id=(select id from md_branches where code='BM'),
  location_id=(select id from md_locations where code='BM-GDG-01'), updated_at=now() where legacy_code='data-client';
commit;

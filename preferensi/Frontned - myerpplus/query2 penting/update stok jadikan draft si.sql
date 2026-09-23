UPDATE m1_item_stock_warehouse isw

JOIN m5_si_detail sid ON isw.idbarang = sid.idbarang 
SET isw.stok = (isw.stok + sid.jmlbarang)
WHERE isw.kgudang = "sgt" AND sid.idsi = "552289"
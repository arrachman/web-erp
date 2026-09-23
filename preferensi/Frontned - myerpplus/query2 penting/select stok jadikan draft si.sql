SELECT isw.idbarang,  isw.kgudang, isw.stok, sid.jmlbarang, (isw.stok + sid.jmlbarang) AS newstok FROM m1_item_stock_warehouse isw
JOIN m5_si_detail sid ON isw.idbarang = sid.idbarang 
WHERE kgudang = "SGT" AND sid.idsi = "321524"
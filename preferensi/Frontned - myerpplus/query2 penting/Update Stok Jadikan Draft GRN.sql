#SELECT isw.idbarang, isw.stok, isw.kgudang FROM m1_item_stock_warehouse isw
UPDATE m1_item_stock_warehouse isw
JOIN m4_grn_detail grnd ON isw.idbarang = grnd.idbarang 
SET isw.stok = (isw.stok - grnd.jmlbarang)
WHERE isw.kgudang = "DC" AND grnd.idgrn = "5975"
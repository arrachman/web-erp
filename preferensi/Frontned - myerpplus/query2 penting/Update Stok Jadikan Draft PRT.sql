UPDATE m1_item_stock_warehouse isw

JOIN m4_prt_detail prtd ON isw.idbarang = prtd.idbarang 
SET isw.stok = (isw.stok + prtd.jmlbarang)
WHERE isw.kgudang = "DC" AND prtd.idprt = "1205"
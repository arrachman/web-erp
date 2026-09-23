UPDATE
m1_item_transaction it
JOIN m4_grn_detail grnd ON it.sumber = 'grn'
AND it.idutama = grnd.idgrn AND it.iddetail = grnd.idgrndetail AND it.idbarang = grnd.idbarang
AND it.jenismutasi = 1
AND it.tgl BETWEEN '2016-04-01' AND '2016-04-31'
JOIN m4_grn grn ON grnd.idgrn = grn.grnid
AND ROUND(it.hpp,2) <> ROUND((CASE grn.grnhargatermasukpajak
WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) / grnd.jml
ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1) / grnd.jml
END),2)  
SET it.hpp = (CASE grn.grnhargatermasukpajak
WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) / grnd.jml
ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1) / grnd.jml
END)
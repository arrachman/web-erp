UPDATE
m1_item_transaction it
JOIN m4_ri_detail rid ON it.sumber = 'ri'
AND it.idutama = rid.idri AND it.iddetail = rid.idridetail AND it.idbarang = rid.idbarang
AND it.jenismutasi = 1
AND it.tgl BETWEEN '2016-04-01' AND '2016-04-31'
JOIN m4_ri ri ON rid.idri = ri.riid
AND ROUND(it.hpp,2) <> ROUND((CASE ri.rihargatermasukpajak
WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) / rid.jml
ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1) / rid.jml
END),2)  
SET it.hpp = (CASE ri.rihargatermasukpajak
WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) / rid.jml
ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1) / rid.jml
END)
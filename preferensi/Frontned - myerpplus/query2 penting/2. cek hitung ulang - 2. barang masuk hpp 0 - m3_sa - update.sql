UPDATE
m1_item_transaction it
JOIN m3_sa_detail sad ON it.sumber = 'SA'
AND it.idutama = sad.idsa AND it.iddetail = sad.idsadetail AND it.idbarang = sad.idbarang
AND it.jenismutasi = 1
AND it.tgl BETWEEN '2016-04-01' AND '2016-04-31'
AND it.hpp <> sad.hpp
SET it.hpp = sad.hpp
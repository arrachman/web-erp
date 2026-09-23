UPDATE
m1_item_transaction it
JOIN m5_si_detail sid ON it.sumber = 'si'
AND it.idutama = sid.idsi AND it.iddetail = sid.idsidetail AND it.idbarang = sid.idbarang
AND it.jenismutasi = 0
AND it.tgl BETWEEN '2016-04-01' AND '2016-04-31'
JOIN m5_si si ON sid.idsi = si.siid
AND ROUND(it.hpp,2) <> ROUND(sid.hpp,2)  
SET it.hpp = sid.hpp


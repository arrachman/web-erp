SELECT
si.siid,
si.sinotransaksi,
sid.idsidetail,
sid.idbarang,
sid.namabarang,
sid.jmlbarang,
sid.satuanbarang,
sid.hpp#,
#it.hpp
FROM
m5_si si
JOIN m5_si_detail sid ON si.siid = sid.idsi
#left JOIN m1_item_transaction it ON si.sisumber = it.sumber AND si.siid = it.idutama AND sid.idsidetail = it.iddetail AND sid.idbarang = it.idbarang
WHERE 
si.sitgl BETWEEN '2015-03-01' AND '2015-03-31' AND si.sistatus in(2,3,4,7)
#AND si.silokasi = 'PJN' 
#and it.id IS NULL

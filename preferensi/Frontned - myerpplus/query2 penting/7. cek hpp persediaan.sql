SELECT
*
FROM
(
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
) as sibaru

RIGHT JOIN

(
SELECT
it.id,
it.cabang,
it.lokasi,
it.gudang,
it.sumber,
it.idutama,
it.iddetail,
it.notransaksi,
it.tgl,
it.idbarang,
i.bkode,
it.namabarang,
it.jmlbarang,
it.satuanbarang,
it.hpp, i.bjenis
FROM `m1_item_transaction` it
JOIN m1_item i on idbarang = bid
#left JOIN m5_si_detail sid ON it.sumber = 'SI' AND it.idutama = sid.idsi AND it.iddetail = sid.idsidetail AND sid.idbarang = it.idbarang
WHERE it.jenismutasi = '0' AND it.tgl BETWEEN '2015-03-01' AND '2015-03-31' 
AND it.sumber <> 'TS' AND it.sumber <> 'RS'
AND it.sumber = 'SI' #AND it.lokasi = 'PJN' 
#AND sid.idsi IS NULL
ORDER BY it.idutama, it.iddetail
) as itbaru

ON sibaru.siid = itbaru.idutama AND sibaru.idsidetail = itbaru.iddetail AND sibaru.idbarang = itbaru.idbarang
WHERE sibaru.siid IS NULL
ORDER BY itbaru.notransaksi ASC
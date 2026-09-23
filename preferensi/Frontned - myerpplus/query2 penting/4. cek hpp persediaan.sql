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
left JOIN m5_si_detail sid ON it.sumber = 'SI' AND it.idutama = sid.idsi AND it.iddetail = sid.idsidetail AND sid.idbarang = it.idbarang
WHERE it.jenismutasi = '0' AND it.tgl BETWEEN '2015-03-01' AND '2015-03-31' 
AND it.sumber <> 'TS' AND it.sumber <> 'RS'
AND it.sumber = 'SI' #AND it.lokasi = 'PJN' 
AND sid.idsi IS NULL
ORDER BY it.idutama, it.iddetail

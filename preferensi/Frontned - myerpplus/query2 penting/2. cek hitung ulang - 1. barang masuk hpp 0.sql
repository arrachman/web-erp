SELECT #COUNT(it.id), 
it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber, it.idutama, it.iddetail, it.jmlbarang, it.hpp, it.saldojml, it.saldohpp, it.saldonilai, it.customint10, it.notransaksi, i.bkode 
FROM m1_item_transaction it
JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R'
JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1
WHERE it.tgl BETWEEN '2016-04-01' AND '2016-04-31'
AND it.hpp = 0 AND it.jenismutasi = 1
#GROUP BY it.sumber
ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail 
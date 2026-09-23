SELECT it.id, it.idbarang, it.jenismutasi, it.tgl, it.inputtgl, it.sumber,  it.idutama, it.iddetail, it.jmlbarang, it.notransaksi,it.hpp, it.customint10, 
it.saldojml, it.saldohpp, it.saldonilai
FROM m1_item_transaction it 
JOIN m1_item i ON it.idbarang = i.bid #AND i.bjenis <> 'J' AND i.bjenis <> 'V' AND i.bhpp = 'R' 
#AND it.tgl BETWEEN '2015-09-01' AND '2015-09-31'
AND it.tgl <= '2016-03-31'
#AND i.bid = 17001















JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 
ORDER BY it.tgl, it.inputtgl, it.customint10, it.jenismutasi, it.idutama, it.iddetail 
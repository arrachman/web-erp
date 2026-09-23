SELECT
it.sumber, it.notransaksi, it.jenismutasi, it.idutama, it.iddetail, COUNT(it.iddetail) as jmlrow
FROM
m1_item_transaction it
WHERE tgl >= '2016-04-01'
#JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 
GROUP BY it.sumber, it.jenismutasi, it.idutama, it.iddetail
HAVING jmlrow <> 1
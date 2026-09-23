SELECT
SUM(
(CASE it.jenismutasi
WHEN 1 THEN (it.jmlbarang * it.hpp)
ELSE (it.jmlbarang * it.hpp) * -1
END)
)
FROM
m1_item_transaction it
JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1 #AND it.jenismutasi = 1
AND it.tgl BETWEEN '2015-05-01' AND '2015-05-31'
AND it.gudang = 'DC'
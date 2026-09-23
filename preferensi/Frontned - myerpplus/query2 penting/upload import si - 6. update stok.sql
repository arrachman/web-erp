INSERT INTO m1_item_stock_warehouse(
SELECT
tb.idbarang,
tb.gudang,
SUM(
(CASE tb.jenismutasi
WHEN 1 THEN tb.jmlbarang
ELSE tb.jmlbarang * -1
END)
) as stokfix
FROM
m1_item_transaction tb
JOIN m0_nomor n 
ON tb.sumber = n.kodetabel 
AND n.transaksibarang = 1
#AND tb.tgl BETWEEN '2016-02-01' AND '2016-03-01'
JOIN m1_item i 
ON tb.idbarang = i.bid 
AND i.bjenis = 'P'
GROUP BY tb.idbarang, tb.gudang
#HAVING stokfix < 0
)
ON DUPLICATE KEY UPDATE stok = VALUES(stok);
SELECT
tb.idbarang,
tb.gudang,
i.bkode AS kodebarang,
i.bnama AS namabarang,
SUM(
(CASE tb.jenismutasi
WHEN 1 THEN tb.jmlbarang
ELSE tb.jmlbarang * -1
END)
) as stokfix,
(CASE i.bhppaverage
WHEN 0 THEN i.bhargabeli
ELSE i.bhppaverage
END) AS hpp,
i.bhargabeli AS hargabeli
#, IFNULL(h.saldohpp,0) as hpp
FROM
m1_item_transaction tb
JOIN m0_nomor n 
ON tb.sumber = n.kodetabel 
AND n.transaksibarang = 1
AND tb.tgl <= '2016-04-31'
JOIN m1_item i 
ON tb.idbarang = i.bid 
AND i.bjenis = 'P'
#LEFT JOIN m0_hppaverage h ON i.bid = h.idbarang
GROUP BY tb.gudang ASC, tb.idbarang ASC
HAVING stokfix < 0
ORDER BY tb.gudang ASC, tb.idbarang ASC

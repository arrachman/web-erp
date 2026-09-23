SELECT i.bid, (CASE IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0) WHEN 0 THEN 0 ELSE (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang * hpp ELSE jmlbarang * hpp * (-1) END)),0)) / (IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0)) END ) as kssaldohpp
FROM m1_item i
JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.tgl < '2015-11-01'
JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1
GROUP BY i.bid
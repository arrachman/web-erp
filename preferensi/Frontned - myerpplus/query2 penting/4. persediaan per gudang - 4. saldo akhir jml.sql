SELECT 
it.gudang as ksgudang, 
i.bid as ksidbarang, 
#i.bkode as kskodebarang, 
#i.bnama as ksnamabarang, 
IFNULL(SUM((CASE jenismutasi WHEN 1 THEN jmlbarang ELSE jmlbarang * (-1) END)),0) as kssaldojml
FROM m1_item i
JOIN m1_item_transaction it ON i.bid = it.idbarang AND it.tgl <= '2016-03-31'
#WHERE i.bid = '" & FixQuotes(idbarang) & "'
GROUP BY it.gudang ASC, i.bkode
ORDER BY it.gudang ASC, i.bkode

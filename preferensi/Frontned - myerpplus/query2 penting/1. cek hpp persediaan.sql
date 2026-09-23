SELECT SUM(jmlbarang*hpp) FROM `m1_item_transaction` 
JOIN m1_item on idbarang = bid
WHERE `jenismutasi` = '0' AND `tgl` BETWEEN '2015-03-01' AND '2015-03-31' 
AND sumber <> 'TS' AND sumber <> 'RS'
#AND sumber = 'SA' #AND lokasi = 'PJN'
#GROUP BY sumber, lokasi, gudang, SUBSTR(notransaksi FROM 1 FOR 6)
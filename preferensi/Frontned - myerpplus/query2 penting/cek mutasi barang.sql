SELECT i.bkode, tsd.namabarang , tsd.satuan, tsd.jmlbarang, 
ts.tsnotransaksi, ts.tstgl, ts.tsgudangasal, ts.tsgudangtujuan
FROM m3_ts ts 
JOIN m3_ts_detail tsd ON ts.tsid = tsd.idts
JOIN m1_item i ON tsd.idbarang = i.bid
WHERE ts.tstgl BETWEEN "2015-08-01" AND "2015-08-25" 
AND ts.tsgudangasal = "DC" AND ts.tsgudangtujuan = "GL1"
AND ts.tsstatus IN(2,3,4,7)
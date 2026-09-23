SELECT i.bkode, mrd.namabarang , mrd.satuan, mrd.jmlbarang, 
mr.mrnotransaksi, mr.mrtgl, mr.mrgudangasal, mr.mrgudangtujuan
FROM m3_mr mr 
JOIN m3_mr_detail mrd ON mr.mrid = mrd.idmr
JOIN m1_item i ON mrd.idbarang = i.bid
WHERE mr.mrtgl BETWEEN "2015-08-01" AND "2015-08-25" 
AND mr.mrgudangasal = "DC" AND mr.mrgudangtujuan = "GL1"
AND mr.mrstatus IN(2,3,4,7)
update `m5_si_detail` sid JOIN m1_item i
ON sid.idbarang = i.bid
JOIN m5_si si ON si.siid = sid.idsi 
SET sid.customdbl2 = i.bkp
WHERE si.sitgl >= "2016-04-01";
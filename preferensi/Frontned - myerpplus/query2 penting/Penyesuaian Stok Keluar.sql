SELECT
sa.said,
sa.sagudang,
sa.sajenis,
sa.sanotransaksi,
sa.satgl,
sad.idsadetail,
sad.idbarang,
i.bkode,
sad.namabarang,
sad.jmlbarangkeluar,
sad.satuanbarang,
sad.hpp,
sad.jmlbarangkeluar * sad.hpp as nilai
FROM
m3_sa sa
JOIN m3_sa_detail sad ON sa.said = sad.idsa 
AND sa.sastatus IN(2,3,4,7)
AND sad.jmlbarangkeluar <> 0
AND sa.satgl BETWEEN '2015-05-01' AND '2015-05-31'
JOIN m1_item i ON sad.idbarang = i.bid

SELECT
sa.sagudang,
sad.idsadetail,
sa.said,
sa.sanotransaksi,
sad.idbarang,
sad.namabarang,
sad.hpp,
sum(sad.hpp * sad.jmlbarangmasuk)
FROM
m3_sa sa
JOIN m3_sa_detail sad ON sa.said = sad.idsa
AND sa.sastatus IN(2,3,4,7)
AND sa.satgl BETWEEN '2016-04-01' AND '2016-04-31'
#JOIN m1_item_transaction it ON sa.sasumber = it.sumber
#AND sa.said = it.idutama
#AND sad.idsadetail = it.iddetail
#AND sad.hpp <> it.hpp
AND sad.jmlmasuk <> 0
#AND sa.salokasa <> 'PJN'
GROUP BY sa.sagudang
ORDER BY sad.idsadetail
SELECT
sa.sasumber as sumber,
sa.said as idutama,
sad.idsadetail as iddetail,
sad.idbarang as idbarang
FROM
m3_sa sa
JOIN m3_sa_detail sad ON sa.said = sad.idsa
AND sa.sastatus IN (2,3,4,7)
AND sa.satgl BETWEEN '2016-04-01' AND '2016-04-31'
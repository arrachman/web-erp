SELECT
ri.risumber as sumber,
ri.riid as idutama,
rid.idridetail as iddetail,
rid.idbarang as idbarang
FROM
m4_ri ri
JOIN m4_ri_detail rid ON ri.riid = rid.idri
AND ri.ristatus IN (2,3,4,7)
AND ri.ritgl BETWEEN '2016-04-01' AND '2016-04-31'
AND ri.rijenispembeliankategori = 1
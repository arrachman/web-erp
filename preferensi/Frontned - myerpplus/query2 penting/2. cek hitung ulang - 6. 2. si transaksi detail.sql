SELECT
si.sisumber as sumber,
si.siid as idutama,
sid.idsidetail as iddetail,
sid.idbarang as idbarang
FROM
m5_si si
JOIN m5_si_detail sid ON si.siid = sid.idsi
AND si.sistatus IN (2,3,4,7)
AND si.sitgl BETWEEN '2016-04-01' AND '2016-04-31'
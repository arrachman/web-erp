SELECT
prt.prtsumber as sumber,
prt.prtid as idutama,
prtd.idprtdetail as iddetail,
prtd.idbarang as idbarang
FROM
m4_prt prt
JOIN m4_prt_detail prtd ON prt.prtid = prtd.idprt
AND prt.prtstatus IN (2,3,4,7)
AND prt.prttgl BETWEEN '2016-04-01' AND '2016-04-31'
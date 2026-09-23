SELECT
grn.grnsumber as sumber,
grn.grnid as idutama,
grnd.idgrndetail as iddetail,
grnd.idbarang as idbarang
FROM
m4_grn grn
JOIN m4_grn_detail grnd ON grn.grnid = grnd.idgrn
AND grn.grnstatus IN (2,3,4,7)
AND grn.grntgl BETWEEN '2016-04-01' AND '2016-04-31'
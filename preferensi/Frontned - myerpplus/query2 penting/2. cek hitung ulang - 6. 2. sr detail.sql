SELECT
sr.srsumber as sumber,
sr.srid as idutama,
srd.idsrdetail as iddetail,
srd.idbarang as idbarang
FROM
m5_sr sr
JOIN m5_sr_detail srd ON sr.srid = srd.idsr
AND sr.srstatus IN (2,3,4,7)
AND sr.srtgl BETWEEN '2016-04-01' AND '2016-04-31'
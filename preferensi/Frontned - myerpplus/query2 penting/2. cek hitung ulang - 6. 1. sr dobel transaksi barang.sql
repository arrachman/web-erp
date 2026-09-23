SELECT
it.id,it.notransaksi,
COUNT(it.id) as jmlrow
FROM
(SELECT
m1_item_transaction.*
FROM m1_item_transaction
WHERE sumber = 'sr' AND tgl BETWEEN '2016-04-01' AND '2016-04-31') as it

LEFT JOIN 

(
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
#AND sr.srjenispembeliankategori = 1
) as gr

ON it.sumber = gr.sumber
AND it.idutama = gr.idutama
AND it.iddetail = gr.iddetail
AND it.idbarang = gr.idbarang
GROUP BY it.sumber ,it.idutama ,it.iddetail, it.idbarang 
#WHERE gr.idutama IS NULL
HAVING jmlrow <> 1
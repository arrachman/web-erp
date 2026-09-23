SELECT
it.id,it.notransaksi,
COUNT(it.id) as jmlrow
FROM
(SELECT
m1_item_transaction.*
FROM m1_item_transaction
WHERE sumber = 'ri' AND tgl BETWEEN '2016-04-01' AND '2016-04-31') as it

LEFT JOIN 

(
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
AND ri.rijenispembeliankategori = 1) as gr

ON it.sumber = gr.sumber
AND it.idutama = gr.idutama
AND it.iddetail = gr.iddetail
AND it.idbarang = gr.idbarang
GROUP BY it.sumber ,it.idutama ,it.iddetail, it.idbarang 
#WHERE gr.idutama IS NULL
HAVING jmlrow <> 1
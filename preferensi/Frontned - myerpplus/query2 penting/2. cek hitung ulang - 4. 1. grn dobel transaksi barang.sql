SELECT
it.id,it.notransaksi,
COUNT(it.id) as jmlrow
FROM
(SELECT
m1_item_transaction.*
FROM m1_item_transaction
WHERE sumber = 'grn' AND tgl BETWEEN '2016-04-01' AND '2016-04-31') as it

LEFT JOIN 

(
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
#AND grn.grnjenispembeliankategori = 1
) as gr

ON it.sumber = gr.sumber
AND it.idutama = gr.idutama
AND it.iddetail = gr.iddetail
AND it.idbarang = gr.idbarang
GROUP BY it.sumber ,it.idutama ,it.iddetail, it.idbarang 
#WHERE gr.idutama IS NULL
HAVING jmlrow <> 1
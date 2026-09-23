SELECT
it.id,it.notransaksi,
COUNT(it.id) as jmlrow
FROM
(SELECT
m1_item_transaction.*
FROM m1_item_transaction
WHERE sumber = 'sa' AND tgl BETWEEN '2016-04-01' AND '2016-04-31') as it

LEFT JOIN 

(
SELECT
sa.sasumber as sumber,
sa.said as idutama,
sad.idsadetail as iddetail,
sad.idbarang as idbarang
FROM
m3_sa sa
JOIN m3_sa_detail sad ON sa.said = sad.idsa
AND sa.sastatus IN (2,3,4,7)
AND sa.satgl BETWEEN '2016-03-01' AND '2016-03-31'
#AND sa.sajenispembeliankategori = 1
) as gr

ON it.sumber = gr.sumber
AND it.idutama = gr.idutama
AND it.iddetail = gr.iddetail
AND it.idbarang = gr.idbarang
GROUP BY it.sumber ,it.idutama ,it.iddetail, it.idbarang 
#WHERE gr.idutama IS NULL
HAVING jmlrow <> 1
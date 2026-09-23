SELECT
it.id,it.notransaksi,
COUNT(it.id) as jmlrow
FROM
(SELECT
m1_item_transaction.*
FROM m1_item_transaction
WHERE sumber = 'prt' AND tgl BETWEEN '2016-04-01' AND '2016-04-31') as it

LEFT JOIN 

(
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
#AND prt.prtjenispembeliankategori = 1
) as gr

ON it.sumber = gr.sumber
AND it.idutama = gr.idutama
AND it.iddetail = gr.iddetail
AND it.idbarang = gr.idbarang
GROUP BY it.sumber ,it.idutama ,it.iddetail, it.idbarang 
#WHERE gr.idutama IS NULL
HAVING jmlrow <> 1
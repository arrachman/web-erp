SELECT
cfo.cfosumber,
cfo.cfoidtransaksi,
SUM(cfo.cfojmlkeluar) as cfokeluar,
it.jmlbarang
FROM
m1_cogs_fifo_out cfo
JOIN m1_item_transaction it ON
cfo.cfosumber = it.sumber AND cfo.cfoidtransaksi = it.iddetail AND cfo.cfoidbarang = it.idbarang AND it.jenismutasi = 0
#WHERE cfo.cfoidbarang = 9453
GROUP BY cfo.cfosumber, cfo.cfoidtransaksi, cfo.cfoidbarang
HAVING cfokeluar <> jmlbarang
SELECT
masuk.gudang,
masuk.bid,
masuk.bkode,
masuk.btipe,
masuk.bnama,
masuk.masuk - IFNULL(keluar.keluar,0) jmltotal,
isw.stok,
masuk.bsatuan,
(CASE masuk.masuk - IFNULL(keluar.keluar,0) WHEN 0 THEN 0 ELSE (masuk.nilaimasuk - IFNULL(keluar.nilaikeluar,0)) / (masuk.masuk - IFNULL(keluar.keluar,0)) END) hpptotal,
masuk.nilaimasuk - IFNULL(keluar.nilaikeluar,0) nilaitotal
FROM
(
SELECT
b.bid,
b.bkode,
b.btipe,
b.bnama,
tb.gudang,
sum(tb.jmlbarang) as masuk,
b.bsatuan,
SUM(tb.hpp) as hppmasuk,
sum(tb.jmlbarang * tb.hpp) as nilaimasuk
FROM
m1_item_transaction tb
JOIN m1_item b ON tb.idbarang = b.bid
WHERE
b.bjenis = 'P'
AND
tb.tgl <= "2020-12-31"
AND
tb.jenismutasi = 1
GROUP BY tb.idbarang, tb.gudang
ORDER BY b.bkode, tb.gudang
) as masuk
LEFT JOIN
(
SELECT
b.bid,
b.bkode,
b.btipe,
b.bnama,
tb.gudang,
sum(tb.jmlbarang) as keluar,
b.bsatuan,
SUM(tb.hpp) as hppkeluar,
sum(tb.jmlbarang * tb.hpp) as nilaikeluar
FROM
m1_item_transaction tb
JOIN m1_item b ON tb.idbarang = b.bid
WHERE
b.bjenis = 'P'
AND
tb.tgl <= "2020-12-31"
AND
tb.jenismutasi = 0
GROUP BY tb.idbarang, tb.gudang
ORDER BY b.bkode, tb.gudang
) as keluar
ON masuk.bid = keluar.bid AND masuk.gudang = keluar.gudang
LEFT JOIN m1_item_stock_warehouse isw ON masuk.bid = isw.idbarang AND masuk.gudang = isw.kgudang
HAVING jmltotal <> stok AND jmltotal > -1
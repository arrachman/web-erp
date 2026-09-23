SELECT
pi.pikategori,
w.wkode,
i.bid,
i.bkode,
i.bnama,
i.bsatuan,
IFNULL(stockout.stok,0) as stok
FROM
m_12_pos_item pi
JOIN m1_item i ON pi.piidbarang = i.bid AND i.bjenis = 'P' AND i.bassembly = 0 AND pi.pikategori = 'GKB'
JOIN m1_location l ON pi.pikategori = l.lkategoripos
JOIN m1_warehouse w ON l.lkode = w.wlokasi
LEFT JOIN
(SELECT
pi.pikategori,
it.gudang,
it.idbarang,
i.bkode,
i.bnama,
i.bsatuan,
SUM((CASE it.jenismutasi WHEN 1 THEN it.jmlbarang ELSE it.jmlbarang * (-1) END)) as stok
FROM
m1_item_transaction it
JOIN m1_item i ON it.idbarang = i.bid AND i.bjenis = 'P' AND i.bassembly = 0 AND it.tgl <= '2015-04-30'
JOIN m_12_pos_item pi ON it.idbarang = pi.piidbarang AND pi.pikategori = 'GKB'
JOIN m1_location l ON pi.pikategori = l.lkategoripos
JOIN m1_warehouse w ON l.lkode = w.wlokasi AND it.gudang = w.wkode
GROUP BY pi.pikategori, w.wkode ASC, i.bkode
#HAVING stok = 0
) AS stockout ON pi.pikategori = stockout.pikategori AND w.wkode = stockout.gudang AND pi.piidbarang = stockout.idbarang
WHERE IFNULL(stockout.stok,0)  = 0
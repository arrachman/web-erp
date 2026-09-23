SELECT
grn.grngudang,
grn.grnsumber,
grn.grnnotransaksi,
grn.grntgl,
grnd.idbarang,
i.bkode,
grnd.namabarang,
grnd.jml,
grnd.satuan,
grnd.nilaisatuan,
grnd.jmlbarang,
grnd.satuanbarang,
grnd.harga,
grnd.diskon,
grnd.jmldiskon,
grnd.pajak1,
grnd.jmlpajak1,
(CASE grn.grnhargatermasukpajak
WHEN 0 THEN (grnd.jmlbarang * grnd.harga) - grnd.jmldiskon
ELSE (grnd.jmlbarang * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1
END) as dpp,
c.kkode,
c.knama,
grn.grnhargatermasukpajak,
(CASE grn.grnhargatermasukpajak
WHEN 0 THEN (grnd.jmlbarang * grnd.harga) - grnd.jmldiskon + grnd.jmlpajak1
ELSE (grnd.jmlbarang * grnd.harga) - grnd.jmldiskon
END) as total
FROM
m4_grn grn
JOIN m4_grn_detail grnd ON grn.grnid = grnd.idgrn
AND grn.grnstatus IN(2,3,4,7)
AND grn.grntgl BETWEEN '2016-04-01' AND '2016-04-31'
AND grn.grnhargatermasukpajak = 0
JOIN m1_item i ON grnd.idbarang = i.bid
JOIN m1_contact c ON grn.grnsupplier = c.kid
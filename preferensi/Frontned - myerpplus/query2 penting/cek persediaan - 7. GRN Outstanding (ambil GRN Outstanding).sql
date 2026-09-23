SELECT
*
FROM
(SELECT
grn.grnid,
grn.grngudang,
grn.grnnotransaksi,
grn.grntgl,
grn.grnsupplier,
c.kkode,
c.knama,
grn.grnhargatermasukpajak,
grnd.idgrndetail,
grnd.idbarang,
i.bkode,
grnd.namabarang,
grnd.jmlbarang,
grnd.satuanbarang,
grnd.harga,
(grnd.jmlbarang * grnd.harga) as subtotal,
grnd.diskon,
grnd.jmldiskon,
(CASE grn.grnhargatermasukpajak
WHEN 0 THEN (grnd.jmlbarang * grnd.harga) - grnd.jmldiskon
ELSE (grnd.jmlbarang * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1
END) as dpp,
grnd.pajak1,
grnd.jmlpajak1,
(CASE grn.grnhargatermasukpajak
WHEN 0 THEN (grnd.jmlbarang * grnd.harga) - grnd.jmldiskon + grnd.jmlpajak1
ELSE (grnd.jmlbarang * grnd.harga) - grnd.jmldiskon
END) as total,
grnd.jmlrealisasi,
grnd.statusrealisasi,
grnd.urutan
FROM
m4_grn grn
JOIN m4_grn_detail grnd ON grn.grnid = grnd.idgrn
AND grn.grnstatus IN(2,3,4,7)
AND grn.grntgl BETWEEN '2016-01-01' AND '2016-01-31'
JOIN m1_item i ON grnd.idbarang = i.bid
JOIN m1_contact c ON grn.grnsupplier = c.kid
) as gr

LEFT JOIN

(SELECT
ri.riid,
ri.rinotransaksi,
ri.ritgl,
rid.idridetail,
rid.idbarang,
rid.namabarang,
rid.jmlbarang,
rid.satuanbarang,
rid.harga,
rid.diskon,
rid.jmldiskon,
rid.pajak1,
rid.jmlpajak1,
rid.idgrndetail
FROM
m4_ri ri
JOIN m4_ri_detail rid ON ri.riid = rid.idri
AND ri.ristatus IN(2,3,4,7)
AND ri.ritgl BETWEEN '2016-01-01' AND '2016-01-31'
JOIN m4_grn_detail grnd ON rid.idgrndetail = grnd.idgrndetail
JOIN m4_grn grn ON grn.grnid = grnd.idgrn
AND grn.grnstatus IN(2,3,4,7)
AND grn.grntgl BETWEEN '2016-01-01' AND '2016-01-31'
) as pj

ON gr.idgrndetail = pj.idgrndetail

WHERE pj.idgrndetail IS NULL

ORDER BY gr.grntgl, gr.grnnotransaksi, gr.urutan

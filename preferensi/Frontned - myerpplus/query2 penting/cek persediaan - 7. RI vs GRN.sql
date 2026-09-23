SELECT
ri.riid,
ri.rigudang,
ri.rinotransaksi,
ri.ritgl,
ri.risupplier,
c.kkode,
c.knama,
ri.rihargatermasukpajak,
rid.idridetail,
rid.idbarang,
i.bkode,
rid.namabarang,
rid.jmlbarang,
rid.satuanbarang,
rid.harga,
(rid.jmlbarang * rid.harga) as subtotal,
rid.diskon,
rid.jmldiskon,
rid.pajak1,
rid.jmlpajak1,
ROUND(
(CASE ri.rihargatermasukpajak
WHEN 0 THEN (rid.jmlbarang * rid.harga) - rid.jmldiskon
ELSE (rid.jmlbarang * rid.harga) - rid.jmldiskon - rid.jmlpajak1
END)
,2) as dpp,
grn.grnnotransaksi,
(grnd.jmlbarang * grnd.harga) as subtotal,
grnd.diskon as diskongrn,
grnd.jmldiskon as jmldiskongrn,
grnd.pajak1 as pajak1grn,
grnd.jmlpajak1 as jmlpajak1grn,
ROUND(
(CASE grn.grnhargatermasukpajak
WHEN 0 THEN (grnd.jmlbarang * grnd.harga) - grnd.jmldiskon
ELSE (grnd.jmlbarang * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1
END)
,2) as dppgrn
FROM
m4_ri ri
JOIN m4_ri_detail rid ON ri.riid = rid.idri
AND ri.ristatus IN(2,3,4,7)
AND ri.ritgl BETWEEN '2016-02-01' AND '2016-02-31'
AND ri.rijenispembeliankategori = 0
JOIN m1_item i ON rid.idbarang = i.bid
JOIN m1_contact c ON ri.risupplier = c.kid
JOIN m4_grn_detail grnd ON rid.idgrndetail = grnd.idgrndetail
JOIN m4_grn grn ON grnd.idgrn = grn.grnid
HAVING ROUND(dpp,2) <> ROUND(dppgrn,2)
SELECT
ri.rigudang,
ri.risumber,
ri.rinotransaksi,
ri.ritgl,
rid.idbarang,
i.bkode,
rid.namabarang,
rid.jml,
rid.satuan,
rid.nilaisatuan,
rid.jmlbarang,
rid.satuanbarang,
rid.harga,
rid.diskon,
rid.jmldiskon,
rid.pajak1,
rid.jmlpajak1,
(CASE ri.rihargatermasukpajak
WHEN 0 THEN (rid.jmlbarang * rid.harga) - rid.jmldiskon
ELSE (rid.jmlbarang * rid.harga) - rid.jmldiskon - rid.jmlpajak1
END) as dpp,
c.kkode,
c.knama,
ri.rihargatermasukpajak,
(CASE ri.rihargatermasukpajak
WHEN 0 THEN (rid.jmlbarang * rid.harga) - rid.jmldiskon + rid.jmlpajak1
ELSE (rid.jmlbarang * rid.harga) - rid.jmldiskon
END) as total
FROM
m4_ri ri
JOIN m4_ri_detail rid ON ri.riid = rid.idri
AND ri.ristatus IN(2,3,4,7)
AND ri.ritgl BETWEEN '2016-04-01' AND '2016-04-31'
AND ri.rijenispembeliankategori = 1
JOIN m1_item i ON rid.idbarang = i.bid 
AND (i.bjenis <> 'J')
JOIN m1_contact c ON ri.risupplier = c.kid
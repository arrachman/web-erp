SELECT
sr.srgudang,
sr.srsumber,
sr.srnotransaksi,
sr.srtgl,
srd.idbarang,
i.bkode,
srd.namabarang,
srd.jml,
srd.satuan,
srd.nilaisatuan,
srd.jmlbarang,
srd.satuanbarang,
srd.harga,
srd.diskon,
srd.jmldiskon,
srd.pajak1,
srd.jmlpajak1,
(CASE sr.srhargatermasukpajak
WHEN 0 THEN (srd.jmlbarang * srd.hpp) - srd.jmldiskon
ELSE (srd.jmlbarang * srd.hpp) - srd.jmldiskon - srd.jmlpajak1
END) as dpp,
c.kkode,
c.knama,
sr.srhargatermasukpajak,
(CASE sr.srhargatermasukpajak
WHEN 0 THEN (srd.jmlbarang * srd.harga) - srd.jmldiskon + srd.jmlpajak1
ELSE (srd.jmlbarang * srd.harga) - srd.jmldiskon
END) as total
FROM
m5_sr sr
JOIN m5_sr_detail srd ON sr.srid = srd.idsr
AND sr.srstatus IN(2,3,4,7)
AND sr.srtgl BETWEEN '2016-04-01' AND '2016-04-31'
#AND sr.srjenispembeliankategori = 1
JOIN m1_item i ON srd.idbarang = i.bid 
AND (i.bjenis <> 'J')
JOIN m1_contact c ON sr.srcustomer = c.kid
ORDER BY idsrdetail
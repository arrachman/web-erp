SELECT
prt.prtgudang,
prt.prtsumber,
prt.prtnotransaksi,
prt.prttgl,
prtd.idbarang,
i.bkode,
prtd.namabarang,
prtd.jml,
prtd.satuan,
prtd.nilaisatuan,
prtd.jmlbarang,
prtd.satuanbarang,
prtd.harga,
prtd.diskon,
prtd.jmldiskon,
prtd.pajak1,
prtd.jmlpajak1,
(CASE prt.prthargatermasukpajak
WHEN 0 THEN (prtd.jmlbarang * prtd.harga) - prtd.jmldiskon
ELSE (prtd.jmlbarang * prtd.harga) - prtd.jmldiskon - prtd.jmlpajak1
END) as dpp,
c.kkode,
c.knama,
prt.prthargatermasukpajak,
(CASE prt.prthargatermasukpajak
WHEN 0 THEN (prtd.jmlbarang * prtd.harga) - prtd.jmldiskon + prtd.jmlpajak1
ELSE (prtd.jmlbarang * prtd.harga) - prtd.jmldiskon
END) as total
FROM
m4_prt prt
JOIN m4_prt_detail prtd ON prt.prtid = prtd.idprt
AND prt.prtstatus IN(2,3,4,7)
AND prt.prttgl BETWEEN '2016-04-01' AND '2016-04-31'
#AND prt.prtjenispembeliankategori = 1
JOIN m1_item i ON prtd.idbarang = i.bid 
AND (i.bjenis <> 'J')
JOIN m1_contact c ON prt.prtsupplier = c.kid
ORDER BY idprtdetail
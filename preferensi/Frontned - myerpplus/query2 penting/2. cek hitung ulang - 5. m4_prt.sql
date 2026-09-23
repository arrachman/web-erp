SELECT
prt.prthargatermasukpajak,
prtd.jml, 
prtd.harga,
prtd.jmldiskon,
prtd.jmlpajak1,
it.hpp, 
(CASE prt.prthargatermasukpajak
WHEN 0 THEN ((prtd.jml * prtd.harga) - prtd.jmldiskon) / prtd.jml
ELSE ((prtd.jml * prtd.harga) - prtd.jmldiskon - prtd.jmlpajak1) / prtd.jml
END) as prthpp
FROM
m1_item_transaction it
JOIN m4_prt_detail prtd ON it.sumber = 'prt'
AND it.idutama = prtd.idprt AND it.iddetail = prtd.idprtdetail AND it.idbarang = prtd.idbarang
AND it.jenismutasi = 0
AND it.tgl BETWEEN '2016-04-01' AND '2016-04-31'
JOIN m4_prt prt ON prtd.idprt = prt.prtid
AND ROUND(it.hpp,2) <> ROUND((CASE prt.prthargatermasukpajak
WHEN 0 THEN ((prtd.jml * prtd.harga) - prtd.jmldiskon) / prtd.jml
ELSE ((prtd.jml * prtd.harga) - prtd.jmldiskon - prtd.jmlpajak1) / prtd.jml
END),2)  

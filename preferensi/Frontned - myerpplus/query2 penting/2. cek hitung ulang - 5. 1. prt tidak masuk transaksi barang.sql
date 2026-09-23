SELECT
gr.*
FROM
(SELECT
0 as id,
prt.prtcabang as cabang,
prt.prtlokasi as lokasi,
prt.prtgudang as gudang,
prt.prtkodepa as kodepa,
1 as jenismutasi,
prt.prtsumber as sumber,
prt.prtid as idutama,
prtd.idprtdetail as iddetail,
prt.prtnotransaksi as notransaksi,
prt.prttgl as tgl,
prt.prtsupplier as kontak,
prtd.idbarang as idbarang,
prtd.namabarang as namabarang,
prtd.tipebarang as tipebarang,
i.bhpp as tipehpp,
prtd.jml as jml,
prtd.satuan as satuan,
prtd.jmlbarang as jmlbarang,
prtd.satuanbarang as satuanbarang,
prt.prtmatauang as matauang,
prt.prtkurs as kurs,
prtd.harga as harga,
prtd.diskon as diskon,
prtd.jmldiskon as jmldiskon,
0 as idhppikm,
0 as idhppikk,
0 as sidhppfifo,
(CASE prt.prthargatermasukpajak
WHEN 0 THEN ((prtd.jml * prtd.harga) - prtd.jmldiskon) / prtd.jml
ELSE ((prtd.jml * prtd.harga) - prtd.jmldiskon - prtd.jmlpajak1) / prtd.jml
END) as hpp,
prt.prturaian as uraian,
prt.prtcatatan as catatan,
prtd.catatan as catatandetail,
prtd.costcenter as costcenter,
prtd.divisi as divisi,
prtd.subdivisi as subdivisi,
prtd.proyek as proyek,
0 as saldojml,
0 as saldohpp,
0 as saldonilai,
prt.prtinputtgl as inputtgl,
prt.prtinputuser as inputuser,
prt.prtpostingtgl as postingtgl,
0 as updatehpp,
prt.prtpostingtgl as postinghpp,
0 as hppfix,
0 as postingjurnal,
0 as jurnalfix,
0 as tutupperiode,
0 as isclose,
'' as customtext1,
'' as customtext2,
'' as customtext3,
'' as customtext4,
'' as customtext5,
'' as customtext6,
'' as customtext7,
'' as customtext8,
'' as customtext9,
'' as customtext10,
'0' as customint1,
'0' as customint2,
'0' as customint3,
'0' as customint4,
'0' as customint5,
'0' as customint6,
'0' as customint7,
'0' as customint8,
'0' as customint9,
'0' as customint10,
'0' as customdbl1,
'0' as customdbl2,
'0' as customdbl3,
'0' as customdbl4,
'0' as customdbl5,
'0' as customdbl6,
'0' as customdbl7,
'0' as customdbl8,
'0' as customdbl9,
'0' as customdbl10,
'1900-01-01' as customdate1,
'1900-01-01' as customdate2,
'1900-01-01' as customdate3,
'1900-01-01' as customdate4,
'1900-01-01' as customdate5,
'1900-01-01' as customdate6,
'1900-01-01' as customdate7,
'1900-01-01' as customdate8,
'1900-01-01' as customdate9,
'1900-01-01' as customdate10
FROM
m4_prt prt
JOIN m4_prt_detail prtd ON prt.prtid = prtd.idprt
AND prt.prtstatus IN (2,3,4,7)
AND prt.prttgl BETWEEN '2016-04-01' AND '2016-04-31'
JOIN m1_item i ON prtd.idbarang = i.bid) as gr

LEFT JOIN 

(SELECT
m1_item_transaction.id,
m1_item_transaction.sumber,
m1_item_transaction.idutama,
m1_item_transaction.iddetail,
m1_item_transaction.idbarang
FROM m1_item_transaction
WHERE sumber = 'prt' AND tgl BETWEEN '2016-04-01' AND '2016-04-31'
) as it

ON gr.sumber = it.sumber
AND gr.idutama = it.idutama
AND gr.iddetail = it.iddetail
AND gr.idbarang = it.idbarang

WHERE it.id IS NULL
SELECT
gr.*
FROM
(SELECT
0 as id,
grn.grncabang as cabang,
grn.grnlokasi as lokasi,
grn.grngudang as gudang,
grn.grnkodepa as kodepa,
1 as jenismutasi,
grn.grnsumber as sumber,
grn.grnid as idutama,
grnd.idgrndetail as iddetail,
grn.grnnotransaksi as notransaksi,
grn.grntgl as tgl,
grn.grnsupplier as kontak,
grnd.idbarang as idbarang,
grnd.namabarang as namabarang,
grnd.tipebarang as tipebarang,
i.bhpp as tipehpp,
grnd.jml as jml,
grnd.satuan as satuan,
grnd.jmlbarang as jmlbarang,
grnd.satuanbarang as satuanbarang,
grn.grnmatauang as matauang,
grn.grnkurs as kurs,
grnd.harga as harga,
grnd.diskon as diskon,
grnd.jmldiskon as jmldiskon,
0 as idhppikm,
0 as idhppikk,
0 as sidhppfifo,
(CASE grn.grnhargatermasukpajak
WHEN 0 THEN ((grnd.jml * grnd.harga) - grnd.jmldiskon) / grnd.jml
ELSE ((grnd.jml * grnd.harga) - grnd.jmldiskon - grnd.jmlpajak1) / grnd.jml
END) as hpp,
grn.grnuraian as uraian,
grn.grncatatan as catatan,
grnd.catatan as catatandetail,
grnd.costcenter as costcenter,
grnd.divisi as divisi,
grnd.subdivisi as subdivisi,
grnd.proyek as proyek,
0 as saldojml,
0 as saldohpp,
0 as saldonilai,
grn.grninputtgl as inputtgl,
grn.grninputuser as inputuser,
grn.grnpostingtgl as postingtgl,
0 as updatehpp,
0 as postinghpp,
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
m4_grn grn
JOIN m4_grn_detail grnd ON grn.grnid = grnd.idgrn
AND grn.grnstatus IN (2,3,4,7)
AND grn.grntgl BETWEEN '2016-04-01' AND '2016-04-31'
JOIN m1_item i ON grnd.idbarang = i.bid) as gr

LEFT JOIN 

(SELECT
m1_item_transaction.id,
m1_item_transaction.sumber,
m1_item_transaction.idutama,
m1_item_transaction.iddetail,
m1_item_transaction.idbarang
FROM m1_item_transaction
WHERE sumber = 'GRN' AND tgl BETWEEN '2016-04-01' AND '2016-04-31'
) as it

ON gr.sumber = it.sumber
AND gr.idutama = it.idutama
AND gr.iddetail = it.iddetail
AND gr.idbarang = it.idbarang

WHERE it.id IS NULL
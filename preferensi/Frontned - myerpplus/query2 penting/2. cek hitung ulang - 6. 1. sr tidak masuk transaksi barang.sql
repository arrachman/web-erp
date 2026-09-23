SELECT
gr.*
FROM
(SELECT
0 as id,
sr.srcabang as cabang,
sr.srlokasi as lokasi,
sr.srgudang as gudang,
sr.srkodepa as kodepa,
1 as jenismutasi,
sr.srsumber as sumber,
sr.srid as idutama,
srd.idsrdetail as iddetail,
sr.srnotransaksi as notransaksi,
sr.srtgl as tgl,
sr.srcustomer as kontak,
srd.idbarang as idbarang,
srd.namabarang as namabarang,
srd.tipebarang as tipebarang,
i.bhpp as tipehpp,
srd.jml as jml,
srd.satuan as satuan,
srd.jmlbarang as jmlbarang,
srd.satuanbarang as satuanbarang,
sr.srmatauang as matauang,
sr.srkurs as kurs,
srd.harga as harga,
srd.diskon as diskon,
srd.jmldiskon as jmldiskon,
0 as idhppikm,
0 as idhppikk,
0 as sidhppfifo,
(CASE sr.srhargatermasukpajak
WHEN 0 THEN ((srd.jml * srd.harga) - srd.jmldiskon) / srd.jml
ELSE ((srd.jml * srd.harga) - srd.jmldiskon - srd.jmlpajak1) / srd.jml
END) as hpp,
sr.sruraian as uraian,
sr.srcatatan as catatan,
srd.catatan as catatandetail,
srd.costcenter as costcenter,
srd.divisi as divisi,
srd.subdivisi as subdivisi,
srd.proyek as proyek,
0 as saldojml,
0 as saldohpp,
0 as saldonilai,
sr.srinputtgl as inputtgl,
sr.srinputuser as inputuser,
sr.srpostingtgl as postingtgl,
0 as updatehpp,
sr.srpostingtgl as postinghpp,
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
m5_sr sr
JOIN m5_sr_detail srd ON sr.srid = srd.idsr
AND sr.srstatus IN (2,3,4,7)
AND sr.srtgl BETWEEN '2016-04-01' AND '2016-04-31'
JOIN m1_item i ON srd.idbarang = i.bid) as gr

LEFT JOIN 

(SELECT
m1_item_transaction.id,
m1_item_transaction.sumber,
m1_item_transaction.idutama,
m1_item_transaction.iddetail,
m1_item_transaction.idbarang
FROM m1_item_transaction
WHERE sumber = 'sr' AND tgl BETWEEN '2016-04-01' AND '2016-04-31'
) as it

ON gr.sumber = it.sumber
AND gr.idutama = it.idutama
AND gr.iddetail = it.iddetail
AND gr.idbarang = it.idbarang

WHERE it.id IS NULL
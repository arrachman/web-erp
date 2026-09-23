SELECT
gr.*
FROM
(SELECT
0 as id,
si.sicabang as cabang,
si.silokasi as lokasi,
si.sigudang as gudang,
si.sikodepa as kodepa,
0 as jenismutasi,
si.sisumber as sumber,
si.siid as idutama,
sid.idsidetail as iddetail,
si.sinotransaksi as notransaksi,
si.sitgl as tgl,
si.sicustomer as kontak,
sid.idbarang as idbarang,
sid.namabarang as namabarang,
sid.tipebarang as tipebarang,
i.bhpp as tipehpp,
sid.jml as jml,
sid.satuan as satuan,
sid.jmlbarang as jmlbarang,
sid.satuanbarang as satuanbarang,
si.simatauang as matauang,
si.sikurs as kurs,
sid.harga as harga,
sid.diskon as diskon,
sid.jmldiskon as jmldiskon,
0 as idhppikm,
0 as idhppikk,
0 as sidhppfifo,
(CASE si.sihargatermasukpajak
WHEN 0 THEN ((sid.jml * sid.harga) - sid.jmldiskon) / sid.jml
ELSE ((sid.jml * sid.harga) - sid.jmldiskon - sid.jmlpajak1) / sid.jml
END) as hpp,
si.siuraian as uraian,
si.sicatatan as catatan,
sid.catatan as catatandetail,
sid.costcenter as costcenter,
sid.divisi as divisi,
sid.subdivisi as subdivisi,
sid.proyek as proyek,
0 as saldojml,
0 as saldohpp,
0 as saldonilai,
si.siinputtgl as inputtgl,
si.siinputuser as inputuser,
si.sipostingtgl as postingtgl,
0 as updatehpp,
1 as postinghpp,
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
m5_si si
JOIN m5_si_detail sid ON si.siid = sid.idsi
AND si.sistatus IN (2,3,4,7)
AND si.sitgl BETWEEN '2016-04-04' AND '2016-04-21'
JOIN m1_item i ON sid.idbarang = i.bid) as gr

LEFT JOIN 

(SELECT
m1_item_transaction.id,
m1_item_transaction.sumber,
m1_item_transaction.idutama,
m1_item_transaction.iddetail,
m1_item_transaction.idbarang
FROM m1_item_transaction
WHERE sumber = 'sI' AND tgl BETWEEN '2016-04-04' AND '2016-04-21'
) as it

ON gr.sumber = it.sumber
AND gr.idutama = it.idutama
AND gr.iddetail = it.iddetail
AND gr.idbarang = it.idbarang

WHERE it.id IS NULL
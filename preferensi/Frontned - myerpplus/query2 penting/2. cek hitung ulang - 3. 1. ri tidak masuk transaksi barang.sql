#INSERT INTO m1_item_transaction(
SELECT
gr.*
FROM
(SELECT
0 as id,
ri.ricabang as cabang,
ri.rilokasi as lokasi,
ri.rigudang as gudang,
ri.rikodepa as kodepa,
1 as jenismutasi,
ri.risumber as sumber,
ri.riid as idutama,
rid.idridetail as iddetail,
ri.rinotransaksi as notransaksi,
ri.ritgl as tgl,
ri.risupplier as kontak,
rid.idbarang as idbarang,
rid.namabarang as namabarang,
rid.tipebarang as tipebarang,
i.bhpp as tipehpp,
rid.jml as jml,
rid.satuan as satuan,
rid.jmlbarang as jmlbarang,
rid.satuanbarang as satuanbarang,
ri.rimatauang as matauang,
ri.rikurs as kurs,
rid.harga as harga,
rid.diskon as diskon,
rid.jmldiskon as jmldiskon,
0 as idhppikm,
0 as idhppikk,
0 as sidhppfifo,
(CASE ri.rihargatermasukpajak
WHEN 0 THEN ((rid.jml * rid.harga) - rid.jmldiskon) / rid.jml
ELSE ((rid.jml * rid.harga) - rid.jmldiskon - rid.jmlpajak1) / rid.jml
END) as hpp,
ri.riuraian as uraian,
ri.ricatatan as catatan,
rid.catatan as catatandetail,
rid.costcenter as costcenter,
rid.divisi as divisi,
rid.subdivisi as subdivisi,
rid.proyek as proyek,
0 as saldojml,
0 as saldohpp,
0 as saldonilai,
ri.riinputtgl as inputtgl,
ri.riinputuser as inputuser,
ri.ripostingtgl as postingtgl,
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
m4_ri ri
JOIN m4_ri_detail rid ON ri.riid = rid.idri
AND ri.ristatus IN (2,3,4,7)
AND ri.ritgl BETWEEN '2016-04-01' AND '2016-04-31'
AND ri.rijenispembeliankategori = 1
JOIN m1_item i ON rid.idbarang = i.bid) as gr

LEFT JOIN 

(SELECT
m1_item_transaction.id,
m1_item_transaction.sumber,
m1_item_transaction.idutama,
m1_item_transaction.iddetail,
m1_item_transaction.idbarang
FROM m1_item_transaction
WHERE sumber = 'ri' AND tgl BETWEEN '2016-04-01' AND '2016-04-31'
) as it

ON gr.sumber = it.sumber
AND gr.idutama = it.idutama
AND gr.iddetail = it.iddetail
AND gr.idbarang = it.idbarang

WHERE it.id IS NULL
#)
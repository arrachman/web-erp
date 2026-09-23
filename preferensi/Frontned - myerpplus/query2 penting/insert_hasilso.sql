SELECT
0 AS idspdetail, 367 AS idsp, i2.bid AS idbarang, i2.bnama AS namabarang, i2.btipe AS tipebarang,
hs.jmlsistem - hs.jmlterjual AS jmlsistem, hs.jmlfisik AS jmlfisik, hs.jmlfisik AS jmlbagus, 0 AS jmlrusak, selisih AS selisih,
i2.bsatuan AS satuan, i2.bnilaisatuan AS nilaisatuan, 
hs.jmlsistem - hs.jmlterjual AS jmlbrgsistem, hs.jmlfisik AS jmlbrgfisik, hs.jmlfisik AS jmlbrgbagus, 0 AS jmlrusak, selisih AS jmlbarangselisih, i2.bsatuan AS satuan,
"" AS cabang, "PJN" AS lokasi, "PJN" AS gudang, hs.lokasibarang AS lokasibarang, 0 AS jmlsa, 0 AS statussa, "" AS coscenter, "" AS divisi, "" AS subdivisi, "" AS proyek, "" AS catatan,
0 AS urutan, 0 AS isclose, "" AS customtext1, "" AS customtext2, "" AS customtext3, 0 AS customdbl1, 0 AS customdbl2, 0 AS customdbl3,
"1900-01-01" AS customdate1,"1900-01-01" AS customdate2,"1900-01-01" AS customdate3

FROM
hasil_so hs

LEFT JOIN 

(SELECT
i.bkode
FROM `m3_sp` sp 
JOIN m3_sp_detail spd ON sp.spid = spd.idsp AND sp.spid IN (376,375
,373
,372
,370
,369
,368
)
JOIN m1_item i ON spd.idbarang = i.bid) as sd

ON hs.kodebarang = sd.bkode
JOIN m1_item i2 ON i2.bkode = hs.kodebarang

WHERE sd.bkode IS NULL
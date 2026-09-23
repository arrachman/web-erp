INSERT INTO m3_sa_detail(
SELECT ps.*,
@urut:=@urut+1 as urutan,
0 as isclose,
'' as customtext1,
'' as customtext2,
'' as customtext3,
0 as customdbl1,
0 as customdbl2,
0 as customdbl3,
'1900-01-01' as customdate1,
'1900-01-01' as customdate2,
'1900-01-01' as customdate3
FROM
(
SELECT
0 as idsadetail,
sa.said as idsa,
i.bid as idbarang,
i.bnama as namabarang,
i.btipe as tipebarang,
SUM(sad.stokjual)-sad.stoktersedia as jmlmasuk,
0 as jmlkeluar,
i.bsatuan as satuan,
i.bnilaisatuan as nilaisatuan,
SUM(sad.stokjual)-sad.stoktersedia as jmlbarangmasuk,
0 as jmlbarangkeluar,
i.bsatuan as satuanbarang,
0 as idhppkhususmasuk,
(CASE i.bhppaverage WHEN 0 THEN 1 ELSE i.bhppaverage END) as hpplama,
(CASE i.bhppaverage WHEN 0 THEN 1 ELSE i.bhppaverage END) as hpp,
i.brekpersediaan as rekpersediaan,
tsa.tsarek as reklawan,
0 as idspdetail,
sa.sacabang as cabang,
sa.salokasi as lokasi,
sa.sagudang as gudang,
'' as costcenter,
'' as divisi,
'' as subdivisi,
'' as proyek,
'' as catatan
FROM m3_sa sa
JOIN m1_type_sa tsa ON sa.sajenis = tsa.tsakode
JOIN m2r_stok_gagal_upload sad ON sa.said = 2407
JOIN m1_item i ON sad.idbarang = i.bid
GROUP BY sad.gudang ASC, i.bid ASC) as ps, (SELECT @urut := 0) as variableinit
)
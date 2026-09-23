SELECT
'idlogin' as idlogin,
sk.id as ksnourut,
0 as ksid,
'' as ksgudang,
'' as ksgudangnama,
i.bkategori as kskategoribarang,
ic.icnama as kskategoribarangnama,
sk.id as ksidbarang,
i.bkode as kskodebarang,
i.btipe as kstipebarang,
i.bnama as ksnamabarang,
i.bsatuan as kssatuanbarang,
'2015-11-31' as kstgl,
'KS' as kssumber,
'KS' as ksnotransaksi,
1 as kskontak,
'KS' as kskontakkode,
'KS' as kskontaknama,
'KS' as ksuraian,
'' as kscatatan,
'' as kscatatandetail,
'Rp' as ksmatauang,
1 as kskurs,
0 as ksharga,
0 as ksdiskon,
0 as ksjmldiskon,
1 ksjenismutasi,
IFNULL(ms.jml,0) as ksjmlmasuk,
IFNULL(ms.hpp,0) as kshargamasuk,
IFNULL(ms.nilai,0) as ksnilaimasuk,
IFNULL(kl.jml*-1,0) as ksjmlkeluar,
IFNULL(kl.hpp,0) as kshargakeluar,
IFNULL(kl.nilai*-1,0) as ksnilaikeluar,
sk.jml as kssaldojml,
sk.hpp as kssaldohpp,
sk.nilai as kssaldonilai,
'1971-01-01 00:00:00' as kspostingtgl,
'1971-01-01 00:00:00' as ksinputtgl,
'idmsmq' as idmsmq,
1 as ksuserid,
'' as kscustomtext1,
'' as kscustomtext2,
'' as kscustomtext3,
'' as kscustomtext4,
'' as kscustomtext5,
'0' as kscustomint1,
'0' as kscustomint2,
'0' as kscustomint3,
'0' as kscustomint4,
'0' as kscustomint5,
IFNULL(sa.jml,0) as kscustomdbl1,
IFNULL(sa.hpp,0) as kscustomdbl2,
IFNULL(sa.nilai,0) as kscustomdbl3,
'0' as kscustomdbl4,
'0' as kscustomdbl5,
'1900-01-01' as kscustomdate1,
'1900-01-01' as kscustomdate2,
'1900-01-01' as kscustomdate3,
'1900-01-01' as kscustomdate4,
'1900-01-01' as kscustomdate5
FROM 0_hppsk sk
JOIN m1_item i ON sk.id = i.bid #AND i.bid = 18498
LEFT JOIN m1_item_category ic ON i.bkategori = ic.ickode
LEFT JOIN 0_hppsa sa ON sk.id = sa.id
LEFT JOIN 0_hppms ms ON sk.id = ms.id
LEFT JOIN 0_hppkl kl ON sk.id = kl.id
WHERE
IFNULL(ms.jml,0) <> 0 OR
IFNULL(ms.hpp,0) <> 0 OR
IFNULL(ms.nilai,0) <> 0 OR
IFNULL(kl.jml,0) <> 0 OR
IFNULL(kl.hpp,0) <> 0 OR
IFNULL(kl.nilai,0) <> 0 OR
sk.jml <> 0 OR
sk.hpp <> 0 OR
sk.nilai <> 0 OR
IFNULL(sa.jml,0) <> 0 OR
IFNULL(sa.hpp,0) <> 0 OR
IFNULL(sa.nilai,0) <> 0
ORDER BY
i.bkategori ASC, i.bkode ASC
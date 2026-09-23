SELECT
'idlogin' as idlogin, 
1 as ksnourut, 
1 as ksid, 
skj.gudang as ksgudang, 
w.wnama as ksgudangnama, 
i.bkategori as kskategoribarang, 
ic.icnama as kskategoribarangnama, 
i.bid as ksidbarang, 
i.bkode as kskodebarang, 
i.btipe as kstipebarang, 
i.bnama as ksnamabarang, 
i.bsatuan as kssatuanbarang, 
'2015-11-31' as kstgl, 
'KS' as kssumber, 
'' as ksnotransaksi, 
0 as kskontak, 
'' as kskontakkode, 
'' as kskontaknama, 
'' as ksuraian, 
'' as kscatatan, 
'' as kscatatandetail, 
'Rp' as ksmatauang, 
1 as kskurs, 
0 as ksharga, 
0 as ksdiskon, 
0 as ksjmldiskon, 
1 as ksjenismutasi, 
IFNULL(saj.jml, 0) as ksjmlmasuk, 
IFNULL(sah.hpp, 0) as kshargamasuk, 
IFNULL(saj.jml, 0) * IFNULL(sah.hpp, 0) as ksnilaimasuk, 
skj.jml - IFNULL(saj.jml, 0) as ksjmlkeluar, 
(CASE skj.jml - IFNULL(saj.jml, 0)
	WHEN 0 THEN 0 
	ELSE ((IFNULL(skj.jml, 0) * IFNULL(skh.hpp, 0)) - (IFNULL(saj.jml, 0) * IFNULL(sah.hpp, 0))) / (skj.jml - IFNULL(saj.jml, 0)) 
END) as kshargakeluar, 
((IFNULL(skj.jml, 0) * IFNULL(skh.hpp, 0)) - (IFNULL(saj.jml, 0) * IFNULL(sah.hpp, 0))) as ksnilaikeluar, 
skj.jml as kssaldojml, 
IFNULL(skh.hpp, 0) as kssaldohpp, 
(IFNULL(skj.jml, 0) * IFNULL(skh.hpp, 0)) as kssaldonilai, 
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
'0' as kscustomdbl1, 
'0' as kscustomdbl2, 
'0' as kscustomdbl3, 
'0' as kscustomdbl4, 
'0' as kscustomdbl5, 
'1900-01-01' as kscustomdate1, 
'1900-01-01' as kscustomdate2, 
'1900-01-01' as kscustomdate3, 
'1900-01-01' as kscustomdate4, 
'1900-01-01' as kscustomdate5
FROM
0_hppgudangsk_jml skj
JOIN m1_warehouse w ON skj.gudang = w.wkode
JOIN m1_item i ON skj.id = i.bid
LEFT JOIN m1_item_category ic ON i.bkategori = ic.ickode
LEFT JOIN 0_hppgudangsk skh ON skj.id = skh.id
LEFT JOIN 0_hppgudangsa_jml saj ON skj.gudang = saj.gudang AND skj.id = saj.id
LEFT JOIN 0_hppgudangsa sah ON saj.id = sah.id
ORDER BY skj.gudang ASC, i.bkategori ASC, i.bkode ASC
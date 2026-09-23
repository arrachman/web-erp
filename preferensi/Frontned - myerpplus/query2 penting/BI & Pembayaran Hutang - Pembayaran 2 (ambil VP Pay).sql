SELECT
#vp.vpid,
#vp.vptgl,
vp.vpnotransaksi,
#vpd.carabayar,
vpd.bank,
vpd.jumlah,
vpd.rekbank,
c.cnama,
vp.vpuraian,
vpd.catatan,
vpd.noacbank
FROM
m4_vp vp
JOIN m4_vp_pay vpd ON vp.vpid = vpd.idvp AND vp.vpstatus IN(2,3,4,7)
AND vp.vptgl BETWEEN '2016-03-01' AND '2016-03-31'
LEFT JOIN m1_coa c ON vpd.rekbank = c.cnomor
ORDER BY vp.vpid, vpd.idvpcarabayar

SELECT
#vp.vpid,
vp.vptgl,
vp.vpnotransaksi,
#vp.vpsupplier,
c.kkode,
c.knama,
vpp.vppnotransaksi,
(CASE vpd.sumber
WHEN 'ri' THEN ri.rinotransaksi
WHEN 'prt' THEN prt.prtnotransaksi
ELSE CONCAT(vpd.sumber," (",coa.cnama,")")
END) as notransaksi,
(CASE vpd.sumber
WHEN 'ri' THEN ri.ritotaltransaksi
WHEN 'prt' THEN prt.prttotaltransaksi * -1
ELSE vpd.jmlbayar
END) as totaltransaksi,
(CASE vpd.sumber
WHEN 'ri' THEN vpd.jmlbayar
WHEN 'prt' THEN vpd.jmlbayar * -1
ELSE vpd.jmlbayar
END) as jmlbayar,
vpd.catatan
FROM
m4_vp vp
JOIN m4_vp_detail vpd ON vp.vpid = vpd.idvp AND vp.vpstatus IN(2,3,4,7)
AND vp.vptgl BETWEEN '2016-02-01' AND '2016-02-31'
JOIN m1_contact c ON vp.vpsupplier = c.kid
LEFT JOIN m4_vpp_detail vppd ON vpd.idvppdetail = vppd.idvppdetail
LEFT JOIN m4_vpp vpp ON vppd.idvpp = vpp.vppid
LEFT JOIN m4_ri ri ON vpd.sumber = ri.risumber AND vpd.idtransaksi = ri.riid
LEFT JOIN m4_prt prt ON vpd.sumber = prt.prtsumber AND vpd.idtransaksi = prt.prtid
LEFT JOIN m1_coa coa ON vpd.rekhutangpiutang = coa.cnomor
ORDER BY vp.vpid, vpd.idvpdetail

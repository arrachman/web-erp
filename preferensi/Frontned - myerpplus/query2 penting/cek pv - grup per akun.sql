SELECT
pvd.rekhutangpiutang, 
c.cnama,
SUM(pvd.jmlbayar)
FROM `m5_pv_detail` pvd
JOIN m5_pv pv ON pvd.idpv = pv.pvid AND pv.pvstatus IN(2,3,4,7) AND pv.pvtgl BETWEEN '2015-06-01' AND '2015-06-31' AND pvd.sumber = 'CA' 
LEFT JOIN m1_coa c ON pvd.rekhutangpiutang = c.cnomor
GROUP BY pvd.sumber, pvd.rekhutangpiutang
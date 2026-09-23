SELECT
si.siid, si.sinotransaksi, si.sitgl,
si.sijmlbayar AS bayarutama, 
IFNULL(sum(sip.jumlah),0) as bayar
FROM `m5_si` si
left JOIN m5_si_pay sip ON si.siid = sip.idsi WHERE MONTH(sitgl) = 5

GROUP BY si.siid
HAVING bayarutama <> bayar
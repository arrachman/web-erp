SELECT
si.siid,
si.sitgl,
si.sinotransaksi,
ROUND(si.sitotal,2) as sitotal,
ROUND(sum(((sid.jml * sid.harga) - sid.jmldiskon)),2) as totaldetail
FROM
m5_si si
JOIN m5_si_detail sid ON si.siid = sid.idsi
GROUP BY si.siid
HAVING sitotal <> totaldetail
SELECT
ri.risupplier,
c.kkode,
c.knama,
ri.riid,
ri.rinotransaksi,
ri.ritgl,
ri.ritotaltransaksi
FROM
m4_ri ri
JOIN m1_contact c ON ri.risupplier = c.kid
WHERE risaldoawal = 1
AND ri.ritgl < '2015-06-01'
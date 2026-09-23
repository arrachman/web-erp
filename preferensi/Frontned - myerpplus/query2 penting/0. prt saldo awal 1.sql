SELECT
prt.prtsupplier,
c.kkode,
c.knama,
prt.prtid,
prt.prtnotransaksi,
prt.prttgl,
prt.prttotaltransaksi * -1
FROM
m4_prt prt
JOIN m1_contact c ON prt.prtsupplier = c.kid
WHERE prtsaldoawal = 1
AND prt.prttgl < '2015-06-01'
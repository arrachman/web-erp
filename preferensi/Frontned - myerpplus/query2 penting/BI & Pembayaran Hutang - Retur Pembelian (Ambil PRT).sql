SELECT
prt.prtsaldoawal,
prt.prtid,
prt.prttgl,
prt.prtnotransaksi,
prt.prtsupplier,
c.kkode,
c.knama,
prt.prthargatermasukpajak,
prt.prttotal * -1 as prttotal,
prt.prtjmldiskon * -1 as prtjmldiskon,
(CASE prt.prthargatermasukpajak
WHEN 0 THEN prt.prttotal * -1
ELSE (prt.prttotal - prt.prttotalpajak1detail) * -1
END) as dpp,
prt.prttotalpajak1detail * -1 as prttotalpajak1detail,
prt.prttotalpajak2detail * -1 as prttotalpajak2detail,
prt.prtbiayalain * -1 as prtbiayalain,
prt.prttotaltransaksi * -1 as prttotaltransaksi,
prt.prttermin,
prt.prttgljatuhtempo
FROM
m4_prt prt
JOIN m1_contact c ON prt.prtsupplier = c.kid
AND prt.prtstatus IN(2,3,4,7)
AND prt.prttgl BETWEEN '2015-06-01' AND '2015-06-31'
ORDER BY prt.prttgl, prt.prtnotransaksi
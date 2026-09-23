SELECT
ri.risaldoawal,
#ri.riid,
ri.ritgl,
ri.rinotransaksi,
#ri.risupplier,
c.kkode,
c.knama,
ri.rihargatermasukpajak,
ri.ritotal,
ri.rijmldiskon,
(CASE ri.rihargatermasukpajak
WHEN 0 THEN ri.ritotal
ELSE ri.ritotal - ri.ritotalpajak1detail
END) as dpp,
ri.ritotalpajak1detail,
#ri.ritotalpajak2detail,
ri.ribiayalain,
ri.ritotaltransaksi,
ri.ritermin,
ri.ritgljatuhtempo
FROM
m4_ri ri
JOIN m1_contact c ON ri.risupplier = c.kid
AND ri.ristatus IN(2,3,4,7)
AND ri.ritgl BETWEEN '2016-04-01' AND '2016-04-31'
AND ri.ricarabayar = 1
ORDER BY c.kkode, ri.ritgl, ri.rinotransaksi

SELECT sinoref, sinotransaksi, sitotaltransaksi  from m5_si WHERE
sitgl BETWEEN "2016-01-23" AND "2016-01-23"
and sistatus in(2,3,4,7)
AND silokasi = "SGT"
AND sicarabayar = 0
ORDER BY sinoref
SELECT silokasi, sitgl, sum(sitotaltransaksi) FROM m5_si WHERE sistatus IN(2,3,4,7)
GROUP BY silokasi, MONTH(sitgl)
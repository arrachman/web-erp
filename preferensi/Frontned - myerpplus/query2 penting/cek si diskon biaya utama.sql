SELECT silokasi, sitgl, SUM(sijmldiskon), sum(sibiayalain) FROM m5_si WHERE (sijmldiskon <> 0 OR sibiayalain <> 0) AND sistatus IN(2,3,4,7)
GROUP BY silokasi, MONTH(sitgl)
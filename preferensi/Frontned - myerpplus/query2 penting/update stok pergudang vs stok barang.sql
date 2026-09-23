UPDATE
m1_item i
JOIN
(SELECT
isw.idbarang,
SUM(isw.stok) as totalstok
FROM
m1_item_stock_warehouse isw
GROUP BY isw.idbarang
) as sp
ON i.bid = sp.idbarang
SET i.bstok = sp.totalstok
WHERE i.bstok <> sp.totalstok
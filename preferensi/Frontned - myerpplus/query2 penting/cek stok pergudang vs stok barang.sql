SELECT
i.bid,
i.bkode,
i.bnama,
i.bstok,
sp.totalstok
FROM
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
WHERE i.bstok <> sp.totalstok
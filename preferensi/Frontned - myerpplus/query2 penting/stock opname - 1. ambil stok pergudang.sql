SELECT
pi.pikategori,
i.bid,
i.bkode,
i.bnama,
i.bsatuan,
i.bhppaverage,
IFNULL(isw.stok,0) as stok,
i.bhppaverage * IFNULL(isw.stok,0) as nilai
FROM
m_12_pos_item pi
JOIN m1_item i 
ON pi.piidbarang = i.bid 
AND pi.pikategori = 'DC'
LEFT JOIN m1_item_stock_warehouse isw 
ON i.bid = isw.idbarang
AND isw.kgudang = 'DC'
ORDER BY i.bkode
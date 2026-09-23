SELECT pi.pikategori AS kategoripos, pi.piidbarang AS idbarang,
i.bkode AS kodebarang, i.bnama, IFNULL(isw.stok,0) as stok, i.bhppaverage AS HPP, isw.kgudang  
FROM m_12_pos_item pi
JOIN m1_item i ON pi.piidbarang = i.bid 
LEFT JOIN m1_item_stock_warehouse isw ON i.bid = isw.idbarang AND isw.kgudang = "GKB"
WHERE 
pi.pikategori = "GKB" AND i.bjenis = "P"
ORDER BY i.bkode ASC
select 
pi.piidbarang, 
i.bsatuan,
i.bnilaisatuan,
i.bsatuan,
i.bhargabeli,
pi.pihargajual1,
pi.pihargajual2,
pi.pihargajual3,
pi.pihargajual4,
pi.pihargajual5,
hg.hargajual,
pi.pistokminimal,
pi.pistokmaksimal, 
pi.pistokreorder,
pi.pistokminorder
FROM hargajualgkb hg 
left JOIN m1_item i ON hg.kodebarang = i.bkode
LEFT JOIN m_12_pos_item pi ON pi.piidbarang = i.bid
WHERE pi.pikategori = "GKB"
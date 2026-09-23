select 
i.bkode as kodebarang,
i.bnama as namabarang,
i.bsatuan as satuan,
i.bstok as stokglobal,
i.bhargabeli AS hargabeli,
pi.pihargajual1 AS hargajual,
pi.pistokminimal AS stokmin,
pi.pistokmaksimal AS stokmax,
pi.pistokminorder AS minorder,
i.bkp AS bkp,
i.bkl AS bkl,
i.bdivisi AS divisi,
i.bdepartemen AS departemen,
i.bkategori AS kategoribarang,
i.btag AS tag,
k.kkode AS kodesupplier,
k.knama AS namasupplier,
k.kpkp AS pkp from m_12_pos_item pi
JOIN m1_item i ON i.bid = pi.piidbarang
LEFT JOIN m1_item_supplier isp ON isp.isidbarang = pi.piidbarang
LEFT JOIN m1_contact k ON k.kid = isp.isidkontak
WHERE pi.pikategori = "GL"
ORDER BY i.bkode ASC
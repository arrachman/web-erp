SELECT i.bid, pl2.kode, pl2.kategori AS gudang,
0 AS idlokasi, 
pl2.lokasi,
pl2.lokasi AS namalokasi,
1 as inputuser,
"2015-07-09 00:00:00" AS inputtgl,
1 as modifikasiuser,
"2015-07-09 00:00:00" AS modifikasitgl

FROM 1_plano20150709_gkb pl2
LEFT JOIN m1_item i ON i.bkode = pl2.kode
#GROUP BY pl2.kode
#ORDER BY pl2.lokasi
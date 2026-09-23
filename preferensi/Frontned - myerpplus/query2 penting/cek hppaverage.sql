SELECT
i.bid,
i.bjenis,
i.bhpp,
i.bnama,
i.bstok,
SUM(it.jmlbarang) as jmlmasuk,
SUM(it.jmlbarang * it.hpp) as  nilaimasuk,
SUM(it.jmlbarang * it.hpp) / SUM(it.jmlbarang) as hppaverage,
i.bhppaverage,
abs((SUM(it.jmlbarang * it.hpp) / SUM(it.jmlbarang)) - i.bhppaverage) as selisih
FROM
m1_item_transaction it
JOIN m1_item i ON it.idbarang = i.bid AND it.jenismutasi = 1
JOIN m0_nomor n ON it.sumber = n.kodetabel AND n.transaksihpp = 1
GROUP BY i.bid

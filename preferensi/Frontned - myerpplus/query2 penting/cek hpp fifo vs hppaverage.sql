SELECT
SUM(cfi.cfijmlmasuk) as jmlmasuk,
SUM(cfi.cfijmlmasuk * cfi.cfiharga) as nilaimasuk,
ROUND(SUM(cfi.cfijmlmasuk * cfi.cfiharga) / SUM(cfi.cfijmlmasuk),2) as hppaverage,
i.bhppaverage,
(ROUND(SUM(cfi.cfijmlmasuk * cfi.cfiharga) / SUM(cfi.cfijmlmasuk),2)) - i.bhppaverage as selisih
FROM `m1_cogs_fifo_in` cfi
JOIN m1_item i ON cfi.cfiidbarang = i.bid
GROUP BY cfi.cfiidbarang
HAVING hppaverage <> bhppaverage
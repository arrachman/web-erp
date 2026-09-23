SELECT i.bkode, i.bnama, h.*
FROM `0_hppsk_201603fix` h
JOIN m1_item i ON h.id = i.bid AND i.bjenis = 'P'
WHERE `jml` < '0' 
AND ROUND(nilai) = 0
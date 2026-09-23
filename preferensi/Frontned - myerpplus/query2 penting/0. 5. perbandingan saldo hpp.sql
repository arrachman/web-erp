SELECT
sk.id,
i.bkode,
i.bnama,
IFNULL(sa.jml,0) as sajml,
IFNULL(sa.hpp,0) as sahpp,
IFNULL(sa.nilai,0) as sanilai, 
IFNULL(ms.jml,0) as msjml,
IFNULL(ms.hpp,0) as mshpp,
IFNULL(ms.nilai,0) as msnilai,
IFNULL(kl.jml,0) as kljml,
IFNULL(kl.hpp,0) as klhpp,
IFNULL(kl.nilai,0) as klnilai,
sk.jml as skjml,
sk.hpp as skhpp,
sk.nilai as sknilai,
IFNULL(sa.jml,0) + IFNULL(ms.jml,0) + IFNULL(kl.jml,0) as saldojml,
IFNULL(sa.hpp,0) + IFNULL(ms.hpp,0) + IFNULL(kl.hpp,0) as saldohpp,
IFNULL(sa.nilai,0) + IFNULL(ms.nilai,0) + IFNULL(kl.nilai,0) as saldonilai,
ROUND(sk.jml,2) - ROUND(IFNULL(sa.jml,0) + IFNULL(ms.jml,0) + IFNULL(kl.jml,0),2) seljml,
ROUND(sk.nilai,2) - ROUND(IFNULL(sa.nilai,0) + IFNULL(ms.nilai,0) + IFNULL(kl.nilai,0),2) as selnilai
FROM 0_hppsk sk
JOIN m1_item i ON sk.id = i.bid
LEFT JOIN 0_hppsa sa ON sk.id = sa.id
LEFT JOIN 0_hppms ms ON sk.id = ms.id
LEFT JOIN 0_hppkl kl ON sk.id = kl.id
WHERE 
ABS(ROUND(sk.nilai,2) - ROUND(IFNULL(sa.nilai,0) + IFNULL(ms.nilai,0) + IFNULL(kl.nilai,0),2)) > 1000
#ROUND(sk.jml,2) <> ROUND(IFNULL(sa.jml,0) + IFNULL(ms.jml,0) + IFNULL(kl.jml,0),2)
#OR
#ROUND(sk.nilai,2) <> ROUND(IFNULL(sa.nilai,0) + IFNULL(ms.nilai,0) + IFNULL(kl.nilai,0),2)
#OR
#ABS(ROUND(sk.nilai,2) - ROUND(IFNULL(sa.nilai,0) + IFNULL(ms.nilai,0) + IFNULL(kl.nilai,0),2)) > 1
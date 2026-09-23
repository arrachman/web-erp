SELECT
skj.id,
SUM(skj.jml) as stok,
IFNULL(skh.jml,0) as stokhpp
FROM
0_hppgudangsk_jml skj
LEFT JOIN 0_hppsk_201603fix skh ON skj.id = skh.id
GROUP BY skj.id
HAVING ROUND(stok) <> ROUND(stokhpp)
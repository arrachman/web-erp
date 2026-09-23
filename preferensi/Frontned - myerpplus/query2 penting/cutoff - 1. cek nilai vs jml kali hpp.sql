SELECT
id,
jml,
hpp,
nilai,
jml*hpp as nilaifix
FROM `0_hppsk_201603fix`
HAVING ROUND(nilai,2) <> ROUND(nilaifix,2)

SELECT tnotransaksi, COUNT(tid) as jmlrow FROM `m2_transaction_journal` 
WHERE (
`tnorek` = '110101.001' or
`tnorek` = '110101.017' or
`tnorek` = '110101.018' or
`tnorek` = 'null' or
`tnorek` = '110102.001' or
`tnorek` = '110102.001' or
`tnorek` = '110102.089' or
`tnorek` = '210201.025' or
`tnorek` = '520103.001' or
`tnorek` = '210201.014' or
`tnorek` = '210201.015' or
`tnorek` = '520102.009' or
`tnorek` = '210201.009' or
`tnorek` = '210201.010' or
`tnorek` = '520102.008' OR
`tnorek` = '410101.001' OR
`tnorek` = '110201.001'
)
AND `ttgl` >= '2015-08-01'
GROUP BY tsumber, tidtransaksi, tnorek, tdebit
having jmlrow <> 1
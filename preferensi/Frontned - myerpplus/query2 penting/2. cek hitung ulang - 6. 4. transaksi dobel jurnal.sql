SELECT * FROM
(
SELECT tnotransaksi, ttgl, COUNT(tid) as jmlrow FROM `m2_transaction_journal` 
WHERE `ttgl` >= '2016-04-01'
#AND tnorek = '210101.001'
GROUP BY tsumber, tidtransaksi, tnorek, tdebit, tkredit, turaian, tcatatan, turutan
having jmlrow <> 1
) as dobel
GROUP BY dobel.tnotransaksi

INSERT INTO m5_si_pay(
SELECT
0 as idsicarabayar,
si.siid as idsi,
sid.carabayar,
sid.matauang,
sid.kurs,
sid.jumlah,
sid.jumlahvalas,
sid.nogiro,
sid.tgljt,
sid.bank,
sid.noacbank,
sid.rekbank,
sid.rekgiro,
sid.catatan,
sid.urutan,
sid.isclose
FROM `m0_si_pay` sid
JOIN m5_si si
ON sid.idsi = si.sicustomint7
AND sid.rekgiro = si.sinotransaksi
)
UPDATE
m0_si si
JOIN `m0_si_pay` sid ON si.siid = sid.idsi
AND CONCAT(si.sicabang,"-",si.silokasi) = sid.catatan
SET sid.rekgiro = CONCAT(si.sinotransaksi,"-T")
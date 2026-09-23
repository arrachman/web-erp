SELECT
sa.sanotransaksi,
sad.idbarang,
i.bkode,
i.bnama,
sad.hpplama,
sad.hpp,
i.bhppaverage,
sad.hpp - i.bhppaverage
FROM
m3_sa sa
JOIN m3_sa_detail sad ON sa.said = sad.idsa
JOIN m1_item i ON sad.idbarang = i.bid
WHERE sa.sanotransaksi = 'ADCSA15040043'

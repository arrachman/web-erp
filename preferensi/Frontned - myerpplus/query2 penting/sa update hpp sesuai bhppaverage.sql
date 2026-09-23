UPDATE
m3_sa sa
JOIN m3_sa_detail sad ON sa.said = sad.idsa
JOIN m1_item i ON sad.idbarang = i.bid
SET sad.hpplama = i.bhppaverage, sad.hpp = i.bhppaverage
WHERE sa.sanotransaksi = 'ADCSA15040043'

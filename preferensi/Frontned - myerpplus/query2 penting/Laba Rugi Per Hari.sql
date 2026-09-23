SELECT 

b.bnama AS sicabang, l.lnama AS silokasi , 

"c06ffdbccab2fee0b416ceb44ff29768" As idlogin, 
lr.sumber, 
lr.notransaksi, 
tgl AS sitgl, 
lr.namacustomer AS namacustomer, 
SUM(lr.nilaipenjualan) AS totaltransaksi, SUM(lr.hargapokok) AS hargapokok  , 
(lr.nilaipenjualan-lr.hargapokok) AS labarugi, 

(CASE IFNULL(lr.hargapokok,0)  
WHEN 0 THEN IFNULL(lr.nilaipenjualan - lr.hargapokok,0)  
ELSE (CASE lr.sumber 
WHEN "SI" THEN IFNULL(((lr.nilaipenjualan - lr.hargapokok) / lr.hargapokok) * (100),0) 
ELSE IFNULL(((lr.nilaipenjualan - lr.hargapokok) / lr.hargapokok) * (-100),0) 
END) 
END) AS margin, 
"5771ebae8ebd7bba72334683abe1dbc0" AS idmsmq, 
lr.cabang AS lrcustomtext1, 
lr.lokasi AS lrcustomtext2,  
"" AS lrcustomtext3, 
"" AS lrcustomtext4,
"" AS lrcustomtext5, 
0 AS lrcustomint1, 
0 AS lrcustomint2, 
0 AS lrcustomint3, 
0 AS lrcustomint4, 
0 AS lrcustomint5, 
0 AS lrcustomdbl1, 
0 AS lrcustomdbl2,  
0 AS lrcustomdbl3, 
0 AS lrcustomdbl4, 
0 AS lrcustomdbl5, 
"1900-01-01" AS lrcustomdate1, 
"1900-01-01" AS lrcustomdate2, 
"1900-01-01" AS lrcustomdate3, 
"1900-01-01" AS lrcustomdate4, 
"1900-01-01" AS lrcustomdate5 

FROM (  

(SELECT si.sicabang AS cabang , 
si.silokasi AS lokasi , 
si.sisumber AS sumber, 
si.sinotransaksi AS notransaksi, 
si.sitgl AS tgl, 
si.sicustomer AS idcustomer,  
kc.knama as namacustomer, 
si.sibagianpenjualan AS idsalesman, 

SUM(CASE sid.customdbl2  	
WHEN 0 THEN (((sid.jml * sid.harga) * sid.kurs) - (sid.jmldiskon * sid.kurs)) 	 
WHEN 1 THEN (((((sid.jml * sid.harga) * sid.kurs) - (sid.jmldiskon * sid.kurs)) / 11) * 10) 
END) AS nilaipenjualan,  

SUM(sid.jml * sid.hpp) AS hargapokok 

FROM m5_si_detail sid 
JOIN m5_si si ON si.siid = sid.idsi 
JOIN m1_item i ON i.bid = sid.idbarang  
JOIN m1_contact kc ON si.sicustomer = kc.kid 
JOIN m1_contact ks ON si.sibagianpenjualan = ks.kid  

WHERE si.sistatus IN (2,3,4,7)  
AND (si.sitgl BETWEEN "2015-12-01" AND "2015-12-31")  
AND (si.silokasi BETWEEN "TBN" AND "TBN" )   
#AND si.sitgl = "2015-12-01" 
GROUP BY si.siid  )  

UNION ALL 

(SELECT sr.srcabang AS cabang , 
sr.srlokasi AS lokasi , 
sr.srsumber AS sumber, 
sr.srnotransaksi AS notransaksi, 
sr.srtgl AS tgl,  
sr.srcustomer AS idcustomer, 
kc.knama as namacustomer, 
sr.srbagianpenjualan AS idsalesman,  
CASE i.bkp 
WHEN 0 THEN SUM(((srd.jml * srd.harga) * srd.kurs) - (srd.jmldiskon * srd.kurs)) 	
WHEN 1 THEN SUM(((((srd.jml * srd.harga) * srd.kurs) - (srd.jmldiskon * srd.kurs)) / 11) * 10)  
END AS nilaipenjualan, 
SUM(srd.jml * srd.hpp) AS hargapokok 

FROM m5_sr_detail srd 
JOIN m5_sr sr ON sr.srid = srd.idsr 
JOIN m1_item i ON i.bid = srd.idbarang 
JOIN m1_contact kc ON sr.srcustomer = kc.kid  
JOIN m1_contact ks ON sr.srbagianpenjualan = ks.kid  

WHERE sr.srstatus IN (2,3,4,7)  
AND (sr.srtgl BETWEEN "2015-12-01" AND "2015-12-31")  
AND (sr.srlokasi BETWEEN "TBN" AND "TBN" )  
#AND sr.srtgl = "2015-12-01" 
GROUP BY sr.srid  ) ) AS lr    

JOIN  m1_branch b ON lr.cabang = b.bkode 
JOIN  m1_location l ON lr.lokasi = l.lkode AND lr.cabang = l.lcabang 

GROUP BY lr.cabang ASC , lr.lokasi ASC , tgl ASC  
ORDER BY lr.cabang ASC , lr.lokasi ASC , tgl ASC  
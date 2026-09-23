SELECT prt.prttgl, prt.prtnotransaksi, mj.mjidtransaksi, mj.mjid, mj.mjtglantrian, prt.prtid  FROM `m0_msmq_journal` mj
JOIN m4_prt prt ON mj.mjidtransaksi = prt.prtid
WHERE mj.mjsumber = "PRT" AND prt.prttgl BETWEEN "2015-08-01" AND "2015-08-31"
AND (mj.mjprogress = 0 OR mj.mjprogress = 3);
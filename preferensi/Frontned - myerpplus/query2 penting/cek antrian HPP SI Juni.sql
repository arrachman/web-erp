SELECT
mc.*, si.sitgl
FROM
m0_msmq_cogs mc
JOIN m5_si si
ON mc.mcidtransaksi = si.siid
AND mc.mcsumber = si.sisumber
AND si.sitgl BETWEEN "2015-06-01" AND "2015-06-31"
AND (mc.mcprogress=0 OR mc.mcprogress=3)
#GROUP BY mc.mcsumber, mc.mcidtransaksi
#ORDER BY mc.mcpesan
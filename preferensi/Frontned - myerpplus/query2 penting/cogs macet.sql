SELECT cogs.* from m0_msmq_cogs cogs JOIN m5_si si on cogs.mcsumber = si.sisumber AND cogs.mcidtransaksi = si.siid and si.sistatus in (2,3,4,7) 
AND (cogs.mcprogress = 0 OR cogs.mcprogress = 3)
AND si.sitgl <= "2015-05-31" 
ORDER BY mctglantrian ASC
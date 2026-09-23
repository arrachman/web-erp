	SELECT
		SUM((IFNULL(skj.jml, 0) * IFNULL(skh.hpp, 0))) as saldonilai
	FROM
	0_hppgudangsk_jml skj
	LEFT JOIN m1_warehouse w ON skj.gudang = w.wkode AND skj.jml <> 0
	JOIN m1_item i ON skj.id = i.bid
	LEFT JOIN m1_item_category ic ON i.bkategori = ic.ickode
	LEFT JOIN 0_hppgudangsk skh ON skj.id = skh.id
	ORDER BY skj.gudang ASC, i.bkategori ASC, i.bkode ASC
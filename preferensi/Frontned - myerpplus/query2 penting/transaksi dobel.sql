select count(siid) as jmlrow, sinoref from m5_si 
where sitgl between "2016-01-01" and "2016-01-31"
AND sistatus IN (2,3,4,7) AND sinoref <> ""
group by sinoref
having jmlrow > 1
-- Normalize duplicated sort_order values for existing sidebar groups.

UPDATE "m0_menu"
SET
  "sort_order" = CASE "key"
    WHEN 'master-data-contact' THEN 1
    WHEN 'master-data-division' THEN 2
    WHEN 'master-data-customer' THEN 3
    WHEN 'master-data-supplier' THEN 4
    WHEN 'master-data-company' THEN 5
    WHEN 'master-data-item' THEN 6
    WHEN 'master-data-item-stock' THEN 7
    WHEN 'master-data-province' THEN 8
    WHEN 'master-data-city' THEN 9
    WHEN 'master-data-city-sla' THEN 10
    WHEN 'master-data-uom' THEN 11
    WHEN 'master-data-warehouse' THEN 12
    ELSE "sort_order"
  END,
  "updated_at" = NOW()
WHERE "deleted_at" IS NULL
  AND "key" IN (
    'master-data-contact',
    'master-data-division',
    'master-data-customer',
    'master-data-supplier',
    'master-data-company',
    'master-data-item',
    'master-data-item-stock',
    'master-data-province',
    'master-data-city',
    'master-data-city-sla',
    'master-data-uom',
    'master-data-warehouse'
  );

UPDATE "m0_menu"
SET
  "sort_order" = CASE "key"
    WHEN 'logistic-transaction' THEN 1
    WHEN 'logistic-inbound' THEN 2
    WHEN 'logistic-outbound' THEN 3
    WHEN 'logistic-report-monitoring-do' THEN 4
    WHEN 'logistic-report-stock-batch' THEN 5
    WHEN 'logistic-report-stock-mutation' THEN 6
    ELSE "sort_order"
  END,
  "updated_at" = NOW()
WHERE "deleted_at" IS NULL
  AND "key" IN (
    'logistic-transaction',
    'logistic-inbound',
    'logistic-outbound',
    'logistic-report-monitoring-do',
    'logistic-report-stock-batch',
    'logistic-report-stock-mutation'
  );

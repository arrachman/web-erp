-- MVP navigation scope (Master Data): hide Master Data menus outside the
-- Bahtera Madani MVP (Fase 1: A1, D1, D2, D3, E1) from the live ERP sidebar.
-- Hidden: the whole Production master group (Fase 2), legacy item attributes
-- (Class, Color, Commission, Item Information, Material, Model, Nozzle, OEM,
-- Price Index, Product Class, Section), non-MVP references (Miscellaneous,
-- Point Categories, Production Categories, Transaction Notes + Detail,
-- Work Estimate), and Project.
-- Deactivated, not deleted, so they can be re-enabled when their phase starts.
-- Data, endpoints, and role mappings are untouched; only `is_active` flips,
-- which the `getMyMenus` tree builder already filters out.
-- See docs/bahtera-madani-mvp-scope.md § "Navigasi deployment MVP".

UPDATE "sys_menus"
SET "is_active" = false,
    "updated_at" = NOW()
WHERE "code" IN (
    'M1.PROD',
    'M1.PROD.ACTIVITY',
    'M1.PROD.DESIGNER',
    'M1.PROD.LABOR',
    'M1.PROD.MACHINE',
    'M1.PROD.ROUTE',
    'M1.PROD.SUBCLASS',
    'M1.ITEM.CLASS',
    'M1.ITEM.COLOR',
    'M1.ITEM.COMMISSION',
    'M1.ITEM.INFO',
    'M1.ITEM.MATERIAL',
    'M1.ITEM.MODEL',
    'M1.ITEM.NOZZLE',
    'M1.ITEM.OEM',
    'M1.ITEM.PRICE-INDEX',
    'M1.ITEM.PRODUCT-CLASS',
    'M1.ITEM.SECTION',
    'M1.REF.MISC',
    'M1.REF.POINT-CAT',
    'M1.REF.PRODUCTION-CAT',
    'M1.REF.TXN-NOTE',
    'M1.REF.TXN-NOTE-DETAIL',
    'M1.REF.WORK-ESTIMATE',
    'M1.ORG.PROJECT'
  )
  AND "deleted_at" IS NULL
  AND "is_active" = true;

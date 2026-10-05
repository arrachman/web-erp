/**
 * Dataset-builder configs for Master Data reports (Wave G1).
 *
 * One generic SQL builder (erp-md-reports.service) executes these
 * configs: `select` maps legacy .mrt field names → SQL expressions over
 * md_* tables. ONLY config constants appear in SQL text; runtime param
 * values are always bound parameters. Fields a template declares but a
 * config does not map are selected as NULL by the builder, so every
 * template binding resolves. `empty: true` = no ERP equivalent exists —
 * the report renders header-only and stays CONVERTED (never VERIFIED
 * with fake data).
 */

export interface MdParamFilter {
  /** SQL fragment with one or more `?` placeholders, all bound to the same value. */
  sql: string;
  kind: 'text' | 'number' | 'date';
}

export interface MdDatasetConfig {
  from?: string;
  select: Record<string, string>;
  where?: string;
  orderBy?: string;
  /** Main alias whose deleted_at must be NULL. */
  deletedAlias?: string;
  paramFilters?: Record<string, MdParamFilter>;
  empty?: boolean;
  note?: string;
}

export interface MdReportConfig {
  datasets: Record<string, MdDatasetConfig>;
}

/** Derived stock balance per item (POSTED movements signed + opening), mirroring erp-inv-reports. */
export const ITEM_STOCK_SQL = `(
  SELECT COALESCE(SUM(x.qty), 0) FROM (
    SELECT CASE m.movement_type
      WHEN 'TRANSFER_RECEIPT' THEN l.base_quantity
      WHEN 'RETURN' THEN l.base_quantity
      WHEN 'ISSUE' THEN -l.base_quantity
      WHEN 'TRANSFER' THEN -l.base_quantity
      ELSE 0 END AS qty
    FROM inv_stock_movement_lines l
    JOIN inv_stock_movements m ON m.id = l.stock_movement_id
    WHERE m.status = 'POSTED' AND m.deleted_at IS NULL AND l.item_id = i.id
    UNION ALL
    SELECT ol.quantity
    FROM inv_opening_stock_lines ol
    JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
    WHERE o.status = 'POSTED' AND o.deleted_at IS NULL AND ol.item_id = i.id
  ) x
)`;

export const ITEM_FROM = `
  md_items i
  LEFT JOIN md_item_categories ic ON ic.id = i.category_id
  LEFT JOIN md_units u ON u.id = i.base_unit_id
  LEFT JOIN md_partners sup ON sup.id = i.primary_supplier_id
  LEFT JOIN md_warehouses dw ON dw.id = i.default_warehouse_id
  LEFT JOIN md_item_types k ON k.id = i.kind_id
`;

const priceLevel = (level: number) =>
  `(SELECT p.price FROM md_item_prices p WHERE p.item_id = i.id AND p.level = ${level})`;

export const ITEM_SELECT: Record<string, string> = {
  kodebarang: 'i.code',
  bkode: 'i.code',
  namabarang: 'i.name',
  bnama: 'i.name',
  burutan: 'i.id',
  btipe: 'i.type::text',
  jenis: 'k.name',
  kategoribarang: 'ic.name',
  namakategoribarang: 'ic.name',
  bkategori: 'ic.name',
  icnama: 'ic.name',
  satuanbarang: 'u.name',
  bsatuan: 'u.name',
  satuan: 'u.code',
  gudang: 'dw.code',
  bgudang: 'dw.code',
  blggudang: 'dw.code',
  blokasi: 'dw.code',
  wnama: 'dw.name',
  blgnamalokasi: 'dw.name',
  bstok: ITEM_STOCK_SQL,
  stok: ITEM_STOCK_SQL,
  stokgudang: ITEM_STOCK_SQL,
  stokminimal: 'i.min_stock',
  stokmaksimal: 'i.max_stock',
  jumlahorder: 'i.reorder_qty',
  jmlorderbeli: 'i.min_order_qty',
  hargabeli: 'i.purchase_price',
  hargajual: 'i.sale_price',
  hargajual1: 'i.sale_price',
  bhargajual1: 'i.sale_price',
  hrgajual1: 'i.sale_price',
  hargajual2: priceLevel(2),
  bhargajual2: priceLevel(2),
  hargajual3: priceLevel(3),
  bhargajual3: priceLevel(3),
  hargajual4: priceLevel(4),
  bhargajual4: priceLevel(4),
  hargajual5: priceLevel(5),
  bhargajual5: priceLevel(5),
  diskonjual: `(SELECT p.discount_percent FROM md_item_prices p WHERE p.item_id = i.id AND p.level = 1)`,
  hpp: 'i.average_cost',
  hppaverage: 'i.average_cost',
  namasuplier: 'sup.name',
  status: `CASE WHEN i.is_active THEN 'Aktif' ELSE 'Nonaktif' END`,
  idbarang: 'i.id',
};

export const ITEM_PARAM_FILTERS: Record<string, MdParamFilter> = {
  warehouse: { sql: 'i.default_warehouse_id = ?', kind: 'number' },
  branch: { sql: 'i.branch_id = ?', kind: 'number' },
};

/** Simple code/name(/notes) master list. */
export function simpleMaster(
  table: string,
  prefix: string,
  opts: { notes?: string | null; extra?: Record<string, string>; where?: string; orderBy?: string } = {},
): MdDatasetConfig {
  const select: Record<string, string> = {
    [`${prefix}kode`]: 't.code',
    [`${prefix}nama`]: 't.name',
    ...opts.extra,
  };
  if (opts.notes !== null) select[`${prefix}catatan`] = opts.notes ? `t.${opts.notes}` : 'NULL';
  return {
    from: `${table} t`,
    select,
    where: opts.where,
    orderBy: opts.orderBy ?? 't.code',
    deletedAlias: 't',
  };
}

export function emptyConfig(note: string): MdDatasetConfig {
  return { select: {}, empty: true, note };
}

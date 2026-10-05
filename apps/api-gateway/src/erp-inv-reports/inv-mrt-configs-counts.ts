/**
 * Stock-count (opname) configs (Wave G4): lists, document details,
 * blanko count forms, results and variance reports — all read
 * inv_stock_counts + inv_stock_count_lines (system/physical/good/
 * damaged/variance quantities are stored on the count lines).
 * Lot balances for the berita acara form are derived from POSTED
 * movement lines carrying a lot (the T1 lot convention).
 */

import {
  AVG_COST,
  LINE_WH,
  SIGNED_QTY,
  docNoFilter,
  periodFilters,
  statusLabel,
  warehouseFilter,
  type InvDatasetConfig,
  type InvReportConfig,
} from './inv-mrt-configs';

const SP_FROM = `
  inv_stock_counts c
  JOIN inv_stock_count_lines cl ON cl.stock_count_id = c.id
  JOIN md_items i ON i.id = cl.item_id
  LEFT JOIN md_units u ON u.id = cl.base_unit_id
  LEFT JOIN md_warehouses w ON w.id = c.warehouse_id
  LEFT JOIN inv_bins b ON b.id = cl.bin_id
`;

const LOCATION_FILTER = {
  location: {
    sql: `(b.code = ? OR b.name ILIKE '%' || ? || '%' OR c.location_id = CASE WHEN ? ~ '^[0-9]+$' THEN ?::bigint ELSE -1 END)`,
    kind: 'text' as const,
  },
};

function spConfig(): InvDatasetConfig {
  return {
    from: SP_FROM,
    select: {
      spid: 'c.id',
      idsp: 'c.id',
      idspdetail: 'cl.id',
      spnotransaksi: 'c.doc_number',
      sptgl: 'c.count_date',
      spgudang: 'c.warehouse_id',
      gudang: 'c.warehouse_id',
      gudangnama: 'w.name',
      spuraian: 'c.description',
      spstatus: statusLabel('c.status'),
      lokasibarang: 'b.code',
      ilnama: 'b.name',
      lokasi: 'b.code',
      idbarang: 'i.id',
      bkode: 'i.code',
      namabarang: 'i.name',
      satuan: 'u.name',
      jmlsistem: 'cl.system_qty',
      jmlbarangsistem: 'cl.system_qty',
      saldoawal: 'cl.system_qty',
      jmlfisik: 'cl.physical_qty',
      jmlbarangfisik: 'cl.physical_qty',
      jmlbagus: 'cl.good_qty',
      jmlbarangbagus: 'cl.good_qty',
      jmlrusak: 'cl.damaged_qty',
      jmlbarangrusak: 'cl.damaged_qty',
      jmlselisih: 'cl.variance_qty',
      selisih: 'cl.variance_qty',
      selisihbarangfix: 'cl.variance_qty',
      jmlbarangjual: 'NULL',
      hargajual: 'i.sale_price',
      harga: 'i.sale_price',
      bhppaverage: AVG_COST,
      total: `cl.variance_qty * ${AVG_COST}`,
      catatan: 'cl.notes',
      tgl: 'c.count_date',
    },
    where: 'c.deleted_at IS NULL',
    paramFilters: {
      ...periodFilters('c.count_date'),
      ...docNoFilter('c.doc_number'),
      ...warehouseFilter('w.id', 'w.code', 'w.name'),
      ...LOCATION_FILTER,
    },
    orderBy: 'c.count_date, c.doc_number, cl.line_no',
  };
}

/** Variance totals per item (selisih reports with valuation). */
function spVarianceConfig(): InvDatasetConfig {
  const cfg = spConfig();
  cfg.select = {
    spgudang: 'c.warehouse_id',
    gudang: 'c.warehouse_id',
    bkode: 'i.code',
    namabarang: 'i.name',
    satuan: 'u.name',
    jmlsistem: 'SUM(cl.system_qty)',
    jmlselisih: 'SUM(cl.variance_qty)',
    selisih: 'SUM(cl.variance_qty)',
    bhppaverage: AVG_COST,
    total: `SUM(cl.variance_qty) * ${AVG_COST}`,
  };
  cfg.groupBy = 'c.warehouse_id, i.id, i.code, i.name, u.name, i.average_cost, i.last_hpp, i.purchase_price';
  cfg.orderBy = 'i.code';
  return cfg;
}

const SP_OUTSTANDING: InvDatasetConfig = (() => {
  const cfg = spConfig();
  cfg.select = {
    ...cfg.select,
    jmlsa: `(
      SELECT COALESCE(SUM(al.base_quantity), 0)
      FROM inv_stock_adjustment_lines al
      JOIN inv_stock_adjustments a ON a.id = al.stock_adjustment_id
      WHERE a.deleted_at IS NULL AND al.count_line_id = cl.id
    )`,
    jmlselisih: 'cl.variance_qty',
    sisa: `ABS(cl.variance_qty) - (
      SELECT COALESCE(SUM(al.base_quantity), 0)
      FROM inv_stock_adjustment_lines al
      JOIN inv_stock_adjustments a ON a.id = al.stock_adjustment_id
      WHERE a.deleted_at IS NULL AND al.count_line_id = cl.id
    )`,
  };
  cfg.where = "c.deleted_at IS NULL AND c.adjustment_status <> 'POSTED'";
  return cfg;
})();

const SP_STEP_DS2: InvDatasetConfig = {
  from: SP_FROM,
  select: {
    spidprogress: 'NULL',
    spid: 'c.id',
    spstepke: 'c.step_no',
    idspdetail: 'cl.id',
    idbarang: 'i.id',
    bkode: 'i.code',
    namabarang: 'i.name',
    jmlsistem: 'cl.system_qty',
    jmlfisik: 'cl.physical_qty',
    jmlbagus: 'cl.good_qty',
    jmlrusak: 'cl.damaged_qty',
    selisih: 'cl.variance_qty',
  },
  where: 'c.deleted_at IS NULL',
  paramFilters: {
    ...periodFilters('c.count_date'),
    ...warehouseFilter('w.id', 'w.code', 'w.name'),
    ...LOCATION_FILTER,
  },
  orderBy: 'c.count_date, c.doc_number, cl.line_no',
};

/** Berita acara: remaining qty per lot in a warehouse (derived from lot lines). */
const BERITA_ACARA: InvDatasetConfig = {
  from: `
    (
      SELECT lot.id AS lot_id, l.item_id AS item_id, ${LINE_WH} AS wh_id,
        SUM(${SIGNED_QTY}) AS sisa
      FROM inv_stock_movement_lines l
      JOIN inv_stock_movements m ON m.id = l.stock_movement_id
      JOIN inv_lots lot ON lot.id = l.lot_id
      WHERE m.status = 'POSTED' AND m.deleted_at IS NULL
      GROUP BY lot.id, l.item_id, ${LINE_WH}
    ) lb
    JOIN inv_lots lot ON lot.id = lb.lot_id
    JOIN md_items i ON i.id = lb.item_id
    LEFT JOIN md_warehouses w ON w.id = lb.wh_id
  `,
  select: {
    wkode: 'w.code',
    wnama: 'w.name',
    idbarang: 'i.id',
    bkode: 'i.code',
    bnama: 'i.name',
    nbikode: 'lot.lot_number',
    nbicustomdate1: 'lot.expiry_date',
    stok: 'lb.sisa',
    nbijmlsisa: 'lb.sisa',
  },
  paramFilters: { ...warehouseFilter('w.id', 'w.code', 'w.name') },
  orderBy: 'w.name, i.code, lot.lot_number',
};

export const INV_COUNT_CONFIGS: Record<string, InvReportConfig> = {
  'inv.liststockopname': { datasets: { DS1: spConfig() } },
  'inv.liststockopname2': { datasets: { DS1: spConfig() } },
  'inv.liststockopnameproduct': { datasets: { DS1: spConfig() } },
  'inv.liststockopnamedenganharga': { datasets: { DS1: spConfig() } },
  'inv.liststockopnamedenganharga2': { datasets: { DS1: spConfig() } },
  'inv.liststockopnametanpaharga': { datasets: { DS1: spConfig() } },
  'inv.liststockopnamestep': { datasets: { DS1: spConfig(), DS2: SP_STEP_DS2 } },
  'inv.liststockopnameoutstanding': { datasets: { DS1: SP_OUTSTANDING } },
  'inv.stockopnamedetail': { datasets: { DS1: spConfig() } },
  'inv.stockopnamedetail2': { datasets: { DS1: spConfig() } },
  'inv.stockopnamedetailblk': { datasets: { DS1: spConfig() } },
  'inv.stockopnamedetailjual': { datasets: { DS1: spConfig() } },
  'inv.stockopnamedetailselisih': { datasets: { DS1: spConfig() } },
  'inv.stockopnamedetailselisihblanko': { datasets: { DS1: spConfig() } },
  'inv.sp1': { datasets: { DS1: spConfig() } },
  'inv.hasilstokopname': { datasets: { DS1: spConfig() } },
  'inv.blankostokopname': { datasets: { DS1: spConfig() } },
  'inv.blankostokopnamedengansaldo': { datasets: { DS1: spConfig() } },
  'inv.blankostokopnametanpasaldo': { datasets: { DS1: spConfig() } },
  'inv.blankostokopanameselisihdengansaldo': { datasets: { DS1: spConfig() } },
  'inv.blankostokopanameselisihtanpasaldo': { datasets: { DS1: spConfig() } },
  'inv.blankostokopanameselisihdengansaldojual': { datasets: { DS1: spConfig() } },
  'inv.tessssss': { datasets: { DS1: spConfig() } },
  'inv.stockopnamedetailselisihdgnharga': { datasets: { DS1: spVarianceConfig() } },
  'inv.stockopnamedetailselisihdgnharga2': { datasets: { DS1: spVarianceConfig() } },
  'inv.laporantotalselisihsp': { datasets: { DS1: spVarianceConfig() } },
  'inv.beritaacarastokopnamesin': { datasets: { DS1: BERITA_ACARA } },
};

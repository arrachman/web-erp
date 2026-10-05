/**
 * Document configs (Wave G4): opening balances (IB), stock adjustments
 * (SA), price/cost adjustments (PA ← inv_cost_recalculations), daily
 * checks / timesheets (DC), and the movement-document families
 * material requisition (REQUEST), transfer (TRANSFER) and receipt
 * (TRANSFER_RECEIPT) — all documents live in inv_stock_movements with
 * the type as discriminator; related documents link via
 * related_movement_id / related_line_id.
 */

import {
  AVG_COST,
  balanceExpr,
  docNoFilter,
  periodFilters,
  statusLabel,
  warehouseFilter,
  type InvDatasetConfig,
  type InvReportConfig,
} from './inv-mrt-configs';

const MOVEMENT_FROM = `
  inv_stock_movements m
  JOIN inv_stock_movement_lines l ON l.stock_movement_id = m.id
  JOIN md_items i ON i.id = l.item_id
  LEFT JOIN md_units u ON u.id = l.base_unit_id
  LEFT JOIN md_warehouses ws ON ws.id = m.source_warehouse_id
  LEFT JOIN md_warehouses wt ON wt.id = m.transit_warehouse_id
  LEFT JOIN md_warehouses wd ON wd.id = m.destination_warehouse_id
  LEFT JOIN inv_stock_movements rel ON rel.id = m.related_movement_id
`;

const REALISASI_LINE = `(
  SELECT COALESCE(SUM(rl.base_quantity), 0)
  FROM inv_stock_movement_lines rl
  WHERE rl.related_line_id = l.id
)`;

const REALISASI_DOC_ITEM = `(
  SELECT COALESCE(SUM(rl.base_quantity), 0)
  FROM inv_stock_movement_lines rl
  JOIN inv_stock_movements rm2 ON rm2.id = rl.stock_movement_id
  WHERE rm2.deleted_at IS NULL AND rm2.status = 'POSTED'
    AND rm2.related_movement_id = m.id AND rl.item_id = l.item_id
)`;

function movementDocConfig(
  prefix: 'mr' | 'ts' | 'rs',
  movementType: string,
): InvDatasetConfig {
  const p = prefix;
  return {
    from: MOVEMENT_FROM,
    select: {
      [`${p}id`]: 'm.id',
      [`${p}notransaksi`]: 'm.doc_number',
      [`${p}tgl`]: 'm.movement_date',
      [`${p}gudangasal`]: 'm.source_warehouse_id',
      [`${p}gudangtransit`]: 'm.transit_warehouse_id',
      [`${p}gudangtujuan`]: 'm.destination_warehouse_id',
      [`${p}uraian`]: 'm.description',
      [`${p}status`]: statusLabel('m.status'),
      // MR-only header fields (harmless for ts/rs: not declared there)
      mrtgldipakai: 'm.needed_date',
      mrdimintaoleh: 'm.requested_to',
      mrmintake: 'm.location_id',
      bkode: 'i.code',
      bid: 'i.id',
      idbarang: 'i.id',
      kodebarang: 'i.code',
      namabarang: 'i.name',
      jml: 'l.base_quantity',
      satuan: 'u.name',
      catatan: 'l.notes',
      // Related-doc columns: on an RS print, tsnotransaksi is the SOURCE
      // transfer's number; everywhere else mrnotransaksi is the related MR.
      // (Never override the doc's OWN <prefix>notransaksi set above.)
      ...(p === 'rs' ? { tsnotransaksi: 'rel.doc_number' } : {}),
      ...(p !== 'mr' ? { mrnotransaksi: 'rel.doc_number' } : {}),
      harga: `COALESCE(l.unit_cost, ${AVG_COST})`,
      total: `l.base_quantity * COALESCE(l.unit_cost, ${AVG_COST})`,
      jmlrealisasi: `GREATEST(${REALISASI_LINE}, ${REALISASI_DOC_ITEM})`,
      jmlsisa: `l.base_quantity - GREATEST(${REALISASI_LINE}, ${REALISASI_DOC_ITEM})`,
      stok: balanceExpr('i.id', 'm.source_warehouse_id'),
      sisa: `l.base_quantity - GREATEST(${REALISASI_LINE}, ${REALISASI_DOC_ITEM})`,
      'COUNT_x0028__x002A__x0029_': 'COUNT(*) OVER ()',
    },
    where: `m.deleted_at IS NULL AND m.movement_type = '${movementType}'`,
    paramFilters: {
      ...periodFilters('m.movement_date'),
      ...docNoFilter('m.doc_number'),
      warehouse: {
        sql: `(m.source_warehouse_id = CASE WHEN ? ~ '^[0-9]+$' THEN ?::bigint ELSE -1 END
          OR m.destination_warehouse_id = CASE WHEN ? ~ '^[0-9]+$' THEN ?::bigint ELSE -1 END
          OR ws.code = ? OR wd.code = ? OR ws.name ILIKE '%' || ? || '%' OR wd.name ILIKE '%' || ? || '%')`,
        kind: 'text',
      },
    },
    orderBy: 'm.movement_date, m.doc_number, l.line_no',
  };
}

/** DS2 of batch/serial document variants: only lines carrying a lot/serial. */
function movementDocDs2(
  prefix: 'ts' | 'rs',
  movementType: string,
  kind: 'batch' | 'serial',
): InvDatasetConfig {
  const base = movementDocConfig(prefix, movementType);
  const isBatch = kind === 'batch';
  return {
    ...base,
    from: `
      ${MOVEMENT_FROM}
      ${isBatch ? 'JOIN inv_lots lot ON lot.id = l.lot_id' : 'JOIN inv_serials ser ON ser.id = l.serial_id'}
    `,
    select: {
      ...base.select,
      nbtidbarang: 'l.item_id',
      nstidbarang: 'l.item_id',
      nbtkode: isBatch ? 'lot.lot_number' : 'NULL',
      nstkode: isBatch ? 'NULL' : 'ser.serial_number',
      nbtjml: 'l.base_quantity',
      nbtsatuan: 'u.name',
    },
  };
}

const SA_FROM = `
  inv_stock_adjustments a
  JOIN inv_stock_adjustment_lines al ON al.stock_adjustment_id = a.id
  JOIN md_items i ON i.id = al.item_id
  LEFT JOIN md_units u ON u.id = al.base_unit_id
  LEFT JOIN md_warehouses w ON w.id = a.warehouse_id
  LEFT JOIN inv_stock_counts sc ON sc.id = a.stock_count_id
`;

const SA_SELECT: Record<string, string> = {
  said: 'a.id',
  sagudang: 'a.warehouse_id',
  gudang: 'w.name',
  sanotransaksi: 'a.doc_number',
  satgl: 'a.adjustment_date',
  sabagiansa: 'NULL',
  sajenis: 'a.kind',
  sauraian: 'a.description',
  sastatus: statusLabel('a.status'),
  sacatatan: 'a.notes',
  spnotransaksi: 'sc.doc_number',
  bkode: 'i.code',
  bid: 'i.id',
  idbarang: 'i.id',
  namabarang: 'i.name',
  satuan: 'u.name',
  satuanbarang: 'u.name',
  jmlmasuk: "CASE WHEN al.direction = 'INCREASE' THEN al.base_quantity ELSE 0 END",
  jmlkeluar: "CASE WHEN al.direction = 'DECREASE' THEN al.base_quantity ELSE 0 END",
  hpplama: 'NULL',
  hpp: 'al.unit_cost',
  totalmasuk: "CASE WHEN al.direction = 'INCREASE' THEN al.base_quantity * COALESCE(al.unit_cost, 0) ELSE 0 END",
  totalkeluar: "CASE WHEN al.direction = 'DECREASE' THEN al.base_quantity * COALESCE(al.unit_cost, 0) ELSE 0 END",
  total: "(CASE WHEN al.direction = 'INCREASE' THEN al.base_quantity ELSE -al.base_quantity END) * COALESCE(al.unit_cost, 0)",
  nilai: "(CASE WHEN al.direction = 'INCREASE' THEN al.base_quantity ELSE -al.base_quantity END) * COALESCE(al.unit_cost, 0)",
  catatan: 'al.notes',
  samodifikasitgl: 'a.updated_at',
  balebar: 'i.width',
  bapanjang: 'i.length',
  batinggi: 'i.height',
  volume: 'i.volume',
  customdbl3: 'NULL',
  customtext1: 'NULL',
  'COUNT_x0028__x002A__x0029_': 'COUNT(*) OVER ()',
};

function saConfig(extraFrom = ''): InvDatasetConfig {
  return {
    from: SA_FROM + extraFrom,
    select: { ...SA_SELECT },
    where: 'a.deleted_at IS NULL',
    paramFilters: {
      ...periodFilters('a.adjustment_date'),
      ...docNoFilter('a.doc_number'),
      ...warehouseFilter('w.id', 'w.code', 'w.name'),
    },
    orderBy: 'a.adjustment_date, a.doc_number, al.line_no',
  };
}

function saDs2(kind: 'batch' | 'serial'): InvDatasetConfig {
  const isBatch = kind === 'batch';
  const cfg = saConfig(
    isBatch ? ' JOIN inv_lots lot ON lot.id = al.lot_id' : ' JOIN inv_serials ser ON ser.id = al.serial_id',
  );
  cfg.select = {
    ...cfg.select,
    nbtidbarang: 'al.item_id',
    nstidbarang: 'al.item_id',
    nbtkode: isBatch ? 'lot.lot_number' : 'NULL',
    nstkode: isBatch ? 'NULL' : 'ser.serial_number',
    nbtjml: 'al.base_quantity',
    nbtsatuan: 'u.name',
  };
  return cfg;
}

const IB_CONFIG: InvDatasetConfig = {
  from: `
    inv_opening_stocks o
    JOIN inv_opening_stock_lines ol ON ol.opening_stock_id = o.id
    JOIN md_items i ON i.id = ol.item_id
    LEFT JOIN md_units u ON u.id = ol.base_unit_id
    LEFT JOIN md_warehouses w ON w.id = o.warehouse_id
  `,
  select: {
    ibid: 'o.id',
    ibnotransaksi: 'o.doc_number',
    ibtgl: 'o.opening_date',
    ibgudang: 'o.warehouse_id',
    iburaian: 'o.description',
    ibstatus: statusLabel('o.status'),
    bkode: 'i.code',
    namabarang: 'i.name',
    jml: 'ol.quantity',
    satuan: 'u.name',
    hpplama: 'NULL',
    hpp: 'ol.unit_cost',
    catatan: 'ol.notes',
  },
  where: 'o.deleted_at IS NULL',
  paramFilters: {
    ...periodFilters('o.opening_date'),
    ...docNoFilter('o.doc_number'),
    ...warehouseFilter('w.id', 'w.code', 'w.name'),
  },
  orderBy: 'o.opening_date, o.doc_number, ol.line_no',
};

const PA_CONFIG: InvDatasetConfig = {
  from: `
    inv_cost_recalculations r
    JOIN inv_cost_recalculation_lines rl ON rl.cost_recalculation_id = r.id
    JOIN md_items i ON i.id = rl.item_id
    LEFT JOIN md_units u ON u.id = i.base_unit_id
  `,
  select: {
    paid: 'r.id',
    panotransaksi: 'r.doc_number',
    patgl: 'r.from_date',
    patglberlakusampai: 'r.to_date',
    pamatauang: "'IDR'",
    pakurs: '1',
    pauraian: 'r.notes',
    pastatus: statusLabel('r.status'),
    bkode: 'i.code',
    bid: 'i.id',
    bnama: 'i.name',
    namabarang: 'i.name',
    satuan: 'u.name',
    hargajual1lama: 'rl.old_unit_cost',
    hargajual1: 'rl.new_unit_cost',
    diskonjual1lama: 'NULL',
    diskonjual1: 'NULL',
  },
  where: 'r.deleted_at IS NULL',
  paramFilters: {
    ...periodFilters('r.from_date'),
    ...docNoFilter('r.doc_number'),
  },
  orderBy: 'r.from_date, r.doc_number, rl.line_no',
};

const DC_CONFIG: InvDatasetConfig = {
  from: `
    inv_daily_checks d
    JOIN inv_daily_check_lines dl ON dl.daily_check_id = d.id
    JOIN md_items i ON i.id = dl.item_id
    LEFT JOIN md_locations lc ON lc.id = d.location_id
  `,
  select: {
    dcid: 'd.id',
    dcmintake: 'd.machine_ref',
    dcnotransaksi: 'd.doc_number',
    dctgl: 'd.check_date',
    dcshift: 'NULL',
    dcstatus: statusLabel('d.status'),
    dcdimintaoleh: 'd.operator_ref',
    bkode: 'i.code',
    dcnamabarang: 'i.name',
    namabarang: 'i.name',
    dchmstart: 'NULL',
    dchmstop: 'NULL',
    total: 'dl.work_hours',
    catatan: 'dl.notes',
  },
  where: 'd.deleted_at IS NULL',
  paramFilters: {
    ...periodFilters('d.check_date'),
    location: {
      sql: `(d.location_id = CASE WHEN ? ~ '^[0-9]+$' THEN ?::bigint ELSE -1 END OR lc.code = ? OR lc.name ILIKE '%' || ? || '%')`,
      kind: 'text',
    },
  },
  orderBy: 'd.check_date, d.doc_number, dl.line_no',
};

const DC_DS2: InvDatasetConfig = {
  from: `
    inv_daily_check_lines dl
    JOIN inv_daily_checks d ON d.id = dl.daily_check_id
    LEFT JOIN md_cost_centers cc ON cc.id = dl.cost_center_id
  `,
  select: {
    dcid: 'd.id',
    ccnama: 'cc.name',
    status: 'NULL',
  },
  where: 'd.deleted_at IS NULL',
  orderBy: 'd.id, dl.line_no',
};

export const INV_DOC_CONFIGS: Record<string, InvReportConfig> = {
  /* -------- opening balances (m3_ib) -------- */
  'inv.saldoawalbarang': { datasets: { DS1: IB_CONFIG } },
  'inv.saldoawalbarang2': { datasets: { DS1: IB_CONFIG } },
  'inv.saldoawalbarangdetail': { datasets: { DS1: IB_CONFIG } },
  'inv.saldoawalbarangdetail2': { datasets: { DS1: IB_CONFIG } },

  /* -------- stock adjustments (m3_sa) -------- */
  'inv.daftarpenyesuaiandetail': { datasets: { DS1: saConfig() } },
  'inv.liststockadjustment': { datasets: { DS1: saConfig() } },
  'inv.liststockadjustmentdetail': { datasets: { DS1: saConfig() } },
  'inv.liststockadjustmentglobal': { datasets: { DS1: saConfig() } },
  'inv.liststockadjustmentglobal18072019': { datasets: { DS1: saConfig() } },
  'inv.liststockadjustmentperjenis': { datasets: { DS1: saConfig() } },
  'inv.liststockadjustmentperjenissa': { datasets: { DS1: saConfig() } },
  'inv.liststockadjustmentperkontak': { datasets: { DS1: saConfig() } },
  'inv.liststockadjustmentproduct': { datasets: { DS1: saConfig() } },
  'inv.stockadjustmentdetail': { datasets: { DS1: saConfig() } },
  'inv.stockadjustmentdetailnewvolume': { datasets: { DS1: saConfig() } },
  'inv.stockadjustmentdetailnewvolume2': { datasets: { DS1: saConfig() } },
  'inv.stockadjustmentdetailproduksi': { datasets: { DS1: saConfig() } },
  'inv.stockadjustmentdetailbatch': {
    datasets: { DS1: saConfig(), DS2: saDs2('batch') },
  },
  'inv.stockadjustmentdetailserial': {
    datasets: { DS1: saConfig(), DS2: saDs2('serial') },
  },

  /* -------- price adjustment (m3_pa → inv_cost_recalculations) -------- */
  'inv.listpricelistadjustment': { datasets: { DS1: PA_CONFIG } },
  'inv.listpricelistadjustment2': { datasets: { DS1: PA_CONFIG } },
  'inv.listpricelistadjustmentproduct': { datasets: { DS1: PA_CONFIG } },
  'inv.pricelistadjustmentdetail': { datasets: { DS1: PA_CONFIG } },
  'inv.pricelistadjustmentdetail2': { datasets: { DS1: PA_CONFIG } },

  /* -------- daily checks / timesheets (m3_dc) -------- */
  'inv.listtimesheet': { datasets: { DS1: DC_CONFIG, DS2: DC_DS2 } },
  'inv.timesheet': { datasets: { DS1: DC_CONFIG, DS2: DC_DS2 } },

  /* -------- material requisitions (REQUEST movements) -------- */
  'inv.listmaterialrequisition': { datasets: { DS1: movementDocConfig('mr', 'REQUEST') } },
  'inv.listmaterialrequisition2': { datasets: { DS1: movementDocConfig('mr', 'REQUEST') } },
  'inv.listmaterialrequisitionoutstanding': { datasets: { DS1: movementDocConfig('mr', 'REQUEST') } },
  'inv.listmaterialrequisitionproduct': { datasets: { DS1: movementDocConfig('mr', 'REQUEST') } },
  'inv.materialrequisitiondetail': { datasets: { DS1: movementDocConfig('mr', 'REQUEST') } },
  'inv.mrstokdetail': { datasets: { DS1: movementDocConfig('mr', 'REQUEST') } },
  'inv.mrstokglobal': { datasets: { DS1: movementDocConfig('mr', 'REQUEST') } },

  /* -------- transfers (TRANSFER movements) -------- */
  'inv.listtransferstock': { datasets: { DS1: movementDocConfig('ts', 'TRANSFER') } },
  'inv.listtransferstock2': { datasets: { DS1: movementDocConfig('ts', 'TRANSFER') } },
  'inv.listtransferstockoutstanding': { datasets: { DS1: movementDocConfig('ts', 'TRANSFER') } },
  'inv.listtransferstockproduct': { datasets: { DS1: movementDocConfig('ts', 'TRANSFER') } },
  'inv.transferstockdetail1newmakmur': { datasets: { DS1: movementDocConfig('ts', 'TRANSFER') } },
  'inv.transferstockdetailnewdenganharga': { datasets: { DS1: movementDocConfig('ts', 'TRANSFER') } },
  'inv.transferstockdetailnewdenganharga2': { datasets: { DS1: movementDocConfig('ts', 'TRANSFER') } },
  'inv.transferstockdetailud': { datasets: { DS1: movementDocConfig('ts', 'TRANSFER') } },
  'inv.transferstockdetailbatch': {
    datasets: { DS1: movementDocConfig('ts', 'TRANSFER'), DS2: movementDocDs2('ts', 'TRANSFER', 'batch') },
  },
  'inv.transferstockdetailserial': {
    datasets: { DS1: movementDocConfig('ts', 'TRANSFER'), DS2: movementDocDs2('ts', 'TRANSFER', 'serial') },
  },

  /* -------- receipts (TRANSFER_RECEIPT movements) -------- */
  'inv.listreceiptstock': { datasets: { DS1: movementDocConfig('rs', 'TRANSFER_RECEIPT') } },
  'inv.listreceiptstock2': { datasets: { DS1: movementDocConfig('rs', 'TRANSFER_RECEIPT') } },
  'inv.listreceiptstockproduct': { datasets: { DS1: movementDocConfig('rs', 'TRANSFER_RECEIPT') } },
  'inv.receiptstockdetailud': { datasets: { DS1: movementDocConfig('rs', 'TRANSFER_RECEIPT') } },
  'inv.receiptstockdetailbatch': {
    datasets: { DS1: movementDocConfig('rs', 'TRANSFER_RECEIPT'), DS2: movementDocDs2('rs', 'TRANSFER_RECEIPT', 'batch') },
  },
  'inv.receiptstockdetailserial': {
    datasets: { DS1: movementDocConfig('rs', 'TRANSFER_RECEIPT'), DS2: movementDocDs2('rs', 'TRANSFER_RECEIPT', 'serial') },
  },
};

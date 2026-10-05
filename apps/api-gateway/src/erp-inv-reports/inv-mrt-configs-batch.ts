/**
 * Batch (lot) & serial configs (Wave G4). Lot balances are DERIVED
 * from POSTED movement lines carrying a lot_id (the T1 convention:
 * balances are never stored); serial positions come from inv_serials
 * (current warehouse + status). Legacy in/out staging rows collapse
 * to one row per lot/serial with masuk/keluar/sisa aggregates.
 */

import {
  LINE_WH,
  SIGNED_QTY,
  partnerFilter,
  periodFilters,
  warehouseFilter,
  type InvDatasetConfig,
  type InvReportConfig,
} from './inv-mrt-configs';

const LOT_BALANCE_SUB = `(
  SELECT lot.id AS lot_id, l.item_id AS item_id, ${LINE_WH} AS wh_id,
    SUM(${SIGNED_QTY}) AS sisa,
    SUM(CASE WHEN ${SIGNED_QTY} > 0 THEN ${SIGNED_QTY} ELSE 0 END) AS masuk,
    SUM(CASE WHEN ${SIGNED_QTY} < 0 THEN -${SIGNED_QTY} ELSE 0 END) AS keluar
  FROM inv_stock_movement_lines l
  JOIN inv_stock_movements m ON m.id = l.stock_movement_id
  JOIN inv_lots lot ON lot.id = l.lot_id
  WHERE m.status = 'POSTED' AND m.deleted_at IS NULL
  GROUP BY lot.id, l.item_id, ${LINE_WH}
) lb`;

const LOT_FROM = `
  ${LOT_BALANCE_SUB}
  JOIN inv_lots lot ON lot.id = lb.lot_id
  JOIN md_items i ON i.id = lb.item_id
  LEFT JOIN md_warehouses w ON w.id = lb.wh_id
  LEFT JOIN md_units u ON u.id = i.base_unit_id
  LEFT JOIN pur_goods_receipts gr ON gr.id = lot.origin_goods_receipt_id
  LEFT JOIN md_partners p ON p.id = gr.supplier_id
`;

function lotBalanceConfig(): InvDatasetConfig {
  return {
    from: LOT_FROM,
    select: {
      // bp* (mutasi batch / rekap batch per tanggal)
      bpgudang: 'lb.wh_id',
      bpgudangnama: 'w.name',
      bpidbarang: 'i.id',
      bpkodebarang: 'i.code',
      bpnamabarang: 'i.name',
      bpnobatch: 'lot.lot_number',
      bptglexpired: 'lot.expiry_date',
      bpsatuan: 'u.name',
      bpmasuk: 'lb.masuk',
      bpkeluar: 'lb.keluar',
      bpsisa: 'lb.sisa',
      // nbi* (stok batch per gudang / laporan barang batch)
      nbiidbatchin: 'lot.id',
      nbigudang: 'lb.wh_id',
      wnama: 'w.name',
      kkode: 'p.code',
      knama: 'p.name',
      nbiidbarang: 'i.id',
      bkode: 'i.code',
      bnama: 'i.name',
      namabarang: 'i.name',
      nbikode: 'lot.lot_number',
      nbisumber: "'GRN'",
      nbiidtransaksi: 'lot.origin_goods_receipt_id',
      notransaksi: 'gr.doc_number',
      nbisatuan: 'u.name',
      nbijmlmasuk: 'lb.masuk',
      nbijmlkeluar: 'lb.keluar',
      nbijmlsisa: 'lb.sisa',
      jmlsisa: 'lb.sisa',
      nbiisclose: 'lb.sisa <= 0',
      volume: 'i.volume',
      // nbo* (out side of laporan barang batch — same aggregates)
      nboidbatchin: 'lot.id',
      sonotransaksi: 'gr.doc_number',
      sotgl: 'gr.created_at',
      sokanama: 'p.name',
      sonamabarang: 'i.name',
      nbojmlkeluar: 'lb.keluar',
      nbosatuan: 'u.name',
      nbokode: 'lot.lot_number',
      nboidtransaksi: 'lot.origin_goods_receipt_id',
      nboid: 'lot.id',
      tgl: 'lot.created_at',
    },
    paramFilters: {
      ...warehouseFilter('w.id', 'w.code', 'w.name'),
      ...partnerFilter('p.id', 'p.code', 'p.name'),
    },
    orderBy: 'w.name, i.code, lot.lot_number',
  };
}

/** Per-movement-line kartu batch/serial (one row per lot/serial line). */
function lotCardConfig(): InvDatasetConfig {
  return {
    from: `
      inv_stock_movement_lines l
      JOIN inv_stock_movements m ON m.id = l.stock_movement_id
      JOIN inv_lots lot ON lot.id = l.lot_id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.base_unit_id
      LEFT JOIN md_warehouses w ON w.id = ${LINE_WH}
    `,
    select: {
      gudang: LINE_WH,
      bkode: 'i.code',
      wnama: 'w.name',
      jenismutasi: 'm.movement_type::text',
      tgl: 'm.movement_date',
      notransaksi: 'm.doc_number',
      kodepa: 'NULL',
      namabarang: 'i.name',
      jmlmasuk: `CASE WHEN ${SIGNED_QTY} > 0 THEN ${SIGNED_QTY} ELSE 0 END`,
      jmlkeluar: `CASE WHEN ${SIGNED_QTY} < 0 THEN -${SIGNED_QTY} ELSE 0 END`,
      uraian: 'm.description',
      batch: 'lot.lot_number',
    },
    where: "m.deleted_at IS NULL AND m.status = 'POSTED'",
    paramFilters: {
      ...periodFilters('m.movement_date'),
      ...warehouseFilter('w.id', 'w.code', 'w.name'),
    },
    orderBy: 'm.movement_date, m.doc_number, l.line_no',
  };
}

function serialCardConfig(): InvDatasetConfig {
  return {
    from: `
      inv_stock_movement_lines l
      JOIN inv_stock_movements m ON m.id = l.stock_movement_id
      JOIN inv_serials ser ON ser.id = l.serial_id
      JOIN md_items i ON i.id = l.item_id
      LEFT JOIN md_units u ON u.id = l.base_unit_id
      LEFT JOIN md_warehouses w ON w.id = ${LINE_WH}
    `,
    select: {
      gudang: LINE_WH,
      bkode: 'i.code',
      jenismutasi: 'm.movement_type::text',
      tgl: 'm.movement_date',
      notransaksi: 'm.doc_number',
      kodepa: 'NULL',
      namabarang: 'i.name',
      jmlmasuk: `CASE WHEN ${SIGNED_QTY} > 0 THEN ${SIGNED_QTY} ELSE 0 END`,
      jmlkeluar: `CASE WHEN ${SIGNED_QTY} < 0 THEN -${SIGNED_QTY} ELSE 0 END`,
      uraian: 'm.description',
      serial: 'ser.serial_number',
    },
    where: "m.deleted_at IS NULL AND m.status = 'POSTED'",
    paramFilters: {
      ...periodFilters('m.movement_date'),
      ...warehouseFilter('w.id', 'w.code', 'w.name'),
    },
    orderBy: 'm.movement_date, m.doc_number, l.line_no',
  };
}

function serialPositionConfig(): InvDatasetConfig {
  return {
    from: `
      inv_serials s
      JOIN md_items i ON i.id = s.item_id
      LEFT JOIN md_warehouses w ON w.id = s.current_warehouse_id
      LEFT JOIN md_units u ON u.id = i.base_unit_id
      LEFT JOIN pur_goods_receipts gr ON gr.id = s.origin_goods_receipt_id
      LEFT JOIN md_partners p ON p.id = gr.supplier_id
    `,
    select: {
      nsiidserialin: 's.id',
      nsiidbarang: 'i.id',
      nsigudang: 's.current_warehouse_id',
      gudang: 's.current_warehouse_id',
      wnama: 'w.name',
      kkode: 'p.code',
      knama: 'p.name',
      bkode: 'i.code',
      bnama: 'i.name',
      namabarang: 'i.name',
      nsikode: 's.serial_number',
      nsisumber: "'GRN'",
      nsiidtransaksi: 's.origin_goods_receipt_id',
      notransaksi: 'gr.doc_number',
      tgl: 'gr.created_at',
      nsisatuan: 'u.name',
      satuan: 'u.name',
      nsijmlmasuk: '1',
      nsijmlkeluar: "CASE WHEN s.status = 'IN_STOCK' THEN 0 ELSE 1 END",
      nsojmlkeluar: "CASE WHEN s.status = 'IN_STOCK' THEN 0 ELSE 1 END",
      nsijmlsisa: "CASE WHEN s.status = 'IN_STOCK' THEN 1 ELSE 0 END",
      jmlsisa: "CASE WHEN s.status = 'IN_STOCK' THEN 1 ELSE 0 END",
      nsiisclose: "s.status <> 'IN_STOCK'",
      volume: 'i.volume',
      nsoidserialin: 's.id',
      sonotransaksi: 'NULL',
      sotgl: 'NULL',
      sokanama: 'NULL',
      sonamabarang: 'i.name',
      nsosatuan: 'u.name',
      nsokode: 's.serial_number',
      nsoidtransaksi: 's.origin_goods_receipt_id',
      nsoid: 's.id',
    },
    where: 's.deleted_at IS NULL',
    paramFilters: {
      ...warehouseFilter('w.id', 'w.code', 'w.name'),
      ...partnerFilter('p.id', 'p.code', 'p.name'),
    },
    orderBy: 'i.code, s.serial_number',
  };
}

export const INV_BATCH_CONFIGS: Record<string, InvReportConfig> = {
  'inv.mutasibatch': { datasets: { DS1: lotBalanceConfig() } },
  'inv.rekapbatchpertanggal': { datasets: { DS1: lotBalanceConfig() } },
  'inv.stokbatchpergudang': { datasets: { DS1: lotBalanceConfig() } },
  'inv.stokbatchpergudangperbarang': { datasets: { DS1: lotBalanceConfig() } },
  'inv.stokbatchperkontakpergudang': { datasets: { DS1: lotBalanceConfig() } },
  'inv.laporanbarangbatch': { datasets: { DS1: lotBalanceConfig() } },
  'inv.laporankartubarangbatch': { datasets: { DS1: lotCardConfig() } },
  'inv.stokserialpergudang': { datasets: { DS1: serialPositionConfig() } },
  'inv.stokserialpergudangperbarang': { datasets: { DS1: serialPositionConfig() } },
  'inv.laporanbarangserial': { datasets: { DS1: serialPositionConfig() } },
  'fin.laporanbarangserial': { datasets: { DS1: serialPositionConfig() } },
  'inv.laporankartubarangserial': { datasets: { DS1: serialCardConfig() } },
};

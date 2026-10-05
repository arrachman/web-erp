/**
 * Merged dataset-builder map for Inventory .mrt reports (Wave G4),
 * including the 25 inventory-analytics keys misplaced in the legacy
 * m2 folder (registered under fin.* report_keys — they render through
 * this provider and appear in the Finance combo box, per registry).
 *
 * Not built here (stay CONVERTED with the generic fallback, see
 * DECISIONS G4): fin.nilaipersediaansatuan / -satuan25 / -satuanhj and
 * fin.nilaipersediaangudangdetailrswijaya (per-customer unit/payer
 * columns hardcoded in the legacy staging schema), inv.materialused
 * (production material costing → Wave G7).
 */

import { emptyConfig, type InvReportConfig } from './inv-mrt-configs';
import { INV_STOCK_CONFIGS } from './inv-mrt-configs-stock';
import { INV_DOC_CONFIGS } from './inv-mrt-configs-docs';
import { INV_COUNT_CONFIGS } from './inv-mrt-configs-counts';
import { INV_BATCH_CONFIGS } from './inv-mrt-configs-batch';

const RF_NOTE =
  'Dokumen pengisian bahan bakar legacy (m3_rf + m1_item_hauling) belum ada padanannya di ERP: ' +
  'bahan bakar dicatat sebagai pengeluaran stok biasa (ISSUE), tidak ada entitas refuel/kendaraan.';
const DAILY_STOCK_NOTE =
  'Staging harian legacy (m2r_dailyavailablestock) dihitung ETL per pelanggan dengan kode site BR/BW ' +
  'dan parameter hedging sendiri — tidak ada padanan semantik di ERP.';
const GROUPING_RM_NOTE =
  'Grouping bahan baku legacy membaca staging m2r_dailyavailablestock per divisi/departemen dengan ' +
  'snapshot qtybefore1/2 — tidak ada padanan di ERP.';
const KONSINYASI_NOTE =
  'Rekap barang konsinyasi legacy (m2r_rekap_barang_konsinyasi) butuh pelacakan mutasi konsinyasi ' +
  'per pelanggan; ERP belum memiliki entitas konsinyasi.';

const emptyReport = (note: string): InvReportConfig => ({
  datasets: { DS1: emptyConfig(note) },
});

export const INV_MRT_CONFIGS: Record<string, InvReportConfig> = {
  ...INV_STOCK_CONFIGS,
  ...INV_DOC_CONFIGS,
  ...INV_COUNT_CONFIGS,
  ...INV_BATCH_CONFIGS,

  /* -------- honest empty: no ERP equivalent -------- */
  'inv.listpengisianbahanbakar': emptyReport(RF_NOTE),
  'inv.pemakaiansolar': emptyReport(RF_NOTE),
  'inv.pemakaiansolar3': emptyReport(RF_NOTE),
  'inv.pengisianbahanbakardetail': emptyReport(RF_NOTE),
  'inv.pengisianbahanbakardetail2': emptyReport(RF_NOTE),
  'inv.dailyavailablestock': {
    datasets: { DS1: emptyConfig(DAILY_STOCK_NOTE), DS2: emptyConfig(DAILY_STOCK_NOTE) },
  },
  'inv.dailyrekonbahanmaklon': emptyReport(DAILY_STOCK_NOTE),
  'inv.hedgingdailyavailablestock': emptyReport(DAILY_STOCK_NOTE),
  'inv.groupingrm': emptyReport(GROUPING_RM_NOTE),
  'inv.rekapbarangkonsinyasi': emptyReport(KONSINYASI_NOTE),
};

/** Families computed by the TS replay engine (running balances). */
export type InvComputedFamily = 'ks' | 'ks-summary' | 'ms' | 'ms-sumber' | 'pd' | 'stok-pivot';

export const INV_MRT_COMPUTED: Record<string, InvComputedFamily> = {
  // kartu stok (ks*) — inv
  'inv.kartustokaverage': 'ks',
  'inv.kartustokaverage1baris': 'ks',
  'inv.kartustokfifo': 'ks',
  'inv.kartustokfifo2': 'ks',
  'inv.kartustokfifo3': 'ks',
  'inv.kartustokkhusus': 'ks',
  'inv.kartustokrekapdetail': 'ks',
  'inv.kartustokrekapfifo': 'ks',
  'inv.kartustokrekapglobal': 'ks',
  'inv.inventoryactualbalancesheet': 'ks',
  // kartu stok (ks*) — fin.* analytics keys
  'fin.kartustokaverage': 'ks',
  'fin.kartustokfifo': 'ks',
  'fin.kartustokkhusus': 'ks',
  'fin.nilaipersediaan': 'ks',
  'fin.nilaipersediaanperkategori': 'ks',
  'fin.nilaipersediaangudangglobal': 'ks',
  'fin.saldopersediaangudangdetail': 'ks',
  'fin.saldopersediaangudangdetail2oktober2015': 'ks',
  'fin.saldopersediaangudangglobal': 'ks',
  'fin.saldopersediaangudangglobal2oktober2015': 'ks',
  'fin.nilaipersediaangudangdetail': 'ks-summary',
  // mutasi stok per-row (ms*)
  'inv.mutasistok': 'ms',
  'inv.mutasistokbengkulu': 'ms',
  'inv.mutasistokcostcenter': 'ms',
  'inv.mutasistokdivisi': 'ms',
  'inv.mutasistokproyek': 'ms',
  'fin.mutasistok': 'ms',
  'inv.mutasistokrekapsumber': 'ms-sumber',
  // persediaan detail (pd*)
  'fin.persediaanbarangdetail': 'pd',
  'fin.persediaanbarangdetail2': 'pd',
  // stok pivot per gudang (g1..g10 / j1..j10)
  'inv.stok': 'stok-pivot',
};

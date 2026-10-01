/**
 * Seed Chart of Accounts (md_accounts) — Senti ERP.
 * Format kode akun: NNNN.NN.NNN (4-2-3, dual dot, 11 char total).
 *   - 4 digit prefix = kelompok-grup PSAK (1xxx Aset, 2xxx Liabilitas, dst).
 *   - 2 digit middle = sub-grup / sub-kategori (max 99 anak per cabang).
 *   - 3 digit leaf = nomor urut akun (max 999, mirror legacy `.NNN`).
 * Hierarki:
 *   L1 HEADER  `1000.00.000` Kelompok (Aset/…)
 *   L2 HEADER  `1100.00.000` Grup (Aset Lancar/…)
 *   L3 HEADER  `1101.00.000` Sub-grup (Kas, Bank, Piutang, …) — untuk grouping UI
 *   L4 POSTABLE `1101.01.001` Akun postable di bawah sub-grup
 * Lihat keputusan format di apps/web-erp/db-design/README.md §8.
 * Run: npx ts-node prisma/seed-erp-accounts.ts
 * Idempotent: upsert by code.
 */
import { PrismaClient, ErpAccountType, ErpAccountKind, ErpNormalBalance, ErpCashFlowCategory } from '@prisma/client';

const prisma = new PrismaClient();

type AccountSeed = {
  code: string;
  name: string;
  alias?: string;
  type: ErpAccountType;
  kind: ErpAccountKind;
  normalBalance: ErpNormalBalance;
  level: number;
  parentCode?: string;
  cashFlowCategory?: ErpCashFlowCategory;
  isControlAccount?: boolean;
  bankName?: string;
  bankAccountNo?: string;
  notes?: string;
  legacyCode?: string;
};

const A = ErpAccountType.ASSET;
const L = ErpAccountType.LIABILITY;
const E = ErpAccountType.EQUITY;
const R = ErpAccountType.REVENUE;
const X = ErpAccountType.EXPENSE;
const H = ErpAccountKind.HEADER;
const P = ErpAccountKind.POSTABLE;
const D = ErpNormalBalance.DEBIT;
const C = ErpNormalBalance.CREDIT;
const OP = ErpCashFlowCategory.OPERATING;
const IV = ErpCashFlowCategory.INVESTING;
const FN = ErpCashFlowCategory.FINANCING;

const ACCOUNTS: AccountSeed[] = [
  // ── LEVEL 1: Kelompok Utama ────────────────────────────────────────────────
  { code: '1000.00.000', name: 'Aset',                              type: A, kind: H, normalBalance: D, level: 1 },
  { code: '2000.00.000', name: 'Kewajiban',                         type: L, kind: H, normalBalance: C, level: 1 },
  { code: '3000.00.000', name: 'Ekuitas',                           type: E, kind: H, normalBalance: C, level: 1 },
  { code: '4000.00.000', name: 'Pendapatan',                        type: R, kind: H, normalBalance: C, level: 1 },
  { code: '5000.00.000', name: 'Harga Pokok Penjualan',             type: X, kind: H, normalBalance: D, level: 1 },
  { code: '6000.00.000', name: 'Beban Operasional',                 type: X, kind: H, normalBalance: D, level: 1 },
  { code: '7000.00.000', name: 'Pos Luar Biasa & Pajak',            type: X, kind: H, normalBalance: D, level: 1 },

  // ── LEVEL 2: Grup ──────────────────────────────────────────────────────────
  { code: '1100.00.000', name: 'Aset Lancar',          type: A, kind: H, normalBalance: D, level: 2, parentCode: '1000.00.000' },
  { code: '1200.00.000', name: 'Aset Tetap',           type: A, kind: H, normalBalance: D, level: 2, parentCode: '1000.00.000' },
  { code: '1300.00.000', name: 'Aset Tidak Berwujud',  type: A, kind: H, normalBalance: D, level: 2, parentCode: '1000.00.000' },
  { code: '1400.00.000', name: 'Investasi Jangka Panjang', type: A, kind: H, normalBalance: D, level: 2, parentCode: '1000.00.000' },

  { code: '2100.00.000', name: 'Kewajiban Jangka Pendek', type: L, kind: H, normalBalance: C, level: 2, parentCode: '2000.00.000' },
  { code: '2200.00.000', name: 'Kewajiban Jangka Panjang', type: L, kind: H, normalBalance: C, level: 2, parentCode: '2000.00.000' },

  { code: '3100.00.000', name: 'Modal dan Cadangan', type: E, kind: H, normalBalance: C, level: 2, parentCode: '3000.00.000' },

  { code: '4100.00.000', name: 'Pendapatan Usaha',    type: R, kind: H, normalBalance: C, level: 2, parentCode: '4000.00.000' },
  { code: '4200.00.000', name: 'Pendapatan Lain-lain', type: R, kind: H, normalBalance: C, level: 2, parentCode: '4000.00.000' },

  { code: '5100.00.000', name: 'Harga Pokok Produksi', type: X, kind: H, normalBalance: D, level: 2, parentCode: '5000.00.000' },
  { code: '5200.00.000', name: 'Beban Distribusi',     type: X, kind: H, normalBalance: D, level: 2, parentCode: '5000.00.000' },

  { code: '6100.00.000', name: 'Beban Penjualan',              type: X, kind: H, normalBalance: D, level: 2, parentCode: '6000.00.000' },
  { code: '6200.00.000', name: 'Beban Umum dan Administrasi',  type: X, kind: H, normalBalance: D, level: 2, parentCode: '6000.00.000' },
  { code: '6300.00.000', name: 'Beban Keuangan',               type: X, kind: H, normalBalance: D, level: 2, parentCode: '6000.00.000' },

  { code: '7100.00.000', name: 'Pos Luar Biasa',    type: X, kind: H, normalBalance: D, level: 2, parentCode: '7000.00.000' },
  { code: '7200.00.000', name: 'Pajak Penghasilan', type: X, kind: H, normalBalance: D, level: 2, parentCode: '7000.00.000' },

  // ── LEVEL 3: Sub-grup (HEADER) — grouping Kas / Bank / Piutang / … ─────────
  // Aset Lancar
  { code: '1101.00.000', name: 'Kas',                      type: A, kind: H, normalBalance: D, level: 3, parentCode: '1100.00.000' },
  { code: '1110.00.000', name: 'Bank',                     type: A, kind: H, normalBalance: D, level: 3, parentCode: '1100.00.000' },
  { code: '1120.00.000', name: 'Piutang',                  type: A, kind: H, normalBalance: D, level: 3, parentCode: '1100.00.000' },
  { code: '1130.00.000', name: 'Persediaan',               type: A, kind: H, normalBalance: D, level: 3, parentCode: '1100.00.000' },
  { code: '1140.00.000', name: 'Pajak Dibayar Dimuka',     type: A, kind: H, normalBalance: D, level: 3, parentCode: '1100.00.000' },
  { code: '1150.00.000', name: 'Biaya Dibayar Dimuka',     type: A, kind: H, normalBalance: D, level: 3, parentCode: '1100.00.000' },

  // Aset Tetap
  { code: '1201.00.000', name: 'Tanah',                    type: A, kind: H, normalBalance: D, level: 3, parentCode: '1200.00.000' },
  { code: '1202.00.000', name: 'Bangunan',                 type: A, kind: H, normalBalance: D, level: 3, parentCode: '1200.00.000' },
  { code: '1210.00.000', name: 'Mesin dan Peralatan',      type: A, kind: H, normalBalance: D, level: 3, parentCode: '1200.00.000' },
  { code: '1220.00.000', name: 'Kendaraan',                type: A, kind: H, normalBalance: D, level: 3, parentCode: '1200.00.000' },
  { code: '1230.00.000', name: 'Peralatan Kantor',         type: A, kind: H, normalBalance: D, level: 3, parentCode: '1200.00.000' },
  { code: '1240.00.000', name: 'Inventaris',               type: A, kind: H, normalBalance: D, level: 3, parentCode: '1200.00.000' },

  // Aset Tidak Berwujud
  { code: '1301.00.000', name: 'Software dan Lisensi',     type: A, kind: H, normalBalance: D, level: 3, parentCode: '1300.00.000' },
  { code: '1310.00.000', name: 'Hak Merek dan Paten',      type: A, kind: H, normalBalance: D, level: 3, parentCode: '1300.00.000' },
  { code: '1320.00.000', name: 'Biaya Pendirian',          type: A, kind: H, normalBalance: D, level: 3, parentCode: '1300.00.000' },

  // Investasi
  { code: '1401.00.000', name: 'Investasi Saham dan Obligasi', type: A, kind: H, normalBalance: D, level: 3, parentCode: '1400.00.000' },

  // Kewajiban Jangka Pendek
  { code: '2101.00.000', name: 'Hutang Usaha',             type: L, kind: H, normalBalance: C, level: 3, parentCode: '2100.00.000' },
  { code: '2102.00.000', name: 'Hutang Bank Jangka Pendek', type: L, kind: H, normalBalance: C, level: 3, parentCode: '2100.00.000' },
  { code: '2103.00.000', name: 'Hutang Operasional',       type: L, kind: H, normalBalance: C, level: 3, parentCode: '2100.00.000' },
  { code: '2110.00.000', name: 'Hutang Pajak',             type: L, kind: H, normalBalance: C, level: 3, parentCode: '2100.00.000' },
  { code: '2120.00.000', name: 'Hutang Lainnya Jangka Pendek', type: L, kind: H, normalBalance: C, level: 3, parentCode: '2100.00.000' },

  // Kewajiban Jangka Panjang
  { code: '2201.00.000', name: 'Hutang Bank Jangka Panjang', type: L, kind: H, normalBalance: C, level: 3, parentCode: '2200.00.000' },
  { code: '2202.00.000', name: 'Hutang Obligasi dan Leasing', type: L, kind: H, normalBalance: C, level: 3, parentCode: '2200.00.000' },
  { code: '2210.00.000', name: 'Liabilitas Pajak dan Imbalan Kerja', type: L, kind: H, normalBalance: C, level: 3, parentCode: '2200.00.000' },

  // Ekuitas
  { code: '3101.00.000', name: 'Modal',                    type: E, kind: H, normalBalance: C, level: 3, parentCode: '3100.00.000' },
  { code: '3103.00.000', name: 'Laba Ditahan',             type: E, kind: H, normalBalance: C, level: 3, parentCode: '3100.00.000' },
  { code: '3110.00.000', name: 'Cadangan',                 type: E, kind: H, normalBalance: C, level: 3, parentCode: '3100.00.000' },
  { code: '3120.00.000', name: 'Komponen Ekuitas Lain',    type: E, kind: H, normalBalance: C, level: 3, parentCode: '3100.00.000' },

  // Pendapatan
  { code: '4101.00.000', name: 'Penjualan',                type: R, kind: H, normalBalance: C, level: 3, parentCode: '4100.00.000' },
  { code: '4110.00.000', name: 'Potongan Penjualan',       type: R, kind: H, normalBalance: C, level: 3, parentCode: '4100.00.000' },
  { code: '4201.00.000', name: 'Pendapatan Non-Usaha',     type: R, kind: H, normalBalance: C, level: 3, parentCode: '4200.00.000' },

  // HPP
  { code: '5101.00.000', name: 'Biaya Produksi Langsung',  type: X, kind: H, normalBalance: D, level: 3, parentCode: '5100.00.000' },
  { code: '5110.00.000', name: 'Overhead Pabrik',          type: X, kind: H, normalBalance: D, level: 3, parentCode: '5100.00.000' },
  { code: '5201.00.000', name: 'Beban Angkut',             type: X, kind: H, normalBalance: D, level: 3, parentCode: '5200.00.000' },

  // Beban Operasional
  { code: '6101.00.000', name: 'Beban Personil Penjualan', type: X, kind: H, normalBalance: D, level: 3, parentCode: '6100.00.000' },
  { code: '6103.00.000', name: 'Beban Promosi dan Distribusi', type: X, kind: H, normalBalance: D, level: 3, parentCode: '6100.00.000' },
  { code: '6201.00.000', name: 'Beban Gaji dan Tunjangan', type: X, kind: H, normalBalance: D, level: 3, parentCode: '6200.00.000' },
  { code: '6210.00.000', name: 'Beban Kantor dan Utilitas', type: X, kind: H, normalBalance: D, level: 3, parentCode: '6200.00.000' },
  { code: '6217.00.000', name: 'Beban Penyusutan dan Amortisasi', type: X, kind: H, normalBalance: D, level: 3, parentCode: '6200.00.000' },
  { code: '6220.00.000', name: 'Beban Administrasi Lain',  type: X, kind: H, normalBalance: D, level: 3, parentCode: '6200.00.000' },
  { code: '6301.00.000', name: 'Beban Bunga dan Bank',     type: X, kind: H, normalBalance: D, level: 3, parentCode: '6300.00.000' },
  { code: '6303.00.000', name: 'Beban Kurs dan Provisi',   type: X, kind: H, normalBalance: D, level: 3, parentCode: '6300.00.000' },

  // Pos luar biasa & pajak
  { code: '7101.00.000', name: 'Pos Luar Biasa',           type: X, kind: H, normalBalance: D, level: 3, parentCode: '7100.00.000' },
  { code: '7201.00.000', name: 'Beban Pajak Penghasilan',  type: X, kind: H, normalBalance: D, level: 3, parentCode: '7200.00.000' },

  // ── LEVEL 4: Akun postable ─────────────────────────────────────────────────
  // Kas
  { code: '1101.01.001', name: 'Kas Besar',                 alias: 'Kas Utama',          type: A, kind: P, normalBalance: D, level: 4, parentCode: '1101.00.000', cashFlowCategory: OP, legacyCode: '1-1101' },
  { code: '1102.01.001', name: 'Kas Kecil',                 alias: 'Petty Cash',         type: A, kind: P, normalBalance: D, level: 4, parentCode: '1101.00.000', cashFlowCategory: OP, legacyCode: '1-1102' },
  // Bank
  { code: '1110.01.001', name: 'Bank BCA - Giro IDR',       alias: 'BCA IDR',            type: A, kind: P, normalBalance: D, level: 4, parentCode: '1110.00.000', cashFlowCategory: OP, bankName: 'Bank BCA',    bankAccountNo: '123-456-7890', legacyCode: '1-1110' },
  { code: '1111.01.001', name: 'Bank Mandiri - Giro IDR',   alias: 'Mandiri IDR',        type: A, kind: P, normalBalance: D, level: 4, parentCode: '1110.00.000', cashFlowCategory: OP, bankName: 'Bank Mandiri', bankAccountNo: '156-000-8888888', legacyCode: '1-1111' },
  { code: '1112.01.001', name: 'Bank BNI - Giro IDR',       alias: 'BNI IDR',            type: A, kind: P, normalBalance: D, level: 4, parentCode: '1110.00.000', cashFlowCategory: OP, bankName: 'Bank BNI',    bankAccountNo: '0987654321', legacyCode: '1-1112' },
  { code: '1113.01.001', name: 'Bank BRI - Giro IDR',       alias: 'BRI IDR',            type: A, kind: P, normalBalance: D, level: 4, parentCode: '1110.00.000', cashFlowCategory: OP, bankName: 'Bank BRI',    bankAccountNo: '0321-01-001234-56-7', legacyCode: '1-1113' },
  { code: '1114.01.001', name: 'Bank CIMB Niaga - Giro IDR', alias: 'CIMB IDR',          type: A, kind: P, normalBalance: D, level: 4, parentCode: '1110.00.000', cashFlowCategory: OP, bankName: 'Bank CIMB Niaga', bankAccountNo: '800123456700' },
  { code: '1115.01.001', name: 'Bank Mandiri - Giro USD',   alias: 'Mandiri USD',        type: A, kind: P, normalBalance: D, level: 4, parentCode: '1110.00.000', cashFlowCategory: OP, bankName: 'Bank Mandiri', bankAccountNo: '156-000-9999999' },
  // Piutang
  { code: '1120.01.001', name: 'Piutang Dagang',                   alias: 'AR Trade',   type: A, kind: P, normalBalance: D, level: 4, parentCode: '1120.00.000', cashFlowCategory: OP, isControlAccount: true,  legacyCode: '1-1120' },
  { code: '1121.01.001', name: 'Cadangan Kerugian Piutang',        alias: 'Allowance',  type: A, kind: P, normalBalance: C, level: 4, parentCode: '1120.00.000', cashFlowCategory: OP, notes: 'Contra asset — kontra piutang dagang', legacyCode: '1-1121' },
  { code: '1122.01.001', name: 'Piutang Karyawan',                                      type: A, kind: P, normalBalance: D, level: 4, parentCode: '1120.00.000', cashFlowCategory: OP, legacyCode: '1-1122' },
  { code: '1123.01.001', name: 'Uang Muka Pembelian',              alias: 'DP Beli',    type: A, kind: P, normalBalance: D, level: 4, parentCode: '1120.00.000', cashFlowCategory: OP, legacyCode: '1-1123' },
  { code: '1124.01.001', name: 'Piutang Lainnya',                                       type: A, kind: P, normalBalance: D, level: 4, parentCode: '1120.00.000', cashFlowCategory: OP },
  { code: '1125.01.001', name: 'Giro Masuk Dalam Proses Kliring',  alias: 'Giro Masuk', type: A, kind: P, normalBalance: D, level: 4, parentCode: '1120.00.000', cashFlowCategory: OP },
  // Persediaan
  { code: '1130.01.001', name: 'Persediaan Bahan Baku',            alias: 'Raw Mat',       type: A, kind: P, normalBalance: D, level: 4, parentCode: '1130.00.000', cashFlowCategory: OP, legacyCode: '1-1130' },
  { code: '1131.01.001', name: 'Persediaan Bahan Pembantu',        alias: 'Aux Mat',       type: A, kind: P, normalBalance: D, level: 4, parentCode: '1130.00.000', cashFlowCategory: OP, legacyCode: '1-1131' },
  { code: '1132.01.001', name: 'Persediaan Barang Dalam Proses',   alias: 'WIP',           type: A, kind: P, normalBalance: D, level: 4, parentCode: '1130.00.000', cashFlowCategory: OP, legacyCode: '1-1132' },
  { code: '1133.01.001', name: 'Persediaan Barang Jadi',           alias: 'Finished Goods', type: A, kind: P, normalBalance: D, level: 4, parentCode: '1130.00.000', cashFlowCategory: OP, legacyCode: '1-1133' },
  { code: '1134.01.001', name: 'Persediaan Perlengkapan / Suku Cadang', alias: 'Spare Parts', type: A, kind: P, normalBalance: D, level: 4, parentCode: '1130.00.000', cashFlowCategory: OP, legacyCode: '1-1134' },
  // Pajak Dibayar Dimuka
  { code: '1140.01.001', name: 'PPN Masukan',                   alias: 'VAT In',     type: A, kind: P, normalBalance: D, level: 4, parentCode: '1140.00.000', cashFlowCategory: OP, legacyCode: '1-1140' },
  { code: '1141.01.001', name: 'PPh Pasal 22 Dibayar Dimuka',  alias: 'PPh 22',     type: A, kind: P, normalBalance: D, level: 4, parentCode: '1140.00.000', cashFlowCategory: OP },
  { code: '1142.01.001', name: 'PPh Pasal 23 Dibayar Dimuka',  alias: 'PPh 23',     type: A, kind: P, normalBalance: D, level: 4, parentCode: '1140.00.000', cashFlowCategory: OP },
  { code: '1143.01.001', name: 'PPh Pasal 25 Dibayar Dimuka',  alias: 'PPh 25',     type: A, kind: P, normalBalance: D, level: 4, parentCode: '1140.00.000', cashFlowCategory: OP },
  { code: '1144.01.001', name: 'Fiskal Tahun Berjalan',                              type: A, kind: P, normalBalance: D, level: 4, parentCode: '1140.00.000', cashFlowCategory: OP },
  // Biaya Dibayar Dimuka
  { code: '1150.01.001', name: 'Biaya Sewa Dibayar Dimuka',    alias: 'Prepaid Rent',   type: A, kind: P, normalBalance: D, level: 4, parentCode: '1150.00.000', cashFlowCategory: OP },
  { code: '1151.01.001', name: 'Biaya Asuransi Dibayar Dimuka', alias: 'Prepaid Ins',   type: A, kind: P, normalBalance: D, level: 4, parentCode: '1150.00.000', cashFlowCategory: OP },
  { code: '1152.01.001', name: 'Biaya Lainnya Dibayar Dimuka', alias: 'Other Prepaid',  type: A, kind: P, normalBalance: D, level: 4, parentCode: '1150.00.000', cashFlowCategory: OP },

  // Aset Tetap postable
  { code: '1201.01.001', name: 'Tanah',                                type: A, kind: P, normalBalance: D, level: 4, parentCode: '1201.00.000', cashFlowCategory: IV, notes: 'Tidak disusutkan', legacyCode: '1-1201' },
  { code: '1202.01.001', name: 'Bangunan dan Prasarana',               type: A, kind: P, normalBalance: D, level: 4, parentCode: '1202.00.000', cashFlowCategory: IV, legacyCode: '1-1202' },
  { code: '1203.01.001', name: 'Akumulasi Penyusutan Bangunan',        type: A, kind: P, normalBalance: C, level: 4, parentCode: '1202.00.000', cashFlowCategory: IV, legacyCode: '1-1203' },
  { code: '1210.01.001', name: 'Mesin Produksi',                       type: A, kind: P, normalBalance: D, level: 4, parentCode: '1210.00.000', cashFlowCategory: IV, legacyCode: '1-1210' },
  { code: '1211.01.001', name: 'Akumulasi Penyusutan Mesin',           type: A, kind: P, normalBalance: C, level: 4, parentCode: '1210.00.000', cashFlowCategory: IV, legacyCode: '1-1211' },
  { code: '1212.01.001', name: 'Peralatan Pabrik',                     type: A, kind: P, normalBalance: D, level: 4, parentCode: '1210.00.000', cashFlowCategory: IV },
  { code: '1213.01.001', name: 'Akumulasi Penyusutan Peralatan Pabrik', type: A, kind: P, normalBalance: C, level: 4, parentCode: '1210.00.000', cashFlowCategory: IV },
  { code: '1220.01.001', name: 'Kendaraan',                            type: A, kind: P, normalBalance: D, level: 4, parentCode: '1220.00.000', cashFlowCategory: IV, legacyCode: '1-1220' },
  { code: '1221.01.001', name: 'Akumulasi Penyusutan Kendaraan',       type: A, kind: P, normalBalance: C, level: 4, parentCode: '1220.00.000', cashFlowCategory: IV, legacyCode: '1-1221' },
  { code: '1230.01.001', name: 'Peralatan Kantor',                     type: A, kind: P, normalBalance: D, level: 4, parentCode: '1230.00.000', cashFlowCategory: IV, legacyCode: '1-1230' },
  { code: '1231.01.001', name: 'Akumulasi Penyusutan Peralatan Kantor', type: A, kind: P, normalBalance: C, level: 4, parentCode: '1230.00.000', cashFlowCategory: IV, legacyCode: '1-1231' },
  { code: '1240.01.001', name: 'Inventaris Kantor',                    type: A, kind: P, normalBalance: D, level: 4, parentCode: '1240.00.000', cashFlowCategory: IV },
  { code: '1241.01.001', name: 'Akumulasi Penyusutan Inventaris',      type: A, kind: P, normalBalance: C, level: 4, parentCode: '1240.00.000', cashFlowCategory: IV },

  // Aset Tidak Berwujud
  { code: '1301.01.001', name: 'Lisensi Software',                    type: A, kind: P, normalBalance: D, level: 4, parentCode: '1301.00.000', cashFlowCategory: IV },
  { code: '1302.01.001', name: 'Akumulasi Amortisasi Lisensi',        type: A, kind: P, normalBalance: C, level: 4, parentCode: '1301.00.000', cashFlowCategory: IV },
  { code: '1310.01.001', name: 'Hak Merek dan Paten',                 type: A, kind: P, normalBalance: D, level: 4, parentCode: '1310.00.000', cashFlowCategory: IV },
  { code: '1311.01.001', name: 'Akumulasi Amortisasi Hak Merek',      type: A, kind: P, normalBalance: C, level: 4, parentCode: '1310.00.000', cashFlowCategory: IV },
  { code: '1320.01.001', name: 'Biaya Pendirian / Organisasi',        type: A, kind: P, normalBalance: D, level: 4, parentCode: '1320.00.000', cashFlowCategory: IV },
  { code: '1321.01.001', name: 'Akumulasi Amortisasi Biaya Pendirian', type: A, kind: P, normalBalance: C, level: 4, parentCode: '1320.00.000', cashFlowCategory: IV },

  // Investasi
  { code: '1401.01.001', name: 'Investasi Saham di Entitas Anak',     type: A, kind: P, normalBalance: D, level: 4, parentCode: '1401.00.000', cashFlowCategory: IV },
  { code: '1402.01.001', name: 'Investasi Saham di Entitas Asosiasi', type: A, kind: P, normalBalance: D, level: 4, parentCode: '1401.00.000', cashFlowCategory: IV },
  { code: '1403.01.001', name: 'Investasi Obligasi',                  type: A, kind: P, normalBalance: D, level: 4, parentCode: '1401.00.000', cashFlowCategory: IV },

  // Kewajiban JP
  { code: '2101.01.001', name: 'Hutang Dagang',                alias: 'AP Trade',   type: L, kind: P, normalBalance: C, level: 4, parentCode: '2101.00.000', cashFlowCategory: OP, isControlAccount: true, legacyCode: '2-2101' },
  { code: '2102.01.001', name: 'Hutang Bank Jangka Pendek',    alias: 'ST Bank Debt', type: L, kind: P, normalBalance: C, level: 4, parentCode: '2102.00.000', cashFlowCategory: FN, legacyCode: '2-2102' },
  { code: '2103.01.001', name: 'Hutang Gaji Karyawan',                               type: L, kind: P, normalBalance: C, level: 4, parentCode: '2103.00.000', cashFlowCategory: OP },
  { code: '2104.01.001', name: 'Biaya Yang Masih Harus Dibayar', alias: 'Accrued Exp', type: L, kind: P, normalBalance: C, level: 4, parentCode: '2103.00.000', cashFlowCategory: OP, legacyCode: '2-2104' },
  { code: '2105.01.001', name: 'Uang Muka Penjualan',          alias: 'DP Jual',    type: L, kind: P, normalBalance: C, level: 4, parentCode: '2103.00.000', cashFlowCategory: OP },
  { code: '2106.01.001', name: 'Giro Keluar Dalam Proses',     alias: 'Giro Keluar', type: L, kind: P, normalBalance: C, level: 4, parentCode: '2103.00.000', cashFlowCategory: OP },
  { code: '2110.01.001', name: 'PPN Keluaran',                 alias: 'VAT Out',    type: L, kind: P, normalBalance: C, level: 4, parentCode: '2110.00.000', cashFlowCategory: OP, legacyCode: '2-2110' },
  { code: '2111.01.001', name: 'PPh Pasal 21 Terutang',        alias: 'PPh 21',     type: L, kind: P, normalBalance: C, level: 4, parentCode: '2110.00.000', cashFlowCategory: OP, legacyCode: '2-2111' },
  { code: '2112.01.001', name: 'PPh Pasal 23 Terutang',        alias: 'PPh 23',     type: L, kind: P, normalBalance: C, level: 4, parentCode: '2110.00.000', cashFlowCategory: OP },
  { code: '2113.01.001', name: 'PPh Pasal 25 Terutang',        alias: 'PPh 25',     type: L, kind: P, normalBalance: C, level: 4, parentCode: '2110.00.000', cashFlowCategory: OP },
  { code: '2114.01.001', name: 'PPh Badan Terutang',                                 type: L, kind: P, normalBalance: C, level: 4, parentCode: '2110.00.000', cashFlowCategory: OP, legacyCode: '2-2114' },
  { code: '2115.01.001', name: 'BPJS Ketenagakerjaan Terutang',                      type: L, kind: P, normalBalance: C, level: 4, parentCode: '2110.00.000', cashFlowCategory: OP },
  { code: '2116.01.001', name: 'BPJS Kesehatan Terutang',                            type: L, kind: P, normalBalance: C, level: 4, parentCode: '2110.00.000', cashFlowCategory: OP },
  { code: '2120.01.001', name: 'Dividen Yang Harus Dibayar',                         type: L, kind: P, normalBalance: C, level: 4, parentCode: '2120.00.000', cashFlowCategory: FN },
  { code: '2121.01.001', name: 'Hutang Jangka Panjang - Jatuh Tempo', alias: 'Current LTD', type: L, kind: P, normalBalance: C, level: 4, parentCode: '2120.00.000', cashFlowCategory: FN },

  // Kewajiban Jangka Panjang
  { code: '2201.01.001', name: 'Hutang Bank Jangka Panjang',   alias: 'LT Bank Debt', type: L, kind: P, normalBalance: C, level: 4, parentCode: '2201.00.000', cashFlowCategory: FN, legacyCode: '2-2201' },
  { code: '2202.01.001', name: 'Hutang Obligasi',                                      type: L, kind: P, normalBalance: C, level: 4, parentCode: '2202.00.000', cashFlowCategory: FN },
  { code: '2203.01.001', name: 'Hutang Sewa Pembiayaan (Leasing)',                     type: L, kind: P, normalBalance: C, level: 4, parentCode: '2202.00.000', cashFlowCategory: FN, legacyCode: '2-2203' },
  { code: '2210.01.001', name: 'Liabilitas Pajak Tangguhan',                           type: L, kind: P, normalBalance: C, level: 4, parentCode: '2210.00.000', cashFlowCategory: OP },
  { code: '2220.01.001', name: 'Cadangan Imbalan Pasca-Kerja',  alias: 'DPLK/Pesangon', type: L, kind: P, normalBalance: C, level: 4, parentCode: '2210.00.000', cashFlowCategory: OP },

  // Ekuitas
  { code: '3101.01.001', name: 'Modal Saham Disetor',           alias: 'Share Capital',  type: E, kind: P, normalBalance: C, level: 4, parentCode: '3101.00.000' },
  { code: '3102.01.001', name: 'Tambahan Modal Disetor (Agio)', alias: 'Agio Saham',     type: E, kind: P, normalBalance: C, level: 4, parentCode: '3101.00.000' },
  { code: '3103.01.001', name: 'Saldo Laba Ditahan',            alias: 'Retained Earnings', type: E, kind: P, normalBalance: C, level: 4, parentCode: '3103.00.000' },
  { code: '3104.01.001', name: 'Laba (Rugi) Tahun Berjalan',   alias: 'Current Profit', type: E, kind: P, normalBalance: C, level: 4, parentCode: '3103.00.000' },
  { code: '3110.01.001', name: 'Cadangan Umum',                                          type: E, kind: P, normalBalance: C, level: 4, parentCode: '3110.00.000' },
  { code: '3111.01.001', name: 'Cadangan Khusus',                                        type: E, kind: P, normalBalance: C, level: 4, parentCode: '3110.00.000' },
  { code: '3120.01.001', name: 'Selisih Kurs Penjabaran',       alias: 'CTA',            type: E, kind: P, normalBalance: C, level: 4, parentCode: '3120.00.000' },

  // Pendapatan Usaha
  { code: '4101.01.001', name: 'Penjualan Produk Jadi',         alias: 'Sales FG',    type: R, kind: P, normalBalance: C, level: 4, parentCode: '4101.00.000', cashFlowCategory: OP, legacyCode: '4-4101' },
  { code: '4102.01.001', name: 'Penjualan Jasa Pengolahan',     alias: 'Toll Mfg',    type: R, kind: P, normalBalance: C, level: 4, parentCode: '4101.00.000', cashFlowCategory: OP },
  { code: '4103.01.001', name: 'Penjualan Bahan Baku / Sisa',   alias: 'Scrap Sales', type: R, kind: P, normalBalance: C, level: 4, parentCode: '4101.00.000', cashFlowCategory: OP },
  { code: '4110.01.001', name: 'Retur Penjualan',               alias: 'Sales Return', type: R, kind: P, normalBalance: D, level: 4, parentCode: '4110.00.000', cashFlowCategory: OP, notes: 'Contra revenue', legacyCode: '4-4110' },
  { code: '4111.01.001', name: 'Diskon Penjualan',              alias: 'Sales Disc',  type: R, kind: P, normalBalance: D, level: 4, parentCode: '4110.00.000', cashFlowCategory: OP, notes: 'Contra revenue', legacyCode: '4-4111' },

  // Pendapatan Lain-lain
  { code: '4201.01.001', name: 'Pendapatan Bunga Bank',         alias: 'Interest Inc', type: R, kind: P, normalBalance: C, level: 4, parentCode: '4201.00.000', cashFlowCategory: IV },
  { code: '4202.01.001', name: 'Keuntungan Selisih Kurs',                              type: R, kind: P, normalBalance: C, level: 4, parentCode: '4201.00.000', cashFlowCategory: OP },
  { code: '4203.01.001', name: 'Keuntungan Penjualan Aset Tetap',                      type: R, kind: P, normalBalance: C, level: 4, parentCode: '4201.00.000', cashFlowCategory: IV },
  { code: '4204.01.001', name: 'Dividen Diterima',                                     type: R, kind: P, normalBalance: C, level: 4, parentCode: '4201.00.000', cashFlowCategory: IV },
  { code: '4205.01.001', name: 'Pendapatan Sewa',                                      type: R, kind: P, normalBalance: C, level: 4, parentCode: '4201.00.000', cashFlowCategory: OP },
  { code: '4206.01.001', name: 'Pendapatan Lainnya',                                   type: R, kind: P, normalBalance: C, level: 4, parentCode: '4201.00.000', cashFlowCategory: OP },

  // HPP
  { code: '5101.01.001', name: 'Pemakaian Bahan Baku',          alias: 'Raw Mat Used',  type: X, kind: P, normalBalance: D, level: 4, parentCode: '5101.00.000', cashFlowCategory: OP, legacyCode: '5-5101' },
  { code: '5102.01.001', name: 'Pemakaian Bahan Pembantu',      alias: 'Aux Mat Used',  type: X, kind: P, normalBalance: D, level: 4, parentCode: '5101.00.000', cashFlowCategory: OP, legacyCode: '5-5102' },
  { code: '5103.01.001', name: 'Tenaga Kerja Langsung',         alias: 'Direct Labor',  type: X, kind: P, normalBalance: D, level: 4, parentCode: '5101.00.000', cashFlowCategory: OP, legacyCode: '5-5103' },
  { code: '5110.01.001', name: 'Overhead Pabrik — Tetap',       alias: 'Fixed FOH',     type: X, kind: P, normalBalance: D, level: 4, parentCode: '5110.00.000', cashFlowCategory: OP, legacyCode: '5-5110' },
  { code: '5111.01.001', name: 'Overhead Pabrik — Variabel',    alias: 'Variable FOH',  type: X, kind: P, normalBalance: D, level: 4, parentCode: '5110.00.000', cashFlowCategory: OP, legacyCode: '5-5111' },
  { code: '5120.01.001', name: 'Penyusutan Mesin Produksi',                             type: X, kind: P, normalBalance: D, level: 4, parentCode: '5110.00.000', cashFlowCategory: OP },
  { code: '5121.01.001', name: 'Biaya Pemeliharaan Mesin',                              type: X, kind: P, normalBalance: D, level: 4, parentCode: '5110.00.000', cashFlowCategory: OP },
  { code: '5122.01.001', name: 'Biaya Listrik Pabrik',                                  type: X, kind: P, normalBalance: D, level: 4, parentCode: '5110.00.000', cashFlowCategory: OP },
  { code: '5123.01.001', name: 'Biaya Air dan Gas Pabrik',                              type: X, kind: P, normalBalance: D, level: 4, parentCode: '5110.00.000', cashFlowCategory: OP },
  { code: '5201.01.001', name: 'Beban Pengiriman Pembelian',    alias: 'Freight In',    type: X, kind: P, normalBalance: D, level: 4, parentCode: '5201.00.000', cashFlowCategory: OP },
  { code: '5202.01.001', name: 'Beban Pengiriman Penjualan',    alias: 'Freight Out',   type: X, kind: P, normalBalance: D, level: 4, parentCode: '5201.00.000', cashFlowCategory: OP },

  // Beban Penjualan
  { code: '6101.01.001', name: 'Beban Gaji Bagian Penjualan',                           type: X, kind: P, normalBalance: D, level: 4, parentCode: '6101.00.000', cashFlowCategory: OP, legacyCode: '6-6101' },
  { code: '6102.01.001', name: 'Beban Komisi Agen / Salesman',                          type: X, kind: P, normalBalance: D, level: 4, parentCode: '6101.00.000', cashFlowCategory: OP, legacyCode: '6-6102' },
  { code: '6103.01.001', name: 'Beban Promosi dan Iklan',                               type: X, kind: P, normalBalance: D, level: 4, parentCode: '6103.00.000', cashFlowCategory: OP, legacyCode: '6-6103' },
  { code: '6104.01.001', name: 'Beban Transportasi Penjualan',                          type: X, kind: P, normalBalance: D, level: 4, parentCode: '6103.00.000', cashFlowCategory: OP },
  { code: '6105.01.001', name: 'Beban Kemasan',                                         type: X, kind: P, normalBalance: D, level: 4, parentCode: '6103.00.000', cashFlowCategory: OP },
  { code: '6106.01.001', name: 'Beban Pameran dan Expo',                                type: X, kind: P, normalBalance: D, level: 4, parentCode: '6103.00.000', cashFlowCategory: OP },
  { code: '6107.01.001', name: 'Beban Garansi dan Retur Pelanggan',                     type: X, kind: P, normalBalance: D, level: 4, parentCode: '6103.00.000', cashFlowCategory: OP },

  // Beban Umum & Administrasi
  { code: '6201.01.001', name: 'Beban Gaji Direksi',                                    type: X, kind: P, normalBalance: D, level: 4, parentCode: '6201.00.000', cashFlowCategory: OP, legacyCode: '6-6201' },
  { code: '6202.01.001', name: 'Beban Gaji Staff Administrasi',                         type: X, kind: P, normalBalance: D, level: 4, parentCode: '6201.00.000', cashFlowCategory: OP, legacyCode: '6-6202' },
  { code: '6203.01.001', name: 'Beban Tunjangan Karyawan',                              type: X, kind: P, normalBalance: D, level: 4, parentCode: '6201.00.000', cashFlowCategory: OP },
  { code: '6204.01.001', name: 'Beban BPJS Ketenagakerjaan',                            type: X, kind: P, normalBalance: D, level: 4, parentCode: '6201.00.000', cashFlowCategory: OP },
  { code: '6205.01.001', name: 'Beban BPJS Kesehatan',                                  type: X, kind: P, normalBalance: D, level: 4, parentCode: '6201.00.000', cashFlowCategory: OP },
  { code: '6210.01.001', name: 'Beban Alat Tulis Kantor',                               type: X, kind: P, normalBalance: D, level: 4, parentCode: '6210.00.000', cashFlowCategory: OP, legacyCode: '6-6210' },
  { code: '6211.01.001', name: 'Beban Sewa Kantor dan Gedung',                          type: X, kind: P, normalBalance: D, level: 4, parentCode: '6210.00.000', cashFlowCategory: OP },
  { code: '6212.01.001', name: 'Beban Listrik, Air, dan Gas',                           type: X, kind: P, normalBalance: D, level: 4, parentCode: '6210.00.000', cashFlowCategory: OP, legacyCode: '6-6212' },
  { code: '6213.01.001', name: 'Beban Telepon dan Internet',                            type: X, kind: P, normalBalance: D, level: 4, parentCode: '6210.00.000', cashFlowCategory: OP },
  { code: '6214.01.001', name: 'Beban Perjalanan Dinas',                                type: X, kind: P, normalBalance: D, level: 4, parentCode: '6210.00.000', cashFlowCategory: OP, legacyCode: '6-6214' },
  { code: '6215.01.001', name: 'Beban Pemeliharaan Gedung',                             type: X, kind: P, normalBalance: D, level: 4, parentCode: '6210.00.000', cashFlowCategory: OP },
  { code: '6216.01.001', name: 'Beban Asuransi',                                        type: X, kind: P, normalBalance: D, level: 4, parentCode: '6210.00.000', cashFlowCategory: OP },
  { code: '6217.01.001', name: 'Beban Penyusutan Aset Tetap',                           type: X, kind: P, normalBalance: D, level: 4, parentCode: '6217.00.000', cashFlowCategory: OP },
  { code: '6218.01.001', name: 'Beban Amortisasi Aset Tak Berwujud',                    type: X, kind: P, normalBalance: D, level: 4, parentCode: '6217.00.000', cashFlowCategory: OP },
  { code: '6219.01.001', name: 'Beban Pajak dan Retribusi Daerah',                      type: X, kind: P, normalBalance: D, level: 4, parentCode: '6220.00.000', cashFlowCategory: OP },
  { code: '6220.01.001', name: 'Beban Representasi dan Entertainment',                  type: X, kind: P, normalBalance: D, level: 4, parentCode: '6220.00.000', cashFlowCategory: OP, legacyCode: '6-6220' },
  { code: '6221.01.001', name: 'Beban Jasa Profesional (Konsultan/Auditor)',            type: X, kind: P, normalBalance: D, level: 4, parentCode: '6220.00.000', cashFlowCategory: OP, legacyCode: '6-6221' },
  { code: '6222.01.001', name: 'Beban Piutang Tak Tertagih',                            type: X, kind: P, normalBalance: D, level: 4, parentCode: '6220.00.000', cashFlowCategory: OP },
  { code: '6223.01.001', name: 'Beban Keamanan dan K3',                                 type: X, kind: P, normalBalance: D, level: 4, parentCode: '6220.00.000', cashFlowCategory: OP },
  { code: '6224.01.001', name: 'Beban Lain-lain Administrasi',                          type: X, kind: P, normalBalance: D, level: 4, parentCode: '6220.00.000', cashFlowCategory: OP },

  // Beban Keuangan
  { code: '6301.01.001', name: 'Beban Bunga Pinjaman Bank',     alias: 'Interest Exp',  type: X, kind: P, normalBalance: D, level: 4, parentCode: '6301.00.000', cashFlowCategory: FN, legacyCode: '6-6301' },
  { code: '6302.01.001', name: 'Beban Administrasi Bank',                               type: X, kind: P, normalBalance: D, level: 4, parentCode: '6301.00.000', cashFlowCategory: OP },
  { code: '6303.01.001', name: 'Beban Selisih Kurs',            alias: 'FX Loss',       type: X, kind: P, normalBalance: D, level: 4, parentCode: '6303.00.000', cashFlowCategory: OP },
  { code: '6304.01.001', name: 'Beban Provisi dan Biaya Kredit',                        type: X, kind: P, normalBalance: D, level: 4, parentCode: '6303.00.000', cashFlowCategory: FN },
  { code: '6305.01.001', name: 'Beban Denda Pajak',                                     type: X, kind: P, normalBalance: D, level: 4, parentCode: '6303.00.000', cashFlowCategory: OP },

  // Pos Luar Biasa & Pajak
  { code: '7101.01.001', name: 'Kerugian Bencana Alam / Kerusakan',                    type: X, kind: P, normalBalance: D, level: 4, parentCode: '7101.00.000', cashFlowCategory: OP },
  { code: '7102.01.001', name: 'Kerugian Penjualan Aset Tetap',                        type: X, kind: P, normalBalance: D, level: 4, parentCode: '7101.00.000', cashFlowCategory: IV },
  { code: '7103.01.001', name: 'Beban Restrukturisasi',                                type: X, kind: P, normalBalance: D, level: 4, parentCode: '7101.00.000', cashFlowCategory: OP },
  { code: '7201.01.001', name: 'Beban Pajak Penghasilan Badan',  alias: 'Income Tax',   type: X, kind: P, normalBalance: D, level: 4, parentCode: '7201.00.000', cashFlowCategory: OP },
  { code: '7202.01.001', name: 'Pajak Tangguhan',                alias: 'Deferred Tax', type: X, kind: P, normalBalance: D, level: 4, parentCode: '7201.00.000', cashFlowCategory: OP },
];

async function main() {
  console.log('Seeding Chart of Accounts...');
  const codeToId = new Map<string, bigint>();
  const maxLevel = Math.max(...ACCOUNTS.map((a) => a.level));

  for (let level = 1; level <= maxLevel; level++) {
    const batch = ACCOUNTS.filter((a) => a.level === level);
    for (const acc of batch) {
      const parentId = acc.parentCode ? codeToId.get(acc.parentCode) ?? null : null;
      const result = await prisma.erpAccount.upsert({
        where: { code: acc.code },
        update: {
          name: acc.name,
          alias: acc.alias ?? null,
          type: acc.type,
          kind: acc.kind,
          normalBalance: acc.normalBalance,
          level: acc.level,
          parentId,
          cashFlowCategory: acc.cashFlowCategory ?? null,
          isControlAccount: acc.isControlAccount ?? false,
          bankName: acc.bankName ?? null,
          bankAccountNo: acc.bankAccountNo ?? null,
          notes: acc.notes ?? null,
          legacyCode: acc.legacyCode ?? null,
          deletedAt: null,
          isActive: true,
        },
        create: {
          code: acc.code,
          name: acc.name,
          alias: acc.alias,
          type: acc.type,
          kind: acc.kind,
          normalBalance: acc.normalBalance,
          level: acc.level,
          parentId,
          cashFlowCategory: acc.cashFlowCategory,
          isControlAccount: acc.isControlAccount ?? false,
          bankName: acc.bankName,
          bankAccountNo: acc.bankAccountNo,
          notes: acc.notes,
          legacyCode: acc.legacyCode,
        },
      });
      codeToId.set(acc.code, result.id);
    }
    console.log(`  Level ${level}: ${batch.length} akun`);
  }

  const total = await prisma.erpAccount.count({ where: { deletedAt: null } });
  console.log(`Done. Total akun aktif: ${total}`);
}

main()
  .catch(console.error)
  .finally(() => prisma.$disconnect());

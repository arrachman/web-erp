/**
 * Generic transaction form configuration table. Drives the `TrxForm`
 * page — every Keuangan "Tambah" route (kas/bank/giro/jurnal/saldo
 * awal) renders the same orchestration with different labels, codes,
 * account defaults, and feature flags (journal / giro / cancel /
 * opening). Ported 1:1 from prototype `pages/trx-form.jsx` (`TRX_CFG`).
 *
 * Plain TS (no JSX) — safe to import from anywhere without pulling
 * React into the bundle.
 */

export type TrxKind =
  | 'kas-masuk'
  | 'kas-keluar'
  | 'bank-masuk'
  | 'bank-keluar'
  | 'jurnal-umum'
  | 'giro-masuk'
  | 'giro-keluar'
  | 'giro-masuk-batal'
  | 'giro-keluar-batal'
  | 'saldo-awal';

export interface TrxConfig {
  label: string;
  code: string;
  /** Header party label, e.g. 'Terima Dari' / 'Bayar Ke'. `null` → journal/opening. */
  party: string | null;
  /** Header account label, e.g. 'Akun Kas' / 'Akun Bank' / 'Periode'. `null` → journal. */
  acct: string | null;
  /** Default value for the account field (display string). */
  acctVal?: string;
  contact?: boolean;
  journal?: boolean;
  giro?: boolean;
  cancel?: boolean;
  opening?: boolean;
}

export const TRX_CFG: Record<TrxKind, TrxConfig> = {
  'kas-masuk': {
    label: 'Kas Masuk',
    code: 'CR',
    party: 'Terima Dari',
    acct: 'Akun Kas',
    acctVal: '110101.101 - Cash (IDR)',
    contact: true,
  },
  'kas-keluar': {
    label: 'Kas Keluar',
    code: 'CD',
    party: 'Bayar Ke',
    acct: 'Akun Kas',
    acctVal: '110101.101 - Cash (IDR)',
    contact: true,
  },
  'bank-masuk': {
    label: 'Bank Masuk',
    code: 'RM',
    party: 'Terima Dari',
    acct: 'Akun Bank',
    acctVal: '110102.001 - Bank BCA Operasional',
    contact: true,
  },
  'bank-keluar': {
    label: 'Bank Keluar',
    code: 'SM',
    party: 'Bayar Ke',
    acct: 'Akun Bank',
    acctVal: '110102.001 - Bank BCA Operasional',
    contact: true,
  },
  'jurnal-umum': {
    label: 'Jurnal Umum',
    code: 'GJ',
    party: null,
    acct: null,
    journal: true,
  },
  'giro-masuk': {
    label: 'Giro Masuk',
    code: 'RG',
    party: 'Terima Dari',
    acct: 'Akun Giro',
    acctVal: '110103.001 - Giro/Cek Diterima',
    contact: true,
    giro: true,
  },
  'giro-keluar': {
    label: 'Giro Keluar',
    code: 'SG',
    party: 'Bayar Ke',
    acct: 'Akun Giro',
    acctVal: '210103.001 - Giro/Cek Dikeluarkan',
    contact: true,
    giro: true,
  },
  'giro-masuk-batal': {
    label: 'Giro Masuk Batal',
    code: 'RGC',
    party: 'Terima Dari',
    acct: 'Akun Giro',
    acctVal: '110103.001 - Giro/Cek Diterima',
    contact: true,
    giro: true,
    cancel: true,
  },
  'giro-keluar-batal': {
    label: 'Giro Keluar Batal',
    code: 'SGC',
    party: 'Bayar Ke',
    acct: 'Akun Giro',
    acctVal: '210103.001 - Giro/Cek Dikeluarkan',
    contact: true,
    giro: true,
    cancel: true,
  },
  'saldo-awal': {
    label: 'Saldo Awal Coa',
    code: 'CB',
    party: null,
    acct: 'Periode',
    acctVal: 'Mei 2026',
    opening: true,
  },
};

export function resolveTrxConfig(moduleId: string): TrxConfig {
  return (TRX_CFG as Record<string, TrxConfig | undefined>)[moduleId]
    ?? TRX_CFG['kas-masuk'];
}

// ---------------------------------------------------------------------------
// Detail line shape — kept here so both <TrxForm> and <TrxLines> import the
// same source of truth without a circular dependency.
// ---------------------------------------------------------------------------

export interface TrxLine {
  id: number;
  no: string;
  nama: string;
  total?: number;
  debit?: number;
  kredit?: number;
  catatan: string;
  cc: string;
  proyek?: string;
}

// TrxLineColumn is superseded by GridColumnDef<TrxLine> from lib/grid-types.
// Re-exported here so callers that already import from trx-form-config don't break.
export type { GridColumnDef as TrxLineColumn } from '@/lib/grid-types';

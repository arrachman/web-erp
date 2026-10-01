/**
 * Bank Masuk (RM) form model — extends the shared cash/bank model
 * (cash-bank-form-model.ts) with the two fields unique to bank receipts:
 * Cara Bayar (paymentMethod) and the Giro tab rows. Reuses the shared
 * default/from/to mappers + the shared Giro editor organism so neither the
 * header/contra lines nor the giro grid are duplicated (§3 / §2.1).
 */

import {
  defaultCashBankForm,
  fromCashBankTransaction,
  toCashBankPayload,
  type CashBankFormData,
} from './cash-bank-form-model';
import { newGiro, type GiroRow } from '@/components/organisms/cash-bank-giros';
import type {
  CreateBankReceiptPayload,
  CashBankGiroPayload,
  ErpBankReceipt,
  ErpCashBankGiro,
  ErpPaymentMethod,
} from '@/lib/api/fin-bank-receipts';
import type { ErpCashReceipt } from '@/lib/api/fin-cash-receipts';

export interface BankReceiptFormData extends CashBankFormData {
  paymentMethod: ErpPaymentMethod;
  giros: GiroRow[];
}

/** Cara Bayar options (§2.6: ≥3 → Select). Transfer = default (screenshot). */
export const PAYMENT_METHODS: { value: ErpPaymentMethod; label: string }[] = [
  { value: 'TRANSFER', label: 'Transfer' },
  { value: 'GIRO', label: 'Giro' },
  { value: 'CHEQUE', label: 'Cek' },
  { value: 'CASH', label: 'Tunai' },
  { value: 'CARD', label: 'Kartu' },
  { value: 'OTHER', label: 'Lainnya' },
];

/** Display label for a Cara Bayar value (falls back to the raw value). */
export const paymentMethodLabel = (m?: string | null): string =>
  PAYMENT_METHODS.find((x) => x.value === m)?.label ?? m ?? '—';

const fromGiro = (g: ErpCashBankGiro): GiroRow => ({
  ...newGiro(),
  id: g.id,
  giroNumber: g.giroNumber,
  bankName: g.bankName ?? '',
  bankAccountNo: g.bankAccountNo ?? '',
  amount: g.amount,
  dueDate: g.dueDate.slice(0, 10),
  notes: g.notes ?? '',
});

/** Rows with a giro number + positive amount become payload; blanks dropped. */
const toGiroPayload = (rows: GiroRow[]): CashBankGiroPayload[] =>
  rows
    .filter((r) => r.giroNumber.trim() && Number(r.amount) > 0)
    .map((r, i) => ({
      giroNumber: r.giroNumber.trim(),
      bankName: r.bankName || undefined,
      bankAccountNo: r.bankAccountNo || undefined,
      amount: r.amount,
      dueDate: r.dueDate,
      notes: r.notes || undefined,
      lineNo: i + 1,
    }));

export function defaultBankReceiptForm(): BankReceiptFormData {
  return { ...defaultCashBankForm(), paymentMethod: 'TRANSFER', giros: [] };
}

export function fromBankReceipt(r: ErpBankReceipt): BankReceiptFormData {
  return {
    ...fromCashBankTransaction(r as unknown as ErpCashReceipt),
    paymentMethod: (r.paymentMethod ?? 'TRANSFER') as ErpPaymentMethod,
    giros: (r.giros ?? []).map(fromGiro),
  };
}

export function toBankReceiptPayload(d: BankReceiptFormData): CreateBankReceiptPayload {
  return {
    ...toCashBankPayload(d, 'RECEIPT'),
    kind: 'BANK',
    paymentMethod: d.paymentMethod,
    giros: toGiroPayload(d.giros),
  };
}

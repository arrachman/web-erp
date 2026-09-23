'use client';

/**
 * Cashbank Transfer (CR) — create/edit form fields with nested journal lines.
 * Atomic tier: Organism sub-part.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import type {
  CreateCashbankTransferPayload,
  ErpCashbankTransfer,
} from '@/lib/api/fin-cashbank-transfers';
import type { ErpJournalLine } from '@/lib/api/fin-journal-entries';
import { JournalLinesEditor } from './fin-shared-lines';

export interface CashbankTransferFormData {
  docNumber: string;
  branchId: string;
  cashAccountId: string;
  entryDate: string;
  fiscalPeriodId: string;
  partnerId: string;
  description: string;
  notes: string;
  currencyId: string;
  exchangeRate: string;
  lines: ErpJournalLine[];
}

export const defaultCashbankTransferForm = (): CashbankTransferFormData => ({
  docNumber: '',
  branchId: '1',
  cashAccountId: '',
  entryDate: new Date().toISOString().slice(0, 10),
  fiscalPeriodId: '1',
  partnerId: '',
  description: '',
  notes: '',
  currencyId: '1',
  exchangeRate: '1.000000',
  lines: [],
});

export function fromCashbankTransfer(r: ErpCashbankTransfer): CashbankTransferFormData {
  return {
    docNumber: r.docNumber,
    branchId: r.branchId,
    cashAccountId: r.cashAccountId,
    entryDate: r.entryDate.slice(0, 10),
    fiscalPeriodId: r.fiscalPeriodId,
    partnerId: r.partnerId ?? '',
    description: r.description,
    notes: r.notes ?? '',
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    lines: r.lines.map((l) => ({ ...l })),
  };
}

export function toCashbankTransferPayload(
  f: CashbankTransferFormData,
): CreateCashbankTransferPayload {
  return {
    docNumber: f.docNumber,
    branchId: f.branchId,
    cashAccountId: f.cashAccountId,
    entryDate: f.entryDate,
    fiscalPeriodId: f.fiscalPeriodId,
    description: f.description,
    currencyId: f.currencyId,
    exchangeRate: f.exchangeRate,
    notes: f.notes || undefined,
    partnerId: f.partnerId || undefined,
    lines: f.lines.map((l, i) => ({ ...l, lineNo: i + 1 })),
  };
}

export function CashbankTransferFormFields({
  data,
  onChange,
}: {
  data: CashbankTransferFormData;
  onChange: (d: CashbankTransferFormData) => void;
}) {
  const set = <K extends keyof CashbankTransferFormData>(
    k: K,
    v: CashbankTransferFormData[K],
  ) => onChange({ ...data, [k]: v });

  return (
    <div className="p-4">
      <FormField label="No. Dokumen" htmlFor="cb-doc" required>
        <Input
          id="cb-doc"
          value={data.docNumber}
          onChange={(e) => set('docNumber', e.target.value)}
          placeholder="CB-2026-000001"
        />
      </FormField>
      <FormField label="Tanggal" htmlFor="cb-date" required>
        <DateInput
          id="cb-date"
          value={data.entryDate}
          onChange={(v) => set('entryDate', v)}
        />
      </FormField>
      <FormField label="Cash Account ID" htmlFor="cb-cash" required>
        <Input
          id="cb-cash"
          value={data.cashAccountId}
          onChange={(e) => set('cashAccountId', e.target.value)}
          placeholder="ID rekening kas/bank"
        />
      </FormField>
      <FormField label="Branch ID" htmlFor="cb-branch" required>
        <Input
          id="cb-branch"
          value={data.branchId}
          onChange={(e) => set('branchId', e.target.value)}
        />
      </FormField>
      <FormField label="Fiscal Period ID" htmlFor="cb-fp" required>
        <Input
          id="cb-fp"
          value={data.fiscalPeriodId}
          onChange={(e) => set('fiscalPeriodId', e.target.value)}
        />
      </FormField>
      <FormField label="Partner ID" htmlFor="cb-partner">
        <Input
          id="cb-partner"
          value={data.partnerId}
          onChange={(e) => set('partnerId', e.target.value)}
        />
      </FormField>
      <FormField label="Currency ID" htmlFor="cb-cur" required>
        <Input
          id="cb-cur"
          value={data.currencyId}
          onChange={(e) => set('currencyId', e.target.value)}
        />
      </FormField>
      <FormField label="Kurs" htmlFor="cb-rate" required>
        <Input
          id="cb-rate"
          value={data.exchangeRate}
          onChange={(e) => set('exchangeRate', e.target.value)}
        />
      </FormField>
      <FormField label="Deskripsi" htmlFor="cb-desc" required>
        <Input
          id="cb-desc"
          value={data.description}
          onChange={(e) => set('description', e.target.value)}
        />
      </FormField>
      <FormField label="Catatan" htmlFor="cb-notes">
        <Input
          id="cb-notes"
          value={data.notes}
          onChange={(e) => set('notes', e.target.value)}
        />
      </FormField>
      <JournalLinesEditor
        lines={data.lines}
        onChange={(lines) => onChange({ ...data, lines })}
      />
    </div>
  );
}

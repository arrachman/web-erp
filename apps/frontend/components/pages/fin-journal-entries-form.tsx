'use client';

/**
 * Journal Entry create/edit form fields with nested lines editor.
 * Atomic tier: Organism sub-part.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type {
  CreateJournalEntryPayload,
  ErpJournalEntry,
  ErpJournalLine,
  ErpJournalType,
} from '@/lib/api/fin-journal-entries';

const JOURNAL_TYPES: ErpJournalType[] = [
  'GENERAL',
  'MEMORIAL',
  'ADJUSTMENT',
  'OPENING_BALANCE',
  'CLOSING',
];

export interface JournalFormData {
  docNumber: string;
  journalType: ErpJournalType;
  branchId: string;
  entryDate: string;
  fiscalPeriodId: string;
  description: string;
  notes: string;
  currencyId: string;
  exchangeRate: string;
  lines: ErpJournalLine[];
}

export const defaultJournalForm = (): JournalFormData => ({
  docNumber: '',
  journalType: 'GENERAL',
  branchId: '1',
  entryDate: new Date().toISOString().slice(0, 10),
  fiscalPeriodId: '1',
  description: '',
  notes: '',
  currencyId: '1',
  exchangeRate: '1.000000',
  lines: [],
});

const emptyLine = (lineNo: number): ErpJournalLine => ({
  accountId: '',
  currencyId: '1',
  exchangeRate: '1.000000',
  debit: '0.0000',
  credit: '0.0000',
  lineNo,
});

export function fromEntry(entry: ErpJournalEntry): JournalFormData {
  return {
    docNumber: entry.docNumber,
    journalType: entry.journalType,
    branchId: entry.branchId,
    entryDate: entry.entryDate.slice(0, 10),
    fiscalPeriodId: entry.fiscalPeriodId,
    description: entry.description,
    notes: entry.notes ?? '',
    currencyId: entry.currencyId,
    exchangeRate: entry.exchangeRate,
    lines: entry.lines.map((l) => ({
      accountId: l.accountId,
      currencyId: l.currencyId,
      exchangeRate: l.exchangeRate,
      debit: l.debit,
      credit: l.credit,
      notes: l.notes ?? undefined,
      lineNo: l.lineNo,
    })),
  };
}

export function toJournalPayload(f: JournalFormData): CreateJournalEntryPayload {
  return {
    docNumber: f.docNumber,
    journalType: f.journalType,
    branchId: f.branchId,
    entryDate: f.entryDate,
    fiscalPeriodId: f.fiscalPeriodId,
    description: f.description,
    currencyId: f.currencyId,
    exchangeRate: f.exchangeRate,
    notes: f.notes || undefined,
    lines: f.lines.map((l, i) => ({ ...l, lineNo: i + 1 })),
  };
}

export function JournalFormFields({
  data,
  onChange,
}: {
  data: JournalFormData;
  onChange: (d: JournalFormData) => void;
}) {
  const set = <K extends keyof JournalFormData>(k: K, v: JournalFormData[K]) =>
    onChange({ ...data, [k]: v });

  const updateLine = (idx: number, patch: Partial<ErpJournalLine>) => {
    const next = data.lines.map((l, i) => (i === idx ? { ...l, ...patch } : l));
    onChange({ ...data, lines: next });
  };

  const addLine = () => {
    onChange({ ...data, lines: [...data.lines, emptyLine(data.lines.length + 1)] });
  };

  const removeLine = (idx: number) => {
    onChange({ ...data, lines: data.lines.filter((_, i) => i !== idx) });
  };

  const totalDebit = data.lines.reduce((s, l) => s + Number(l.debit || 0), 0);
  const totalCredit = data.lines.reduce((s, l) => s + Number(l.credit || 0), 0);

  return (
    <div className="p-4">
      <FormField label="No. Dokumen" htmlFor="je-doc" required>
        <Input id="je-doc" value={data.docNumber} onChange={(e) => set('docNumber', e.target.value)} placeholder="JV-2026-000001" />
      </FormField>
      <FormField label="Tipe Jurnal" htmlFor="je-type" required>
        <Select value={data.journalType} onValueChange={(v) => set('journalType', v as ErpJournalType)}>
          <SelectTrigger id="je-type"><SelectValue /></SelectTrigger>
          <SelectContent>
            {JOURNAL_TYPES.map((t) => <SelectItem key={t} value={t}>{t}</SelectItem>)}
          </SelectContent>
        </Select>
      </FormField>
      <FormField label="Tanggal" htmlFor="je-date" required>
        <DateInput id="je-date" value={data.entryDate} onChange={(v) => set('entryDate', v)} />
      </FormField>
      <FormField label="Branch ID" htmlFor="je-branch" required>
        <Input id="je-branch" value={data.branchId} onChange={(e) => set('branchId', e.target.value)} />
      </FormField>
      <FormField label="Fiscal Period ID" htmlFor="je-fp" required>
        <Input id="je-fp" value={data.fiscalPeriodId} onChange={(e) => set('fiscalPeriodId', e.target.value)} />
      </FormField>
      <FormField label="Currency ID" htmlFor="je-cur" required>
        <Input id="je-cur" value={data.currencyId} onChange={(e) => set('currencyId', e.target.value)} />
      </FormField>
      <FormField label="Kurs" htmlFor="je-rate" required>
        <Input id="je-rate" value={data.exchangeRate} onChange={(e) => set('exchangeRate', e.target.value)} />
      </FormField>
      <FormField label="Deskripsi" htmlFor="je-desc" required>
        <Input id="je-desc" value={data.description} onChange={(e) => set('description', e.target.value)} />
      </FormField>
      <FormField label="Catatan" htmlFor="je-notes">
        <Input id="je-notes" value={data.notes} onChange={(e) => set('notes', e.target.value)} />
      </FormField>

      <div style={{ marginTop: 16 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
          <strong>Baris Jurnal</strong>
          <button className="btn sm" type="button" onClick={addLine}>+ Tambah Baris</button>
        </div>
        <table style={{ width: '100%', fontSize: '0.85em' }}>
          <thead>
            <tr>
              <th style={{ textAlign: 'left' }}>No</th>
              <th style={{ textAlign: 'left' }}>Account ID</th>
              <th style={{ textAlign: 'left' }}>Debit</th>
              <th style={{ textAlign: 'left' }}>Credit</th>
              <th style={{ textAlign: 'left' }}>Notes</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {data.lines.length === 0 ? (
              <tr><td colSpan={6} style={{ color: '#888', padding: 8 }}>Belum ada baris. Tambah baris untuk mengisi jurnal.</td></tr>
            ) : (
              data.lines.map((l, i) => (
                <tr key={i}>
                  <td>{i + 1}</td>
                  <td><Input value={l.accountId} onChange={(e) => updateLine(i, { accountId: e.target.value })} /></td>
                  <td><Input value={l.debit} onChange={(e) => updateLine(i, { debit: e.target.value })} /></td>
                  <td><Input value={l.credit} onChange={(e) => updateLine(i, { credit: e.target.value })} /></td>
                  <td><Input value={l.notes ?? ''} onChange={(e) => updateLine(i, { notes: e.target.value })} /></td>
                  <td><button className="btn sm danger" type="button" onClick={() => removeLine(i)}>×</button></td>
                </tr>
              ))
            )}
          </tbody>
          <tfoot>
            <tr>
              <td colSpan={2} style={{ textAlign: 'right', fontWeight: 600 }}>Total</td>
              <td className="mono">{totalDebit.toFixed(4)}</td>
              <td className="mono">{totalCredit.toFixed(4)}</td>
              <td colSpan={2} className="muted">
                {totalDebit === totalCredit ? 'Balanced' : `Selisih ${(totalDebit - totalCredit).toFixed(4)}`}
              </td>
            </tr>
          </tfoot>
        </table>
      </div>
    </div>
  );
}

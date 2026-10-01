'use client';

/**
 * Invoice Swap (SIE) — simple header + swap-lines form.
 * Not based on SalesTransactionForm — lines reference invoices, not items.
 * Atomic tier: Page (organism-level form section).
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import type {
  ErpSlsInvoiceSwap,
  ErpSlsInvoiceSwapLine,
  CreateSlsInvoiceSwapPayload,
} from '@/lib/api/sls-invoice-swaps';

// ─── form data ────────────────────────────────────────────────────────────────

export interface SieFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  branchId: string;
  docDate: string;
  customerId: string;
  currencyId: string;
  exchangeRate: string;
  description: string;
  notes: string;
  lines: SieLineRow[];
}

export interface SieLineRow {
  key: string; // local stable identity (uuid-like)
  lineNo: number;
  fromInvoiceId: string;
  fromInvoiceLabel: string; // display resolved label (code / number)
  toInvoiceId: string;
  toInvoiceLabel: string;
  amount: string;
}

let _seq = 0;
const nextKey = () => `sie-${++_seq}`;

function emptyLine(lineNo: number): SieLineRow {
  return {
    key: nextKey(),
    lineNo,
    fromInvoiceId: '',
    fromInvoiceLabel: '',
    toInvoiceId: '',
    toInvoiceLabel: '',
    amount: '0',
  };
}

export function defaultSieForm(): SieFormData {
  return {
    docNumber: '',
    auto: true,
    branchId: '',
    docDate: new Date().toISOString().slice(0, 10),
    customerId: '',
    currencyId: '',
    exchangeRate: '1',
    description: '',
    notes: '',
    lines: [emptyLine(1)],
  };
}

export function fromSlsInvoiceSwap(r: ErpSlsInvoiceSwap): SieFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: false,
    branchId: r.branchId,
    docDate: r.docDate.slice(0, 10),
    customerId: r.customerId ?? '',
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    description: r.description ?? '',
    notes: r.notes ?? '',
    lines: r.lines.map((l) => ({
      key: l.id ?? nextKey(),
      lineNo: l.lineNo,
      fromInvoiceId: l.fromInvoiceId ?? '',
      fromInvoiceLabel: l.fromInvoice?.code ?? l.fromInvoiceId ?? '',
      toInvoiceId: l.toInvoiceId ?? '',
      toInvoiceLabel: l.toInvoice?.code ?? l.toInvoiceId ?? '',
      amount: l.amount ?? '0',
    })),
  };
}

export function toSiePayload(f: SieFormData): CreateSlsInvoiceSwapPayload {
  return {
    docNumber: f.auto ? undefined : f.docNumber || undefined,
    auto: f.auto,
    branchId: f.branchId,
    docDate: f.docDate,
    customerId: f.customerId || undefined,
    currencyId: f.currencyId,
    exchangeRate: f.exchangeRate || undefined,
    description: f.description || undefined,
    notes: f.notes || undefined,
    lines: f.lines
      .filter((l) => l.fromInvoiceId || l.toInvoiceId)
      .map((l) => ({
        lineNo: l.lineNo,
        fromInvoiceId: l.fromInvoiceId || undefined,
        toInvoiceId: l.toInvoiceId || undefined,
        amount: l.amount || undefined,
      })),
  };
}

// ─── sub-components ───────────────────────────────────────────────────────────

function SieLineGrid({
  lines,
  onChange,
}: {
  lines: SieLineRow[];
  onChange: (lines: SieLineRow[]) => void;
}) {
  const update = (key: string, patch: Partial<SieLineRow>) =>
    onChange(lines.map((l) => (l.key === key ? { ...l, ...patch } : l)));

  const addLine = () =>
    onChange([...lines, emptyLine(lines.length + 1)]);

  const removeLine = (key: string) => {
    const next = lines
      .filter((l) => l.key !== key)
      .map((l, i) => ({ ...l, lineNo: i + 1 }));
    onChange(next.length ? next : [emptyLine(1)]);
  };

  return (
    <div className="mt-4">
      <div className="flex items-center justify-between mb-2">
        <span className="text-sm font-medium">Baris Swap</span>
        <button type="button" className="btn ghost sm" onClick={addLine}>
          + Tambah Baris
        </button>
      </div>
      <div className="overflow-x-auto rounded border border-border">
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
          <thead>
            <tr style={{ background: 'var(--muted)', borderBottom: '1px solid var(--border)' }}>
              <th style={{ padding: '6px 8px', width: 36, textAlign: 'center' }}>#</th>
              <th style={{ padding: '6px 8px', textAlign: 'left' }}>Invoice Asal (From)</th>
              <th style={{ padding: '6px 8px', textAlign: 'left' }}>Invoice Tujuan (To)</th>
              <th style={{ padding: '6px 8px', textAlign: 'right', minWidth: 120 }}>Jumlah</th>
              <th style={{ width: 36 }} />
            </tr>
          </thead>
          <tbody>
            {lines.map((l) => (
              <tr key={l.key} style={{ borderBottom: '1px solid var(--border)' }}>
                <td style={{ padding: '4px 8px', textAlign: 'center', color: 'var(--muted-foreground)' }}>
                  {l.lineNo}
                </td>
                <td style={{ padding: '4px 8px' }}>
                  <Input
                    value={l.fromInvoiceId}
                    onChange={(e) => update(l.key, { fromInvoiceId: e.target.value })}
                    placeholder="ID / No Invoice asal…"
                    style={{ fontSize: 12 }}
                  />
                </td>
                <td style={{ padding: '4px 8px' }}>
                  <Input
                    value={l.toInvoiceId}
                    onChange={(e) => update(l.key, { toInvoiceId: e.target.value })}
                    placeholder="ID / No Invoice tujuan…"
                    style={{ fontSize: 12 }}
                  />
                </td>
                <td style={{ padding: '4px 8px' }}>
                  <Input
                    value={l.amount}
                    onChange={(e) => update(l.key, { amount: e.target.value })}
                    style={{ textAlign: 'right', fontSize: 12 }}
                    placeholder="0"
                  />
                </td>
                <td style={{ padding: '4px 8px', textAlign: 'center' }}>
                  <button
                    type="button"
                    className="btn ghost sm danger"
                    onClick={() => removeLine(l.key)}
                    title="Hapus baris"
                  >
                    ×
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

// ─── main form ────────────────────────────────────────────────────────────────

export interface SieFormProps {
  data: SieFormData;
  onChange: (d: SieFormData) => void;
  saving: boolean;
  onSave: () => void;
  onReset?: () => void;
}

export function SlsInvoiceSwapForm({ data, onChange, saving, onSave, onReset }: SieFormProps) {
  const set = (k: keyof SieFormData, v: unknown) =>
    onChange({ ...data, [k]: v });

  return (
    <div className="flex flex-col gap-4 max-w-4xl">
      {/* Header — two-column grid */}
      <div className="grid grid-cols-2 gap-x-6 gap-y-3">
        {/* Left column */}
        <FormField label="Pelanggan" htmlFor="sie-customer">
          <Input
            id="sie-customer"
            value={data.customerId}
            onChange={(e) => set('customerId', e.target.value)}
            placeholder="ID Pelanggan…"
          />
        </FormField>

        {/* Right column — Tanggal (top-right, §2.36) */}
        <FormField label="Tanggal *" htmlFor="sie-date">
          <DateInput id="sie-date" value={data.docDate} onChange={(v) => set('docDate', v)} />
        </FormField>

        <FormField label="Cabang *" htmlFor="sie-branch">
          <Input
            id="sie-branch"
            value={data.branchId}
            onChange={(e) => set('branchId', e.target.value)}
            placeholder="ID Cabang…"
          />
        </FormField>

        {/* No Transaksi + Auto checkbox */}
        <FormField label="No Transaksi" htmlFor="sie-docnum">
          <div className="flex items-center gap-2">
            <Input
              id="sie-docnum"
              value={data.docNumber}
              onChange={(e) => set('docNumber', e.target.value)}
              placeholder="SIE-2026-000001"
              disabled={data.auto}
              style={{ flex: 1 }}
            />
            <label className="flex items-center gap-1 text-xs cursor-pointer whitespace-nowrap">
              <input
                type="checkbox"
                checked={data.auto}
                onChange={(e) => set('auto', e.target.checked)}
              />
              Auto
            </label>
          </div>
        </FormField>

        <FormField label="Mata Uang" htmlFor="sie-currency">
          <Input
            id="sie-currency"
            value={data.currencyId}
            onChange={(e) => set('currencyId', e.target.value)}
            placeholder="ID Mata Uang…"
          />
        </FormField>

        <FormField label="Kurs" htmlFor="sie-rate">
          <Input
            id="sie-rate"
            value={data.exchangeRate}
            onChange={(e) => set('exchangeRate', e.target.value)}
            placeholder="1"
          />
        </FormField>

        <FormField label="Uraian" htmlFor="sie-desc" className="col-span-2">
          <Input
            id="sie-desc"
            value={data.description}
            onChange={(e) => set('description', e.target.value)}
            placeholder="Keterangan dokumen…"
          />
        </FormField>
      </div>

      {/* Swap lines grid */}
      <SieLineGrid
        lines={data.lines}
        onChange={(lines) => set('lines', lines)}
      />

      {/* Notes */}
      <FormField label="Catatan" htmlFor="sie-notes">
        <Input
          id="sie-notes"
          value={data.notes}
          onChange={(e) => set('notes', e.target.value)}
          placeholder="Catatan tambahan…"
        />
      </FormField>

      {/* Action row */}
      <div className="flex items-center gap-2 pt-2">
        <button type="button" className="btn primary" onClick={onSave} disabled={saving}>
          {saving ? 'Menyimpan…' : 'Simpan'}
        </button>
        {onReset && (
          <button type="button" className="btn ghost" onClick={onReset}>
            Atur ulang
          </button>
        )}
      </div>
    </div>
  );
}

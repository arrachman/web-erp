'use client';

/**
 * Admin tool page — Repost Journal Entries.
 * Triggers server-side re-processing of accounting postings for a period/module.
 * Atomic tier: Page.
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
import { notify } from '@/lib/feedback';
import {
  runRepostJournal,
  type RepostJournalParams,
  type RepostJournalResult,
} from '@/lib/api/tools';

// ─── Constants ────────────────────────────────────────────────────────────────

const MODULES = [
  { value: '', label: 'Semua Modul' },
  { value: 'finance', label: 'Finance' },
  { value: 'inventory', label: 'Inventory' },
  { value: 'purchasing', label: 'Purchasing' },
  { value: 'sales', label: 'Sales' },
  { value: 'production', label: 'Production' },
];

// ─── Result display ────────────────────────────────────────────────────────────

function RepostResult({ result }: { result: RepostJournalResult }) {
  return (
    <div className="card p-4 mt-4">
      <p className="text-sm font-semibold mb-3">Hasil Repost</p>
      <div style={{ display: 'flex', gap: 24, flexWrap: 'wrap' }}>
        <Stat label="Diproses" value={result.processed} />
        <Stat label="Diposting" value={result.posted} />
        <Stat label="Error" value={result.errors} danger={result.errors > 0} />
      </div>
      {result.message && (
        <p className="text-sm text-muted mt-3">{result.message}</p>
      )}
    </div>
  );
}

function Stat({
  label,
  value,
  danger,
}: {
  label: string;
  value: number;
  danger?: boolean;
}) {
  return (
    <div>
      <p className="text-xs text-muted">{label}</p>
      <p
        className="text-2xl font-semibold mono"
        style={{ color: danger && value > 0 ? 'var(--danger)' : undefined }}
      >
        {value}
      </p>
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpRepostJournalPage() {
  const [params, setParams] = React.useState<RepostJournalParams>({
    fiscalPeriodId: '',
    fromDate: '',
    toDate: '',
    module: '',
  });
  const [running, setRunning] = React.useState(false);
  const [result, setResult] = React.useState<RepostJournalResult | null>(null);

  const set = (k: keyof RepostJournalParams, v: string) =>
    setParams((prev) => ({ ...prev, [k]: v }));

  const handleRun = async () => {
    setRunning(true);
    setResult(null);
    try {
      const clean: RepostJournalParams = {
        fiscalPeriodId: params.fiscalPeriodId || undefined,
        fromDate: params.fromDate || undefined,
        toDate: params.toDate || undefined,
        module: params.module || undefined,
      };
      const res = await runRepostJournal(clean);
      setResult(res.data);
      notify('Repost jurnal selesai', 'success');
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menjalankan repost', 'danger');
    } finally {
      setRunning(false);
    }
  };

  return (
    <div className="p-6 max-w-2xl">
      <h2 className="text-lg font-semibold mb-2">Repost Journal Entries</h2>
      <p className="text-sm text-muted mb-6">
        Repost Journal Entries memproses ulang posting akuntansi untuk periode
        atau modul yang dipilih. Gunakan setelah melakukan koreksi data untuk
        memastikan konsistensi buku besar.
      </p>

      <div className="card p-4 mb-4">
        <FormField label="Fiscal Period ID" htmlFor="rj-period">
          <Input
            id="rj-period"
            value={params.fiscalPeriodId ?? ''}
            onChange={(e) => set('fiscalPeriodId', e.target.value)}
            placeholder="Kosongkan untuk semua periode"
          />
        </FormField>

        <div style={{ display: 'flex', gap: 12 }}>
          <div style={{ flex: 1 }}>
            <FormField label="Dari Tanggal" htmlFor="rj-from">
              <DateInput
                id="rj-from"
                value={params.fromDate ?? ''}
                onChange={(v) => set('fromDate', v)}
              />
            </FormField>
          </div>
          <div style={{ flex: 1 }}>
            <FormField label="Sampai Tanggal" htmlFor="rj-to">
              <DateInput
                id="rj-to"
                value={params.toDate ?? ''}
                onChange={(v) => set('toDate', v)}
              />
            </FormField>
          </div>
        </div>

        <FormField label="Modul" htmlFor="rj-module">
          <Select
            value={params.module ?? ''}
            onValueChange={(v) => set('module', v)}
          >
            <SelectTrigger id="rj-module">
              <SelectValue placeholder="Semua Modul" />
            </SelectTrigger>
            <SelectContent>
              {MODULES.map((m) => (
                <SelectItem key={m.value} value={m.value}>
                  {m.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </FormField>
      </div>

      <button
        className="btn primary"
        onClick={handleRun}
        disabled={running}
      >
        {running ? 'Menjalankan...' : 'Repost Journal'}
      </button>

      {result && <RepostResult result={result} />}
    </div>
  );
}

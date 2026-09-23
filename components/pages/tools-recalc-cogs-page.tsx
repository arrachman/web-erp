'use client';

/**
 * Admin tool page — Recalculate COGS.
 * Allows admins to trigger a server-side COGS recalculation for a period/date range.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { notify } from '@/lib/feedback';
import {
  runRecalcCogs,
  type RecalcCogsParams,
  type RecalcCogsResult,
} from '@/lib/api/tools';

// ─── Result display ────────────────────────────────────────────────────────────

function RecalcResult({ result }: { result: RecalcCogsResult }) {
  return (
    <div className="card p-4 mt-4">
      <p className="text-sm font-semibold mb-3">Hasil Kalkulasi</p>
      <div style={{ display: 'flex', gap: 24, flexWrap: 'wrap' }}>
        <Stat label="Diproses" value={result.processed} />
        <Stat label="Diperbarui" value={result.updated} />
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

export function ErpRecalcCogsPage() {
  const [params, setParams] = React.useState<RecalcCogsParams>({
    fiscalPeriodId: '',
    fromDate: '',
    toDate: '',
  });
  const [running, setRunning] = React.useState(false);
  const [result, setResult] = React.useState<RecalcCogsResult | null>(null);

  const set = (k: keyof RecalcCogsParams, v: string) =>
    setParams((prev) => ({ ...prev, [k]: v }));

  const handleRun = async () => {
    setRunning(true);
    setResult(null);
    try {
      const clean: RecalcCogsParams = {
        fiscalPeriodId: params.fiscalPeriodId || undefined,
        fromDate: params.fromDate || undefined,
        toDate: params.toDate || undefined,
      };
      const res = await runRecalcCogs(clean);
      setResult(res.data);
      notify('Kalkulasi COGS selesai', 'success');
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menjalankan kalkulasi', 'danger');
    } finally {
      setRunning(false);
    }
  };

  return (
    <div className="p-6 max-w-2xl">
      <h2 className="text-lg font-semibold mb-2">Recalculate COGS</h2>
      <p className="text-sm text-muted mb-6">
        Recalculate COGS menghitung ulang harga pokok penjualan untuk seluruh
        pergerakan persediaan berdasarkan metode yang dikonfigurasi. Gunakan
        setelah koreksi data atau perubahan metode valuasi.
      </p>

      <div className="card p-4 mb-4">
        <FormField label="Fiscal Period ID" htmlFor="rc-period">
          <Input
            id="rc-period"
            value={params.fiscalPeriodId ?? ''}
            onChange={(e) => set('fiscalPeriodId', e.target.value)}
            placeholder="Kosongkan untuk semua periode"
          />
        </FormField>
        <div style={{ display: 'flex', gap: 12 }}>
          <div style={{ flex: 1 }}>
            <FormField label="Dari Tanggal" htmlFor="rc-from">
              <DateInput
                id="rc-from"
                value={params.fromDate ?? ''}
                onChange={(v) => set('fromDate', v)}
              />
            </FormField>
          </div>
          <div style={{ flex: 1 }}>
            <FormField label="Sampai Tanggal" htmlFor="rc-to">
              <DateInput
                id="rc-to"
                value={params.toDate ?? ''}
                onChange={(v) => set('toDate', v)}
              />
            </FormField>
          </div>
        </div>
      </div>

      <button
        className="btn primary"
        onClick={handleRun}
        disabled={running}
      >
        {running ? 'Menjalankan...' : 'Jalankan Kalkulasi'}
      </button>

      {result && <RecalcResult result={result} />}
    </div>
  );
}

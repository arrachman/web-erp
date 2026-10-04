'use client';

/**
 * D2 — Three-Way Match panel on the Purchase Invoice form: compares PO
 * lines vs goods receipts vs this invoice, shows per-line issues, and can
 * persist the computed status (MATCHED / MISMATCH / PENDING) to the
 * invoice's matchStatus.
 */

import * as React from 'react';
import { toast } from 'sonner';
import {
  getPurInvoiceMatch,
  recomputePurInvoiceMatch,
  type PurInvoiceMatchResult,
} from '@/lib/api/pur-invoices';

const STATUS_STYLE: Record<string, string> = {
  MATCHED: 'bg-emerald-100 text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-200',
  MISMATCH: 'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-200',
  PENDING: 'bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-200',
  WAIVED: 'bg-slate-200 text-slate-700 dark:bg-slate-800 dark:text-slate-200',
};

function StatusChip({ status }: { status: string }) {
  return (
    <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_STYLE[status] ?? STATUS_STYLE.PENDING}`}>
      {status}
    </span>
  );
}

export function PurInvoiceMatchPanel({ invoiceId }: { invoiceId: string }) {
  const [result, setResult] = React.useState<PurInvoiceMatchResult | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [saving, setSaving] = React.useState(false);

  const load = React.useCallback(async () => {
    setLoading(true);
    try {
      setResult(await getPurInvoiceMatch(invoiceId));
    } catch {
      setResult(null);
    } finally {
      setLoading(false);
    }
  }, [invoiceId]);

  React.useEffect(() => { load(); }, [load]);

  const persist = async () => {
    setSaving(true);
    try {
      const res = await recomputePurInvoiceMatch(invoiceId);
      setResult(res);
      toast.success(`Status three-way match tersimpan: ${res.computedStatus}`);
    } catch {
      toast.error('Gagal menyimpan status match');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="mt-4 rounded-lg border bg-card p-4">
      <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="text-sm font-semibold">Three-Way Match</span>
          <span className="text-xs text-muted-foreground">PO ↔ Penerimaan ↔ Invoice</span>
          {result && <StatusChip status={result.computedStatus} />}
          {result && result.persistedStatus !== result.computedStatus && (
            <span className="text-xs text-muted-foreground">
              (tersimpan: {result.persistedStatus})
            </span>
          )}
        </div>
        <button
          type="button"
          onClick={persist}
          disabled={saving || loading || !result}
          className="rounded-md bg-primary px-3 py-1.5 text-xs font-medium text-primary-foreground disabled:opacity-50"
        >
          {saving ? 'Menyimpan…' : 'Hitung Ulang & Simpan Status'}
        </button>
      </div>
      {loading ? (
        <p className="text-sm text-muted-foreground">Memeriksa kesesuaian PO, penerimaan, dan invoice…</p>
      ) : !result ? (
        <p className="text-sm text-muted-foreground">Data match tidak tersedia.</p>
      ) : result.lines.length === 0 ? (
        <p className="text-sm text-muted-foreground">Invoice belum memiliki baris.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-xs text-muted-foreground">
                <th className="py-2 pr-3">Item</th>
                <th className="py-2 pr-3 text-right">Qty PO</th>
                <th className="py-2 pr-3 text-right">Diterima</th>
                <th className="py-2 pr-3 text-right">Ditagih</th>
                <th className="py-2 pr-3 text-right">Harga PO</th>
                <th className="py-2 pr-3 text-right">Harga Invoice</th>
                <th className="py-2">Keterangan</th>
              </tr>
            </thead>
            <tbody>
              {result.lines.map((l) => (
                <tr key={l.lineId} className="border-b last:border-0 align-top">
                  <td className="py-2 pr-3">{l.itemLabel}</td>
                  <td className="py-2 pr-3 text-right">{l.orderedQty ?? '—'}</td>
                  <td className="py-2 pr-3 text-right">{l.receivedQty ?? '—'}</td>
                  <td className="py-2 pr-3 text-right">{l.invoicedQty}</td>
                  <td className="py-2 pr-3 text-right">{l.orderPrice ?? '—'}</td>
                  <td className="py-2 pr-3 text-right">{l.invoicePrice}</td>
                  <td className="py-2 text-xs">
                    {l.issues.length === 0 ? (
                      <span className="text-emerald-600">Sesuai</span>
                    ) : (
                      l.issues.map((msg, i) => <div key={i} className="text-red-600">{msg}</div>)
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

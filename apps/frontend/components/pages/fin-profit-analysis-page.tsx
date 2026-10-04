'use client';

/**
 * E1 — Analisis Laba Kotor per sekolah / produk / wilayah: pendapatan
 * bersih vs HPP dari invoice penjualan ter-posting dikurangi retur
 * ter-posting pada periode terpilih.
 */

import * as React from 'react';
import {
  getProfitAnalysis,
  type ProfitAnalysisResult,
  type ProfitDimension,
} from '@/lib/api/fin-reports';

const inputCls =
  'rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-2 focus:ring-ring';
const rp = (v: number) =>
  new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', maximumFractionDigits: 0 }).format(v);

const DIMENSIONS: { key: ProfitDimension; label: string }[] = [
  { key: 'SCHOOL', label: 'Per Sekolah' },
  { key: 'PRODUCT', label: 'Per Produk' },
  { key: 'REGION', label: 'Per Wilayah' },
];

export function ErpFinProfitAnalysisPage() {
  const now = new Date();
  const [dimension, setDimension] = React.useState<ProfitDimension>('SCHOOL');
  const [from, setFrom] = React.useState(`${now.getFullYear()}-01-01`);
  const [to, setTo] = React.useState(now.toISOString().slice(0, 10));
  const [data, setData] = React.useState<ProfitAnalysisResult | null>(null);
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState('');

  const load = React.useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      setData(await getProfitAnalysis({ from, to, dimension }));
    } catch {
      setError('Gagal memuat analisis laba');
    } finally {
      setLoading(false);
    }
  }, [from, to, dimension]);

  React.useEffect(() => { load(); }, [load]);

  const totals = data?.totals;
  return (
    <div className="flex flex-col gap-4 p-4">
      <div>
        <h1 className="text-lg font-semibold">Analisis Laba Kotor</h1>
        <p className="text-sm text-muted-foreground">
          Pendapatan bersih vs HPP dari invoice penjualan ter-posting, dikurangi retur ter-posting.
        </p>
      </div>

      <div className="flex flex-wrap items-end gap-3">
        <div className="flex overflow-hidden rounded-md border">
          {DIMENSIONS.map((d) => (
            <button
              key={d.key}
              type="button"
              onClick={() => setDimension(d.key)}
              className={`px-3 py-2 text-sm ${dimension === d.key ? 'bg-primary text-primary-foreground' : 'bg-background'}`}
            >
              {d.label}
            </button>
          ))}
        </div>
        <label className="flex flex-col gap-1 text-xs text-muted-foreground">
          Dari
          <input type="date" className={inputCls} value={from} onChange={(e) => setFrom(e.target.value)} />
        </label>
        <label className="flex flex-col gap-1 text-xs text-muted-foreground">
          Sampai
          <input type="date" className={inputCls} value={to} onChange={(e) => setTo(e.target.value)} />
        </label>
        {loading && <span className="pb-2 text-sm text-muted-foreground">Memuat…</span>}
        {error && <span className="pb-2 text-sm text-destructive">{error}</span>}
      </div>

      {totals && (
        <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
          {[
            { label: 'Pendapatan Bersih', value: rp(totals.revenue) },
            { label: 'HPP', value: rp(totals.cogs) },
            { label: 'Laba Kotor', value: rp(totals.grossProfit) },
            { label: 'Margin', value: `${totals.marginPercent}%` },
          ].map((c) => (
            <div key={c.label} className="rounded-lg border bg-card p-3">
              <div className="text-xs text-muted-foreground">{c.label}</div>
              <div className="mt-1 text-base font-semibold">{c.value}</div>
            </div>
          ))}
        </div>
      )}

      <div className="overflow-x-auto rounded-lg border bg-card">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b text-left text-xs text-muted-foreground">
              <th className="px-3 py-2">{DIMENSIONS.find((d) => d.key === dimension)?.label.replace('Per ', '')}</th>
              <th className="px-3 py-2 text-right">Dokumen</th>
              <th className="px-3 py-2 text-right">Pendapatan</th>
              <th className="px-3 py-2 text-right">HPP</th>
              <th className="px-3 py-2 text-right">Laba Kotor</th>
              <th className="px-3 py-2 text-right">Margin</th>
            </tr>
          </thead>
          <tbody>
            {(data?.rows ?? []).map((r) => (
              <tr key={r.key} className="border-b last:border-0">
                <td className="px-3 py-2">{r.label}</td>
                <td className="px-3 py-2 text-right">{r.docCount}</td>
                <td className="px-3 py-2 text-right">{rp(r.revenue)}</td>
                <td className="px-3 py-2 text-right">{rp(r.cogs)}</td>
                <td className="px-3 py-2 text-right font-medium">{rp(r.grossProfit)}</td>
                <td className="px-3 py-2 text-right">{r.marginPercent}%</td>
              </tr>
            ))}
            {(data?.rows ?? []).length === 0 && !loading && (
              <tr>
                <td colSpan={6} className="px-3 py-6 text-center text-muted-foreground">
                  Tidak ada invoice ter-posting pada periode ini.
                </td>
              </tr>
            )}
          </tbody>
          {totals && (data?.rows ?? []).length > 0 && (
            <tfoot>
              <tr className="border-t font-semibold">
                <td className="px-3 py-2">Total</td>
                <td className="px-3 py-2 text-right">{totals.docCount}</td>
                <td className="px-3 py-2 text-right">{rp(totals.revenue)}</td>
                <td className="px-3 py-2 text-right">{rp(totals.cogs)}</td>
                <td className="px-3 py-2 text-right">{rp(totals.grossProfit)}</td>
                <td className="px-3 py-2 text-right">{totals.marginPercent}%</td>
              </tr>
            </tfoot>
          )}
        </table>
      </div>
    </div>
  );
}

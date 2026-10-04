'use client';

/**
 * Fase 2 P5 — Laporan HPP & Laba Job: per job cetak, estimasi (P1) vs biaya
 * aktual (HPP), varians, dan margin terhadap harga estimasi. Panel inilah
 * yang membuka "laba per job cetak" untuk analisis E1.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { notify } from '@/lib/feedback';
import { formatRupiah } from '@/lib/format';
import { getJobCostReport, type JobCostReportRow } from '@/lib/api/mfg-job-costs';
import { PRINT_JOB_STAGE_LABELS, type PrintJobStage } from '@/lib/api/mfg-print-jobs';

const rp = (v: string | number | null | undefined) =>
  v == null ? '—' : formatRupiah(Number(v) || 0);

export function MfgJobCostsPage() {
  const [rows, setRows] = React.useState<JobCostReportRow[]>([]);
  const [loading, setLoading] = React.useState(false);

  const load = React.useCallback(async () => {
    setLoading(true);
    try {
      setRows(await getJobCostReport());
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat laporan HPP.', 'danger');
    } finally {
      setLoading(false);
    }
  }, []);

  React.useEffect(() => {
    void load();
  }, [load]);

  const totEst = rows.reduce((s, r) => s + (Number(r.estimateTotalCost) || 0), 0);
  const totAct = rows.reduce((s, r) => s + (Number(r.actualTotal) || 0), 0);
  const totMargin = rows.reduce((s, r) => s + (Number(r.marginVsEstimatePrice) || 0), 0);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-semibold">HPP Job</h1>
          <p className="text-sm text-muted-foreground">
            Biaya aktual per job cetak vs estimasi, varians, dan margin terhadap harga estimasi.
          </p>
        </div>
        <Button variant="ghost" onClick={() => void load()}>
          Muat Ulang
        </Button>
      </div>

      <div className="grid grid-cols-3 gap-3">
        <Card className="p-3">
          <div className="text-xs text-muted-foreground">Total estimasi biaya</div>
          <div className="text-lg font-semibold">{rp(totEst)}</div>
        </Card>
        <Card className="p-3">
          <div className="text-xs text-muted-foreground">Total aktual (HPP)</div>
          <div className="text-lg font-semibold">{rp(totAct)}</div>
        </Card>
        <Card className="p-3">
          <div className="text-xs text-muted-foreground">Total margin vs harga estimasi</div>
          <div className="text-lg font-semibold">{rp(totMargin)}</div>
        </Card>
      </div>

      <Card className="p-0">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b text-left text-xs uppercase text-muted-foreground">
              <th className="px-3 py-2">No WO</th>
              <th className="px-3 py-2">Pekerjaan</th>
              <th className="px-3 py-2">Tahap</th>
              <th className="px-3 py-2 text-right">Estimasi Biaya</th>
              <th className="px-3 py-2 text-right">Harga Estimasi</th>
              <th className="px-3 py-2 text-right">Aktual (HPP)</th>
              <th className="px-3 py-2 text-right">Varians</th>
              <th className="px-3 py-2 text-right">Margin</th>
              <th className="px-3 py-2">Jurnal</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => {
              const variance = r.varianceVsEstimate != null ? Number(r.varianceVsEstimate) : null;
              return (
                <tr key={r.jobId} className="border-b hover:bg-muted/40">
                  <td className="px-3 py-2 font-medium">{r.workOrderDocNumber ?? '—'}</td>
                  <td className="px-3 py-2">{r.title ?? '—'}</td>
                  <td className="px-3 py-2">
                    {PRINT_JOB_STAGE_LABELS[r.stage as PrintJobStage] ?? r.stage}
                  </td>
                  <td className="px-3 py-2 text-right">{rp(r.estimateTotalCost)}</td>
                  <td className="px-3 py-2 text-right">{rp(r.estimateTotalPrice)}</td>
                  <td className="px-3 py-2 text-right">{rp(r.actualTotal)}</td>
                  <td className={`px-3 py-2 text-right ${variance != null && variance > 0 ? 'text-red-600' : 'text-green-700'}`}>
                    {rp(r.varianceVsEstimate)}
                  </td>
                  <td className="px-3 py-2 text-right">{rp(r.marginVsEstimatePrice)}</td>
                  <td className="px-3 py-2">
                    {r.costJournalDoc ? (
                      <Badge variant="success">{r.costJournalDoc}</Badge>
                    ) : (
                      <Badge variant="default">Belum</Badge>
                    )}
                  </td>
                </tr>
              );
            })}
            {!rows.length && (
              <tr>
                <td colSpan={9} className="px-3 py-8 text-center text-muted-foreground">
                  {loading ? 'Memuat…' : 'Belum ada job cetak.'}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </Card>
    </div>
  );
}

'use client';

/**
 * Fase 2 P5 — Panel Biaya & HPP di detail Job Cetak: entri biaya aktual,
 * ringkasan aktual vs estimasi (varians + margin), dan posting jurnal
 * penyelesaian (Dr Barang Jadi / Cr Barang Dalam Proses).
 */

import * as React from 'react';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { confirmAction, notify } from '@/lib/feedback';
import { formatRupiah } from '@/lib/format';
import {
  JOB_COST_TYPES,
  JOB_COST_TYPE_LABELS,
  createJobCostEntry,
  deleteJobCostEntry,
  getJobCostSummary,
  listJobCostEntries,
  postJobCostJournal,
  voidJobCostJournal,
  type JobCostEntry,
  type JobCostSummary,
  type JobCostType,
} from '@/lib/api/mfg-job-costs';

const rp = (v: string | number | null | undefined) => formatRupiah(Number(v) || 0);
const selectCls = 'h-9 rounded-md border border-input bg-background px-2 text-sm';
const STAGES = ['PRE_PRESS', 'CETAK', 'FINISHING', 'QC'];

export function PrintJobCostsPanel({
  jobId,
  jobStage,
}: {
  jobId: string;
  jobStage: string;
}) {
  const [entries, setEntries] = React.useState<JobCostEntry[]>([]);
  const [summary, setSummary] = React.useState<JobCostSummary | null>(null);
  const [busy, setBusy] = React.useState(false);
  const [entryDate, setEntryDate] = React.useState(() => new Date().toISOString().slice(0, 10));
  const [costType, setCostType] = React.useState<JobCostType>('MATERIAL');
  const [stage, setStage] = React.useState('');
  const [description, setDescription] = React.useState('');
  const [quantity, setQuantity] = React.useState('1');
  const [unitCost, setUnitCost] = React.useState('');

  const load = React.useCallback(async () => {
    try {
      const [e, s] = await Promise.all([listJobCostEntries(jobId), getJobCostSummary(jobId)]);
      setEntries(e);
      setSummary(s);
    } catch (err: any) {
      notify(err?.message ?? 'Gagal memuat biaya job.', 'danger');
    }
  }, [jobId]);

  React.useEffect(() => {
    void load();
  }, [load]);

  const posted = !!summary?.costPostedAt;
  const locked = posted || jobStage === 'CANCELLED';

  const add = async () => {
    if (!unitCost) return notify('Biaya satuan wajib diisi.', 'danger');
    setBusy(true);
    try {
      await createJobCostEntry(jobId, {
        entryDate,
        costType,
        stage: stage || undefined,
        description: description || undefined,
        quantity: quantity || '1',
        unitCost,
      });
      setDescription('');
      setUnitCost('');
      setQuantity('1');
      notify('Entri biaya ditambahkan.', 'success');
      void load();
    } catch (e: any) {
      notify(e?.message ?? 'Gagal menambah biaya.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  const journal = async (which: 'post' | 'void') => {
    setBusy(true);
    try {
      if (which === 'post') await postJobCostJournal(jobId);
      else await voidJobCostJournal(jobId);
      notify(which === 'post' ? 'Jurnal HPP diposting.' : 'Jurnal HPP dibatalkan.', 'success');
      void load();
    } catch (e: any) {
      notify(e?.message ?? 'Aksi jurnal gagal.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  const variance = summary?.varianceVsEstimate != null ? Number(summary.varianceVsEstimate) : null;

  return (
    <Card className="p-4">
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <h3 className="text-sm font-semibold">Biaya &amp; HPP</h3>
        <div className="flex gap-2">
          {jobStage === 'SELESAI' && !posted && (
            <Button disabled={busy} onClick={() => void journal('post')}>
              Posting Jurnal HPP
            </Button>
          )}
          {posted && (
            <Button
              variant="danger"
              disabled={busy}
              onClick={() =>
                confirmAction({
                  message: `Batalkan jurnal ${summary?.costJournalDoc ?? ''}? Baris ledger akan dihapus dan entri biaya terbuka lagi.`,
                  onConfirm: () => void journal('void'),
                })
              }
            >
              Batalkan Jurnal
            </Button>
          )}
        </div>
      </div>

      {summary && (
        <div className="mb-3 grid grid-cols-2 gap-3 text-sm md:grid-cols-4">
          <div>
            <div className="text-xs text-muted-foreground">Estimasi biaya {summary.estimateDocNumber ? `(${summary.estimateDocNumber})` : ''}</div>
            <div className="font-semibold">{summary.estimateTotalCost != null ? rp(summary.estimateTotalCost) : '—'}</div>
          </div>
          <div>
            <div className="text-xs text-muted-foreground">Aktual (HPP)</div>
            <div className="font-semibold">{rp(summary.actualTotal)}</div>
          </div>
          <div>
            <div className="text-xs text-muted-foreground">Varians vs estimasi</div>
            <div className={`font-semibold ${variance != null && variance > 0 ? 'text-red-600' : 'text-green-700'}`}>
              {variance != null
                ? `${rp(variance)}${summary.variancePercent != null ? ` (${Number(summary.variancePercent).toFixed(1)}%)` : ''}`
                : '—'}
            </div>
          </div>
          <div>
            <div className="text-xs text-muted-foreground">Margin vs harga estimasi</div>
            <div className="font-semibold">
              {summary.marginVsEstimatePrice != null ? rp(summary.marginVsEstimatePrice) : '—'}
            </div>
          </div>
          <div>
            <div className="text-xs text-muted-foreground">HPP per eksemplar</div>
            <div className="font-semibold">{rp(summary.actualUnitCost)}</div>
          </div>
          <div>
            <div className="text-xs text-muted-foreground">Status jurnal</div>
            <div className="font-semibold">
              {posted ? `${summary.costJournalDoc} · ${new Date(summary.costPostedAt!).toLocaleString('id-ID')}` : 'Belum diposting'}
            </div>
          </div>
        </div>
      )}

      <table className="w-full text-sm">
        <thead>
          <tr className="border-b text-left text-xs uppercase text-muted-foreground">
            <th className="py-1 pr-2">Tanggal</th>
            <th className="py-1 pr-2">Jenis</th>
            <th className="py-1 pr-2">Tahap</th>
            <th className="py-1 pr-2">Keterangan</th>
            <th className="py-1 pr-2 text-right">Qty</th>
            <th className="py-1 pr-2 text-right">Biaya Satuan</th>
            <th className="py-1 pr-2 text-right">Jumlah</th>
            <th className="w-10" />
          </tr>
        </thead>
        <tbody>
          {entries.map((e) => (
            <tr key={e.id} className="border-b">
              <td className="py-1 pr-2">{e.entryDate}</td>
              <td className="py-1 pr-2">{JOB_COST_TYPE_LABELS[e.costType]}</td>
              <td className="py-1 pr-2">{e.stage ?? '—'}</td>
              <td className="py-1 pr-2">{e.description ?? '—'}</td>
              <td className="py-1 pr-2 text-right">{Number(e.quantity).toLocaleString('id-ID')}</td>
              <td className="py-1 pr-2 text-right">{rp(e.unitCost)}</td>
              <td className="py-1 pr-2 text-right">{rp(e.amount)}</td>
              <td>
                {!locked && (
                  <Button
                    variant="ghost"
                    disabled={busy}
                    onClick={async () => {
                      try {
                        await deleteJobCostEntry(e.id);
                        notify('Entri dihapus.', 'success');
                        void load();
                      } catch (err: any) {
                        notify(err?.message ?? 'Gagal menghapus.', 'danger');
                      }
                    }}
                  >
                    ✕
                  </Button>
                )}
              </td>
            </tr>
          ))}
          {!entries.length && (
            <tr>
              <td colSpan={8} className="py-4 text-center text-muted-foreground">
                Belum ada entri biaya.
              </td>
            </tr>
          )}
        </tbody>
      </table>

      {!locked && (
        <div className="mt-3 flex flex-wrap items-end gap-2 border-t pt-3">
          <Input type="date" className="w-36" value={entryDate} onChange={(e) => setEntryDate(e.target.value)} />
          <select className={selectCls} value={costType} onChange={(e) => setCostType(e.target.value as JobCostType)}>
            {JOB_COST_TYPES.map((t) => (
              <option key={t} value={t}>{JOB_COST_TYPE_LABELS[t]}</option>
            ))}
          </select>
          <select className={selectCls} value={stage} onChange={(e) => setStage(e.target.value)}>
            <option value="">— tahap —</option>
            {STAGES.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
          <Input className="w-56" placeholder="Keterangan" value={description} onChange={(e) => setDescription(e.target.value)} />
          <Input type="number" className="w-24" value={quantity} onChange={(e) => setQuantity(e.target.value)} />
          <Input type="number" className="w-36" placeholder="Biaya satuan" value={unitCost} onChange={(e) => setUnitCost(e.target.value)} />
          <Button disabled={busy} onClick={() => void add()}>
            + Tambah
          </Button>
        </div>
      )}
      {posted && (
        <p className="mt-2 text-xs text-muted-foreground">
          Entri terkunci karena jurnal {summary?.costJournalDoc} sudah diposting (Dr Persediaan Barang Jadi / Cr Persediaan Barang Dalam Proses).
        </p>
      )}
    </Card>
  );
}

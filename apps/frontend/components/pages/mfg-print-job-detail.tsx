'use client';

/**
 * Fase 2 P2 — Detail Job Cetak: stepper tahap, checklist pre-press
 * (gerbang menuju tahap CETAK), log perpindahan tahap, dan spesifikasi.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { notify } from '@/lib/feedback';
import {
  PRINT_JOB_STAGE_FLOW,
  PRINT_JOB_STAGE_LABELS,
  advancePrintJob,
  getPrintJob,
  setPrintJobChecklist,
  type PrintJob,
  type PrintJobStage,
} from '@/lib/api/mfg-print-jobs';

const NEXT: Partial<Record<PrintJobStage, { to: PrintJobStage; label: string }[]>> = {
  PRE_PRESS: [{ to: 'CETAK', label: 'Mulai Cetak →' }],
  CETAK: [{ to: 'FINISHING', label: 'Ke Finishing →' }],
  FINISHING: [{ to: 'QC', label: 'Ke QC →' }],
  QC: [
    { to: 'SELESAI', label: 'Lulus QC — Selesai ✓' },
    { to: 'FINISHING', label: 'Gagal QC — Kembali Finishing' },
  ],
};

export function PrintJobDetail({
  jobId,
  onBack,
}: {
  jobId: string;
  onBack: () => void;
}) {
  const [job, setJob] = React.useState<PrintJob | null>(null);
  const [busy, setBusy] = React.useState(false);

  const load = React.useCallback(async () => {
    try {
      setJob(await getPrintJob(jobId));
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat job.', 'danger');
    }
  }, [jobId]);

  React.useEffect(() => {
    void load();
  }, [load]);

  if (!job) return <p className="text-sm text-muted-foreground">Memuat job…</p>;

  const stageIdx = PRINT_JOB_STAGE_FLOW.indexOf(job.stage);
  const run = async (fn: () => Promise<PrintJob>, ok: string) => {
    setBusy(true);
    try {
      setJob(await fn());
      notify(ok, 'success');
    } catch (e: any) {
      notify(e?.message ?? 'Aksi gagal.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-base font-semibold">
            {job.workOrderDocNumber ?? 'Job Cetak'} — {job.title ?? job.itemName ?? ''}
          </h2>
          <p className="text-sm text-muted-foreground">
            Item hasil: {job.itemName ?? '—'} · Oplah{' '}
            {Number(job.printQuantity).toLocaleString('id-ID')}
            {job.estimateDocNumber ? ` · Dari estimasi ${job.estimateDocNumber}` : ''}
            {job.workOrderStatus ? ` · Status WO: ${job.workOrderStatus}` : ''}
          </p>
        </div>
        <Button variant="ghost" onClick={onBack}>
          ← Kembali
        </Button>
      </div>

      <Card className="p-4">
        <div className="flex flex-wrap items-center gap-2">
          {PRINT_JOB_STAGE_FLOW.map((s, i) => (
            <React.Fragment key={s}>
              <Badge
                variant={
                  job.stage === 'CANCELLED'
                    ? 'default'
                    : i < stageIdx
                      ? 'success'
                      : i === stageIdx
                        ? 'info'
                        : 'default'
                }
              >
                {PRINT_JOB_STAGE_LABELS[s]}
              </Badge>
              {i < PRINT_JOB_STAGE_FLOW.length - 1 && <span className="text-muted-foreground">→</span>}
            </React.Fragment>
          ))}
          {job.stage === 'CANCELLED' && <Badge variant="danger">Dibatalkan</Badge>}
        </div>
        <div className="mt-3 flex flex-wrap gap-2">
          {(NEXT[job.stage] ?? []).map((n) => (
            <Button
              key={n.to}
              disabled={busy}
              onClick={() => run(() => advancePrintJob(job.id, n.to), `Tahap pindah ke ${PRINT_JOB_STAGE_LABELS[n.to]}.`)}
            >
              {n.label}
            </Button>
          ))}
          {job.stage !== 'SELESAI' && job.stage !== 'CANCELLED' && (
            <Button
              variant="danger"
              disabled={busy}
              onClick={() => run(() => advancePrintJob(job.id, 'CANCELLED'), 'Job dibatalkan.')}
            >
              Batalkan Job
            </Button>
          )}
        </div>
      </Card>

      <div className="grid gap-4 md:grid-cols-2">
        <Card className="p-4">
          <h3 className="mb-2 text-sm font-semibold">
            Checklist Pre-press{' '}
            <span className="font-normal text-muted-foreground">
              ({job.checklistProgress.done}/{job.checklistProgress.total})
            </span>
          </h3>
          {job.stage !== 'PRE_PRESS' && (
            <p className="mb-2 text-xs text-muted-foreground">
              Checklist terkunci setelah job meninggalkan tahap pre-press.
            </p>
          )}
          <ul className="space-y-2">
            {job.checklist.map((c) => (
              <li key={c.key} className="flex items-start gap-2 text-sm">
                <input
                  type="checkbox"
                  className="mt-1"
                  checked={c.done}
                  disabled={busy || job.stage !== 'PRE_PRESS'}
                  onChange={(e) =>
                    run(
                      () => setPrintJobChecklist(job.id, c.key, e.target.checked),
                      e.target.checked ? 'Item dicentang.' : 'Item dilepas.',
                    )
                  }
                />
                <span>
                  {c.label}
                  {c.required ? <span className="text-red-500"> *</span> : null}
                  {c.done && c.doneAt ? (
                    <span className="block text-xs text-muted-foreground">
                      {new Date(c.doneAt).toLocaleString('id-ID')}
                    </span>
                  ) : null}
                </span>
              </li>
            ))}
          </ul>
        </Card>

        <Card className="p-4">
          <h3 className="mb-2 text-sm font-semibold">Spesifikasi</h3>
          <dl className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
            <dt className="text-muted-foreground">Ukuran kertas</dt>
            <dd>{job.paperSize ?? '—'}</dd>
            <dt className="text-muted-foreground">Halaman</dt>
            <dd>{job.pageCount ?? '—'}</dd>
            <dt className="text-muted-foreground">Warna</dt>
            <dd>{job.colorSpec ?? '—'}</dd>
            <dt className="text-muted-foreground">Finishing</dt>
            <dd>{job.finishing ?? '—'}</dd>
            <dt className="text-muted-foreground">File master</dt>
            <dd>{job.masterFileName ?? '—'}</dd>
            <dt className="text-muted-foreground">Catatan</dt>
            <dd>{job.notes ?? '—'}</dd>
          </dl>
          <h3 className="mb-2 mt-4 text-sm font-semibold">Log tahap</h3>
          <ul className="space-y-1 text-xs text-muted-foreground">
            {job.stageLog.map((l, i) => (
              <li key={i}>
                {new Date(l.at).toLocaleString('id-ID')} — {l.from ? PRINT_JOB_STAGE_LABELS[l.from as PrintJobStage] ?? l.from : 'Dibuat'} →{' '}
                {PRINT_JOB_STAGE_LABELS[l.to as PrintJobStage] ?? l.to}
              </li>
            ))}
          </ul>
        </Card>
      </div>
    </div>
  );
}

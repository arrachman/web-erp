'use client';

/**
 * Fase 2 P2 — Job Cetak: daftar job (profil cetak di atas Work Order),
 * form pembuatan job (opsional dari Estimasi P1), dan detail tahap.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { apiGet } from '@/lib/api/client';
import { confirmAction, notify } from '@/lib/feedback';
import {
  PRINT_JOB_STAGE_LABELS,
  createPrintJob,
  deletePrintJob,
  listPrintJobs,
  type PrintJob,
  type PrintJobStage,
} from '@/lib/api/mfg-print-jobs';
import { listPrintEstimates, type PrintEstimate } from '@/lib/api/mfg-print-estimates';
import { PrintJobDetail } from './mfg-print-job-detail';

const selectCls = 'h-9 rounded-md border border-input bg-background px-2 text-sm';
const labelCls = 'mb-1 block text-xs font-medium text-muted-foreground';

function stageBadge(s: PrintJobStage) {
  if (s === 'SELESAI') return <Badge variant="success">{PRINT_JOB_STAGE_LABELS[s]}</Badge>;
  if (s === 'CANCELLED') return <Badge variant="danger">{PRINT_JOB_STAGE_LABELS[s]}</Badge>;
  if (s === 'PRE_PRESS') return <Badge variant="default">{PRINT_JOB_STAGE_LABELS[s]}</Badge>;
  return <Badge variant="info">{PRINT_JOB_STAGE_LABELS[s]}</Badge>;
}

function CreateForm({ onDone }: { onDone: (id?: string) => void }) {
  const [docDate, setDocDate] = React.useState(() => new Date().toISOString().slice(0, 10));
  const [estimates, setEstimates] = React.useState<PrintEstimate[]>([]);
  const [estimateId, setEstimateId] = React.useState('');
  const [items, setItems] = React.useState<{ id: string; label: string }[]>([]);
  const [itemId, setItemId] = React.useState('');
  const [printQuantity, setPrintQuantity] = React.useState('');
  const [title, setTitle] = React.useState('');
  const [paperSize, setPaperSize] = React.useState('');
  const [pageCount, setPageCount] = React.useState('');
  const [colorSpec, setColorSpec] = React.useState('');
  const [finishing, setFinishing] = React.useState('');
  const [masterFileName, setMasterFileName] = React.useState('');
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    (async () => {
      try {
        const res: any = await listPrintEstimates({ page: 1, limit: 100 });
        setEstimates(res?.data ?? []);
      } catch { /* abaikan */ }
      try {
        const it: any = await apiGet('/items?limit=200');
        const rows = it?.data ?? it;
        if (Array.isArray(rows))
          setItems(rows.map((r: any) => ({ id: String(r.id), label: r.name ?? String(r.id) })));
      } catch { /* abaikan */ }
    })();
  }, []);

  const pickEstimate = (id: string) => {
    setEstimateId(id);
    const est = estimates.find((e) => e.id === id);
    if (est) {
      setItemId(est.itemId ?? '');
      setPrintQuantity(est.printQuantity);
      setTitle(est.title);
      setPaperSize(est.paperSize ?? '');
      setPageCount(est.pageCount != null ? String(est.pageCount) : '');
      setColorSpec(est.colorSpec ?? '');
      setFinishing(est.finishing ?? '');
    }
  };

  const save = async () => {
    if (!itemId) return notify('Item hasil cetak wajib dipilih.', 'danger');
    if (!printQuantity) return notify('Oplah wajib diisi.', 'danger');
    setSaving(true);
    try {
      const job = await createPrintJob({
        branchId: '1033',
        docDate,
        estimateId: estimateId || undefined,
        itemId,
        printQuantity,
        title: title || undefined,
        paperSize: paperSize || undefined,
        pageCount: pageCount ? Number(pageCount) : undefined,
        colorSpec: colorSpec || undefined,
        finishing: finishing || undefined,
        masterFileName: masterFileName || undefined,
      });
      notify(`Job ${job.workOrderDocNumber ?? ''} dibuat — tahap Pre-press.`, 'success');
      onDone(job.id);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal membuat job.', 'danger');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Card className="p-4">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-base font-semibold">Job Cetak Baru</h2>
        <div className="flex gap-2">
          <Button variant="ghost" onClick={() => onDone()}>
            Kembali
          </Button>
          <Button onClick={save} disabled={saving}>
            {saving ? 'Membuat…' : 'Buat Job'}
          </Button>
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
        <div>
          <label className={labelCls}>Dari estimasi (opsional)</label>
          <select className={`${selectCls} w-full`} value={estimateId} onChange={(e) => pickEstimate(e.target.value)}>
            <option value="">— manual —</option>
            {estimates.map((e) => (
              <option key={e.id} value={e.id}>
                {e.docNumber} — {e.title}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className={labelCls}>Tanggal</label>
          <Input type="date" value={docDate} onChange={(e) => setDocDate(e.target.value)} />
        </div>
        <div>
          <label className={labelCls}>Item hasil cetak *</label>
          <select className={`${selectCls} w-full`} value={itemId} onChange={(e) => setItemId(e.target.value)}>
            <option value="">— pilih item —</option>
            {items.map((o) => (
              <option key={o.id} value={o.id}>{o.label}</option>
            ))}
          </select>
        </div>
        <div>
          <label className={labelCls}>Oplah *</label>
          <Input type="number" value={printQuantity} onChange={(e) => setPrintQuantity(e.target.value)} />
        </div>
        <div className="col-span-2">
          <label className={labelCls}>Judul job</label>
          <Input value={title} onChange={(e) => setTitle(e.target.value)} />
        </div>
        <div>
          <label className={labelCls}>Ukuran kertas</label>
          <Input value={paperSize} onChange={(e) => setPaperSize(e.target.value)} />
        </div>
        <div>
          <label className={labelCls}>Halaman</label>
          <Input type="number" value={pageCount} onChange={(e) => setPageCount(e.target.value)} />
        </div>
        <div>
          <label className={labelCls}>Warna</label>
          <Input value={colorSpec} onChange={(e) => setColorSpec(e.target.value)} />
        </div>
        <div>
          <label className={labelCls}>Finishing</label>
          <Input value={finishing} onChange={(e) => setFinishing(e.target.value)} />
        </div>
        <div className="col-span-2">
          <label className={labelCls}>Nama file master</label>
          <Input value={masterFileName} onChange={(e) => setMasterFileName(e.target.value)} placeholder="cth. lks-mtk-kelas4-final.pdf" />
        </div>
      </div>
      <p className="mt-3 text-xs text-muted-foreground">
        Membuat job akan membuat Work Order baru dan profil cetak tahap Pre-press dengan checklist standar.
      </p>
    </Card>
  );
}

export function MfgPrintJobsPage() {
  const [view, setView] = React.useState<
    { kind: 'list' } | { kind: 'create' } | { kind: 'detail'; id: string }
  >({ kind: 'list' });
  const [rows, setRows] = React.useState<PrintJob[]>([]);
  const [total, setTotal] = React.useState(0);
  const [page, setPage] = React.useState(1);
  const [search, setSearch] = React.useState('');
  const [stage, setStage] = React.useState('');
  const [loading, setLoading] = React.useState(false);
  const limit = 25;

  const load = React.useCallback(async () => {
    setLoading(true);
    try {
      const res: any = await listPrintJobs({
        page,
        limit,
        search: search || undefined,
        stage: stage || undefined,
      });
      setRows(res?.data ?? []);
      setTotal(res?.meta?.total ?? 0);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat job.', 'danger');
    } finally {
      setLoading(false);
    }
  }, [page, search, stage]);

  React.useEffect(() => {
    if (view.kind === 'list') void load();
  }, [load, view]);

  if (view.kind === 'create')
    return (
      <CreateForm
        onDone={(id) =>
          id ? setView({ kind: 'detail', id }) : setView({ kind: 'list' })
        }
      />
    );
  if (view.kind === 'detail')
    return <PrintJobDetail jobId={view.id} onBack={() => setView({ kind: 'list' })} />;

  const totalPages = Math.max(1, Math.ceil(total / limit));

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-semibold">Job Cetak</h1>
          <p className="text-sm text-muted-foreground">
            Pekerjaan cetak di atas Work Order: pre-press → cetak → finishing → QC. Total {total} job.
          </p>
        </div>
        <Button onClick={() => setView({ kind: 'create' })}>+ Job Baru</Button>
      </div>

      <Card className="p-3">
        <div className="flex flex-wrap gap-2">
          <Input
            className="w-64"
            placeholder="Cari nomor WO…"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
          />
          <select
            className={selectCls}
            value={stage}
            onChange={(e) => {
              setStage(e.target.value);
              setPage(1);
            }}
          >
            <option value="">Semua tahap</option>
            {Object.entries(PRINT_JOB_STAGE_LABELS).map(([k, v]) => (
              <option key={k} value={k}>{v}</option>
            ))}
          </select>
        </div>
      </Card>

      <Card className="p-0">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b text-left text-xs uppercase text-muted-foreground">
              <th className="px-3 py-2">No WO</th>
              <th className="px-3 py-2">Tanggal</th>
              <th className="px-3 py-2">Pekerjaan</th>
              <th className="px-3 py-2">Item Hasil</th>
              <th className="px-3 py-2 text-right">Oplah</th>
              <th className="px-3 py-2">Checklist</th>
              <th className="px-3 py-2">Tahap</th>
              <th className="px-3 py-2">Aksi</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => (
              <tr key={r.id} className="border-b hover:bg-muted/40">
                <td className="px-3 py-2 font-medium">{r.workOrderDocNumber ?? '—'}</td>
                <td className="px-3 py-2">{r.workOrderDocDate ?? '—'}</td>
                <td className="px-3 py-2">{r.title ?? '—'}</td>
                <td className="px-3 py-2">{r.itemName ?? '—'}</td>
                <td className="px-3 py-2 text-right">{Number(r.printQuantity).toLocaleString('id-ID')}</td>
                <td className="px-3 py-2">
                  {r.checklistProgress.done}/{r.checklistProgress.total}
                </td>
                <td className="px-3 py-2">{stageBadge(r.stage)}</td>
                <td className="px-3 py-2">
                  <div className="flex gap-1">
                    <Button variant="ghost" onClick={() => setView({ kind: 'detail', id: r.id })}>
                      Detail
                    </Button>
                    {r.stage === 'PRE_PRESS' && (
                      <Button
                        variant="ghost"
                        onClick={() =>
                          confirmAction({
                            message: `Hapus job ${r.workOrderDocNumber ?? ''}? (Work Order tidak ikut terhapus)`,
                            onConfirm: async () => {
                              try {
                                await deletePrintJob(r.id);
                                notify('Job dihapus.', 'success');
                                void load();
                              } catch (e: any) {
                                notify(e?.message ?? 'Gagal menghapus.', 'danger');
                              }
                            },
                          })
                        }
                      >
                        Hapus
                      </Button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {!rows.length && (
              <tr>
                <td colSpan={8} className="px-3 py-8 text-center text-muted-foreground">
                  {loading ? 'Memuat…' : 'Belum ada job cetak.'}
                </td>
              </tr>
            )}
          </tbody>
        </table>
        <div className="flex items-center justify-between px-3 py-2 text-sm">
          <span>
            Halaman {page} dari {totalPages}
          </span>
          <div className="flex gap-2">
            <Button variant="ghost" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
              Sebelumnya
            </Button>
            <Button variant="ghost" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
              Berikutnya
            </Button>
          </div>
        </div>
      </Card>
    </div>
  );
}

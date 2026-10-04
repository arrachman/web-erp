'use client';

/**
 * Fase 2 P1 — Estimasi Cetak: daftar estimasi pekerjaan cetak (komponen
 * biaya -> total biaya -> harga jual bermargin) dan konversinya menjadi
 * Penawaran. Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { confirmAction, notify } from '@/lib/feedback';
import { formatRupiah } from '@/lib/format';
import {
  convertPrintEstimate,
  deletePrintEstimate,
  listPrintEstimates,
  setPrintEstimateStatus,
  type PrintEstimate,
  type PrintEstimateStatus,
} from '@/lib/api/mfg-print-estimates';
import { PrintEstimateForm } from './mfg-print-estimate-form';

const rp = (v: string | number) => formatRupiah(Number(v) || 0);
const selectCls =
  'h-9 rounded-md border border-input bg-background px-2 text-sm';

const STATUS_LABELS: Record<PrintEstimateStatus, string> = {
  DRAFT: 'Draft',
  ISSUED: 'Diterbitkan',
  CONVERTED: 'Jadi Penawaran',
  CANCELLED: 'Batal',
};

function statusBadge(s: PrintEstimateStatus) {
  if (s === 'ISSUED') return <Badge variant="info">{STATUS_LABELS[s]}</Badge>;
  if (s === 'CONVERTED') return <Badge variant="success">{STATUS_LABELS[s]}</Badge>;
  if (s === 'CANCELLED') return <Badge variant="danger">{STATUS_LABELS[s]}</Badge>;
  return <Badge variant="default">{STATUS_LABELS[s]}</Badge>;
}

export function MfgPrintEstimatesPage() {
  const [view, setView] = React.useState<{ kind: 'list' } | { kind: 'form'; id?: string }>({ kind: 'list' });
  const [rows, setRows] = React.useState<PrintEstimate[]>([]);
  const [total, setTotal] = React.useState(0);
  const [page, setPage] = React.useState(1);
  const [search, setSearch] = React.useState('');
  const [status, setStatus] = React.useState('');
  const [loading, setLoading] = React.useState(false);
  const limit = 25;

  const load = React.useCallback(async () => {
    setLoading(true);
    try {
      const res: any = await listPrintEstimates({
        page,
        limit,
        search: search || undefined,
        status: status || undefined,
      });
      setRows(res?.data ?? []);
      setTotal(res?.meta?.total ?? 0);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat estimasi.', 'danger');
    } finally {
      setLoading(false);
    }
  }, [page, search, status]);

  React.useEffect(() => {
    if (view.kind === 'list') void load();
  }, [load, view]);

  if (view.kind === 'form') {
    return (
      <PrintEstimateForm
        estimateId={view.id}
        onDone={() => setView({ kind: 'list' })}
      />
    );
  }

  const totalPages = Math.max(1, Math.ceil(total / limit));

  const act = async (fn: () => Promise<unknown>, okMsg: string) => {
    try {
      await fn();
      notify(okMsg, 'success');
      void load();
    } catch (e: any) {
      notify(e?.message ?? 'Aksi gagal.', 'danger');
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-semibold">Estimasi Cetak</h1>
          <p className="text-sm text-muted-foreground">
            Hitung biaya pekerjaan cetak dan jadikan penawaran. Total {total} estimasi.
          </p>
        </div>
        <Button onClick={() => setView({ kind: 'form' })}>+ Estimasi Baru</Button>
      </div>

      <Card className="p-3">
        <div className="flex flex-wrap gap-2">
          <Input
            className="w-64"
            placeholder="Cari nomor / judul pekerjaan…"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
          />
          <select
            className={selectCls}
            value={status}
            onChange={(e) => {
              setStatus(e.target.value);
              setPage(1);
            }}
          >
            <option value="">Semua status</option>
            {Object.entries(STATUS_LABELS).map(([k, v]) => (
              <option key={k} value={k}>{v}</option>
            ))}
          </select>
        </div>
      </Card>

      <Card className="p-0">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b text-left text-xs uppercase text-muted-foreground">
              <th className="px-3 py-2">Nomor</th>
              <th className="px-3 py-2">Tanggal</th>
              <th className="px-3 py-2">Pekerjaan</th>
              <th className="px-3 py-2">Pelanggan</th>
              <th className="px-3 py-2 text-right">Oplah</th>
              <th className="px-3 py-2 text-right">Total Biaya</th>
              <th className="px-3 py-2 text-right">Harga Jual</th>
              <th className="px-3 py-2">Status</th>
              <th className="px-3 py-2">Aksi</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => (
              <tr key={r.id} className="border-b hover:bg-muted/40">
                <td className="px-3 py-2 font-medium">{r.docNumber}</td>
                <td className="px-3 py-2">{r.docDate}</td>
                <td className="px-3 py-2">{r.title}</td>
                <td className="px-3 py-2">{r.customerName ?? '—'}</td>
                <td className="px-3 py-2 text-right">{Number(r.printQuantity).toLocaleString('id-ID')}</td>
                <td className="px-3 py-2 text-right">{rp(r.totalCost)}</td>
                <td className="px-3 py-2 text-right">{rp(r.totalPrice)}</td>
                <td className="px-3 py-2">{statusBadge(r.status)}</td>
                <td className="px-3 py-2">
                  <div className="flex flex-wrap gap-1">
                    {r.status === 'DRAFT' && (
                      <>
                        <Button variant="ghost" onClick={() => setView({ kind: 'form', id: r.id })}>Ubah</Button>
                        <Button
                          variant="ghost"
                          onClick={() => act(() => setPrintEstimateStatus(r.id, 'ISSUED'), 'Estimasi diterbitkan.')}
                        >
                          Terbitkan
                        </Button>
                        <Button
                          variant="ghost"
                          onClick={() =>
                            confirmAction({
                              message: `Hapus estimasi ${r.docNumber}?`,
                              onConfirm: () =>
                                void act(() => deletePrintEstimate(r.id), 'Estimasi dihapus.'),
                            })
                          }
                        >
                          Hapus
                        </Button>
                      </>
                    )}
                    {(r.status === 'DRAFT' || r.status === 'ISSUED') && (
                      <>
                        <Button
                          variant="primary"
                          onClick={async () => {
                            try {
                              const res = await convertPrintEstimate(r.id);
                              notify(`Menjadi penawaran ${res.quotationDocNumber ?? ''}.`, 'success');
                              void load();
                            } catch (e: any) {
                              notify(e?.message ?? 'Konversi gagal.', 'danger');
                            }
                          }}
                        >
                          Jadikan Penawaran
                        </Button>
                        <Button
                          variant="ghost"
                          onClick={() =>
                            confirmAction({
                              message: `Batalkan estimasi ${r.docNumber}?`,
                              onConfirm: () =>
                                void act(
                                  () => setPrintEstimateStatus(r.id, 'CANCELLED'),
                                  'Estimasi dibatalkan.',
                                ),
                            })
                          }
                        >
                          Batal
                        </Button>
                      </>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {!rows.length && (
              <tr>
                <td colSpan={9} className="px-3 py-8 text-center text-muted-foreground">
                  {loading ? 'Memuat…' : 'Belum ada estimasi.'}
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

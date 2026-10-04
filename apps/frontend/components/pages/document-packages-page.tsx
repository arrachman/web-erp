'use client';

/**
 * A3 — Paket Dokumen pengadaan: buat surat penawaran, surat pesanan,
 * invoice, kuitansi, surat jalan & BAST dari rantai transaksi order
 * (satu dokumen atau satu paket PDF), varian BOS / non-BOS, tanda tangan
 * terekam, arsip per sekolah per tahun anggaran. Seksi penerimaan BAST
 * ada di document-packages-bast.tsx. Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { notify } from '@/lib/feedback';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { listOrderHub, type OrderHubRow } from '@/lib/api/order-hub';
import {
  ALL_DOC_KINDS,
  DOC_KIND_LABELS,
  downloadDocument,
  generateDocuments,
  getDocAvailability,
  listDocumentPackages,
  signDocument,
  type DocAvailability,
  type DocKind,
  type DocVariant,
  type GeneratedDocument,
} from '@/lib/api/document-packages';
import { DocumentPackagesBast } from './document-packages-bast';
import { DocModal, fmtDate } from './document-packages-shared';

function statusBadge(status: GeneratedDocument['status']) {
  if (status === 'SIGNED') return <Badge variant="success">Ditandatangani</Badge>;
  if (status === 'SUPERSEDED') return <Badge variant="default">Digantikan</Badge>;
  return <Badge variant="info">Dibuat</Badge>;
}

export function DocumentPackagesPage() {
  // ── Buat dokumen ──────────────────────────────────────────────────────────
  const [orderSearch, setOrderSearch] = React.useState('');
  const [orderResults, setOrderResults] = React.useState<OrderHubRow[]>([]);
  const [order, setOrder] = React.useState<OrderHubRow | null>(null);
  const [availability, setAvailability] = React.useState<DocAvailability | null>(null);
  const [kinds, setKinds] = React.useState<DocKind[]>([]);
  const [variant, setVariant] = React.useState<DocVariant>('BOS');
  const [asPackage, setAsPackage] = React.useState(false);
  const [generating, setGenerating] = React.useState(false);

  React.useEffect(() => {
    if (!orderSearch.trim() || order) {
      setOrderResults([]);
      return;
    }
    const t = setTimeout(async () => {
      try {
        const res = await listOrderHub({ search: orderSearch.trim(), limit: 8, sortBy: 'docNumber', sortDir: 'desc' });
        setOrderResults(res.data);
      } catch {
        setOrderResults([]);
      }
    }, 300);
    return () => clearTimeout(t);
  }, [orderSearch, order]);

  const pickOrder = async (row: OrderHubRow) => {
    setOrder(row);
    setOrderResults([]);
    try {
      const av = await getDocAvailability(row.id);
      setAvailability(av);
      setVariant(av.defaultVariant);
      setKinds(av.available.map((a) => a.kind));
    } catch {
      notify('Gagal memuat ketersediaan dokumen', 'danger');
    }
  };

  const clearOrder = () => {
    setOrder(null);
    setAvailability(null);
    setKinds([]);
    setOrderSearch('');
  };

  // ── Arsip ─────────────────────────────────────────────────────────────────
  const [search, setSearch] = React.useState('');
  const [debouncedSearch, setDebouncedSearch] = React.useState('');
  const [docType, setDocType] = React.useState('');
  const [status, setStatus] = React.useState('');
  const [year, setYear] = React.useState('');
  const { page, pageSize, setPage } = useListPagination('/sales/document-packages');
  const [signTarget, setSignTarget] = React.useState<GeneratedDocument | null>(null);
  const [signerName, setSignerName] = React.useState('');
  const [signNote, setSignNote] = React.useState('');

  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);
  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, docType, status, year, pageSize, setPage]);

  const { rows, meta, loading, reload } = useErpList<GeneratedDocument>(
    () =>
      listDocumentPackages({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        docType: docType || undefined,
        status: status || undefined,
        budgetYear: year ? Number(year) : undefined,
      }),
    [page, pageSize, debouncedSearch, docType, status, year],
  );

  const runGenerate = async (overrideOrderId?: string, overrideKinds?: DocKind[]) => {
    const targetOrder = overrideOrderId ?? order?.id;
    const targetKinds = overrideKinds ?? kinds;
    if (!targetOrder || targetKinds.length === 0) return;
    setGenerating(true);
    try {
      const made = await generateDocuments({
        orderId: targetOrder,
        docTypes: targetKinds,
        variant,
        package: overrideKinds ? false : asPackage && targetKinds.length > 1,
      });
      notify(`${made.length} dokumen dibuat (versi terbaru)`, 'success');
      reload();
      if (order && !overrideOrderId) {
        setAvailability(await getDocAvailability(order.id));
      }
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal membuat dokumen', 'danger');
    } finally {
      setGenerating(false);
    }
  };

  const runSign = async () => {
    if (!signTarget || !signerName.trim()) return;
    try {
      await signDocument(signTarget.id, { signerName: signerName.trim(), note: signNote || undefined });
      notify('Dokumen ditandai sudah ditandatangani', 'success');
      setSignTarget(null);
      setSignerName('');
      setSignNote('');
      reload();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menandatangani', 'danger');
    }
  };

  return (
    <div className="space-y-4 p-4">
      <div>
        <h1 className="text-xl font-semibold">Paket Dokumen Pengadaan</h1>
        <p className="text-sm text-slate-500">
          Dokumen dibuat dari data transaksi order — tanpa input ulang. Arsip tersimpan per sekolah &amp; tahun anggaran.
        </p>
      </div>

      <Card>
        <div className="space-y-3 px-4 py-4">
          <div className="font-semibold">Buat Dokumen dari Order</div>
          {!order ? (
            <div className="relative max-w-xl">
              <Input
                placeholder="Cari nomor order / nama sekolah…"
                value={orderSearch}
                onChange={(e) => setOrderSearch(e.target.value)}
              />
              {orderResults.length > 0 && (
                <div className="absolute z-10 mt-1 w-full rounded-md border bg-white shadow">
                  {orderResults.map((r) => (
                    <button
                      key={r.id}
                      className="block w-full px-3 py-2 text-left text-sm hover:bg-slate-50"
                      onClick={() => void pickOrder(r)}
                    >
                      <span className="font-medium">{r.docNumber}</span> — {r.customer?.name ?? '-'}
                    </button>
                  ))}
                </div>
              )}
            </div>
          ) : (
            <div className="space-y-3">
              <div className="flex flex-wrap items-center gap-2 text-sm">
                <span className="font-medium">{order.docNumber}</span>
                <span>{order.customer?.name ?? '-'}</span>
                <Button variant="ghost" onClick={clearOrder}>Ganti order</Button>
              </div>
              <div className="flex flex-wrap gap-2">
                {ALL_DOC_KINDS.map((k) => {
                  const av = availability?.available.find((a) => a.kind === k);
                  const checked = kinds.includes(k);
                  return (
                    <label
                      key={k}
                      className={`flex items-center gap-2 rounded-md border px-2 py-1.5 text-sm ${av ? '' : 'opacity-40'}`}
                      title={av ? (av.latest ? `Terbaru: v${av.latest.version} (${av.latest.status})` : 'Belum pernah dibuat') : 'Dokumen sumber belum ada'}
                    >
                      <input
                        type="checkbox"
                        disabled={!av}
                        checked={checked}
                        onChange={(e) =>
                          setKinds((prev) => (e.target.checked ? [...prev, k] : prev.filter((x) => x !== k)))
                        }
                      />
                      {DOC_KIND_LABELS[k]}
                      {av?.latest ? <span className="text-xs text-slate-500">v{av.latest.version}</span> : null}
                    </label>
                  );
                })}
              </div>
              <div className="flex flex-wrap items-center gap-3">
                <select className="rounded-md border px-2 py-1.5 text-sm" value={variant} onChange={(e) => setVariant(e.target.value as DocVariant)}>
                  <option value="BOS">Varian BOS</option>
                  <option value="NON_BOS">Varian Non-BOS</option>
                </select>
                <label className="flex items-center gap-2 text-sm">
                  <input type="checkbox" checked={asPackage} onChange={(e) => setAsPackage(e.target.checked)} />
                  Gabung jadi satu paket PDF
                </label>
                <Button variant="primary" disabled={generating || kinds.length === 0} onClick={() => void runGenerate()}>
                  {generating ? 'Membuat…' : 'Buat Dokumen'}
                </Button>
              </div>
            </div>
          )}
        </div>
      </Card>

      <DocumentPackagesBast onGenerateBast={(orderId) => void runGenerate(orderId, ['BAST'])} />

      <Card>
        <div className="space-y-3 px-4 py-4">
          <div className="font-semibold">Arsip Dokumen</div>
          <div className="flex flex-wrap gap-2">
            <Input placeholder="Cari berkas / nomor order…" value={search} onChange={(e) => setSearch(e.target.value)} />
            <select className="rounded-md border px-2 py-1.5 text-sm" value={docType} onChange={(e) => setDocType(e.target.value)}>
              <option value="">Semua Jenis</option>
              {ALL_DOC_KINDS.map((k) => (
                <option key={k} value={k}>{DOC_KIND_LABELS[k]}</option>
              ))}
              <option value="PAKET">Paket PDF</option>
            </select>
            <select className="rounded-md border px-2 py-1.5 text-sm" value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="">Semua Status</option>
              <option value="GENERATED">Dibuat</option>
              <option value="SIGNED">Ditandatangani</option>
              <option value="SUPERSEDED">Digantikan</option>
            </select>
            <Input placeholder="Tahun anggaran" value={year} onChange={(e) => setYear(e.target.value)} />
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b text-left text-slate-500">
                  <th className="py-2 pr-2">Berkas</th>
                  <th className="py-2 pr-2">Jenis</th>
                  <th className="py-2 pr-2">Sekolah</th>
                  <th className="py-2 pr-2">Order</th>
                  <th className="py-2 pr-2">Versi</th>
                  <th className="py-2 pr-2">Status</th>
                  <th className="py-2 pr-2">Dibuat</th>
                  <th className="py-2">Aksi</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((d) => (
                  <tr key={d.id} className="border-b">
                    <td className="py-2 pr-2 font-medium">{d.fileName}</td>
                    <td className="py-2 pr-2">{d.docType === 'PAKET' ? 'Paket PDF' : DOC_KIND_LABELS[d.docType as DocKind] ?? d.docType} · {d.variant === 'BOS' ? 'BOS' : 'Non-BOS'}</td>
                    <td className="py-2 pr-2">{d.school?.name ?? '-'}</td>
                    <td className="py-2 pr-2">{d.order?.docNumber ?? d.orderId}</td>
                    <td className="py-2 pr-2">v{d.version}</td>
                    <td className="py-2 pr-2">
                      {statusBadge(d.status)}
                      {d.status === 'SIGNED' && d.signedByName ? (
                        <div className="text-xs text-slate-500">{d.signedByName} · {fmtDate(d.signedAt)}</div>
                      ) : null}
                    </td>
                    <td className="py-2 pr-2">{fmtDate(d.createdAt)}</td>
                    <td className="py-2">
                      <div className="flex gap-1">
                        <Button variant="ghost" onClick={() => void downloadDocument(d)}>Unduh</Button>
                        {d.status === 'GENERATED' && (
                          <Button variant="ghost" onClick={() => setSignTarget(d)}>Tanda Tangani</Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
                {!loading && rows.length === 0 && (
                  <tr><td colSpan={8} className="py-4 text-center text-slate-400">Belum ada dokumen di arsip.</td></tr>
                )}
              </tbody>
            </table>
          </div>
          <div className="flex items-center justify-between text-sm text-slate-500">
            <span>Total {meta?.total ?? 0} dokumen</span>
            <span>
              Halaman {page} dari {meta?.totalPages ?? 1}{' '}
              <Button variant="ghost" disabled={page <= 1} onClick={() => setPage(page - 1)}>‹</Button>
              <Button variant="ghost" disabled={!meta || page >= meta.totalPages} onClick={() => setPage(page + 1)}>›</Button>
            </span>
          </div>
        </div>
      </Card>

      {signTarget && (
        <DocModal title={`Tanda Tangani — ${signTarget.fileName}`} onClose={() => setSignTarget(null)}>
          <p className="text-sm text-slate-500">
            Pencatatan tanda tangan/stempel digital: nama penanda tangan dan waktu tersimpan sebagai audit (versi dokumen ini tidak berubah).
          </p>
          <Input placeholder="Nama penanda tangan" value={signerName} onChange={(e) => setSignerName(e.target.value)} />
          <Input placeholder="Catatan (opsional)" value={signNote} onChange={(e) => setSignNote(e.target.value)} />
          <Button variant="primary" disabled={!signerName.trim()} onClick={() => void runSign()}>Simpan Tanda Tangan</Button>
        </DocModal>
      )}
    </div>
  );
}

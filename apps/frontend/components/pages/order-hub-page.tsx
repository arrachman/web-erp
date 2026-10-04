'use client';

/**
 * A2 — Order Hub multi-kanal: satu antrean sales order lintas kanal
 * (SIPLah, sales, admin, portal) dengan status bisnis terpadu yang
 * diturunkan dari rantai dokumen. Dilengkapi impor pesanan SIPLah
 * (idempotent) dan panel detail berisi rantai dokumen + riwayat status.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { notify } from '@/lib/feedback';
import { formatRupiah } from '@/lib/format';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import {
  CHANNEL_LABELS,
  HUB_STAGES,
  HUB_STAGE_LABELS,
  getOrderHubDetail,
  importSiplahOrders,
  listOrderHub,
  syncOrderHub,
  type ErpFundingSource,
  type ErpOrderHubStage,
  type ErpSalesChannel,
  type ImportSiplahRow,
  type OrderHubDetail,
  type OrderHubRow,
} from '@/lib/api/order-hub';

const CHANNELS = Object.keys(CHANNEL_LABELS) as ErpSalesChannel[];

function stageBadgeVariant(stage: ErpOrderHubStage | null): string {
  switch (stage) {
    case 'LUNAS':
      return 'bg-emerald-100 text-emerald-800';
    case 'DITAGIH':
      return 'bg-amber-100 text-amber-800';
    case 'DITERIMA':
      return 'bg-teal-100 text-teal-800';
    case 'DIKIRIM':
      return 'bg-sky-100 text-sky-800';
    case 'DISIAPKAN':
      return 'bg-indigo-100 text-indigo-800';
    case 'DIKONFIRMASI':
      return 'bg-violet-100 text-violet-800';
    default:
      return 'bg-slate-100 text-slate-700';
  }
}

function StageStepper({ stage }: { stage: ErpOrderHubStage | null }) {
  const idx = stage ? HUB_STAGES.indexOf(stage) : -1;
  return (
    <div className="flex items-center gap-1">
      {HUB_STAGES.map((s, i) => (
        <span
          key={s}
          title={HUB_STAGE_LABELS[s]}
          className={`h-1.5 w-5 rounded-full ${i <= idx ? 'bg-emerald-500' : 'bg-slate-200'}`}
        />
      ))}
    </div>
  );
}

// ─── CSV parsing (SIPLah export, parsed client-side) ─────────────────────────

const HEADER_ALIASES: Record<string, keyof ImportSiplahRow> = {
  external_order_id: 'externalOrderId',
  no_pesanan: 'externalOrderId',
  nomor_pesanan: 'externalOrderId',
  id_pesanan: 'externalOrderId',
  order_date: 'orderDate',
  tanggal: 'orderDate',
  tanggal_pesanan: 'orderDate',
  school_code: 'schoolCode',
  kode_sekolah: 'schoolCode',
  school_npsn: 'schoolNpsn',
  npsn: 'schoolNpsn',
  school_name: 'schoolName',
  nama_sekolah: 'schoolName',
  sekolah: 'schoolName',
  item_code: 'itemCode',
  kode_item: 'itemCode',
  kode_produk: 'itemCode',
  quantity: 'quantity',
  qty: 'quantity',
  jumlah: 'quantity',
  unit_price: 'unitPrice',
  harga: 'unitPrice',
  harga_satuan: 'unitPrice',
  funding_source: 'fundingSource',
  sumber_dana: 'fundingSource',
  budget_year: 'budgetYear',
  tahun_anggaran: 'budgetYear',
  budget_stage: 'budgetStage',
  tahap: 'budgetStage',
  marketplace_fee: 'marketplaceFee',
  fee: 'marketplaceFee',
  disbursement_ref: 'disbursementRef',
  ref_pencairan: 'disbursementRef',
};

function parseCsv(text: string): ImportSiplahRow[] {
  const lines = text.split(/\r?\n/).filter((l) => l.trim().length > 0);
  if (lines.length < 2) return [];
  const headers = lines[0].split(/[;,]/).map((h) => h.trim().toLowerCase().replace(/\s+/g, '_'));
  const rows: ImportSiplahRow[] = [];
  for (const line of lines.slice(1)) {
    const cells = line.split(/[;,]/).map((c) => c.trim().replace(/^"|"$/g, ''));
    const raw: Record<string, string> = {};
    headers.forEach((h, i) => {
      const key = HEADER_ALIASES[h];
      if (key && cells[i]) raw[key] = cells[i];
    });
    if (!raw.externalOrderId || !raw.itemCode) continue;
    rows.push({
      externalOrderId: raw.externalOrderId,
      orderDate: raw.orderDate ?? '',
      schoolCode: raw.schoolCode,
      schoolNpsn: raw.schoolNpsn,
      schoolName: raw.schoolName,
      itemCode: raw.itemCode,
      quantity: raw.quantity ?? '1',
      unitPrice: raw.unitPrice,
      fundingSource: (raw.fundingSource as ErpFundingSource) ?? 'BOS',
      budgetYear: raw.budgetYear ? Number(raw.budgetYear) : undefined,
      budgetStage: raw.budgetStage ? Number(raw.budgetStage) : undefined,
      marketplaceFee: raw.marketplaceFee,
      disbursementRef: raw.disbursementRef,
    });
  }
  return rows;
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function OrderHubPage() {
  const [search, setSearch] = React.useState('');
  const [debouncedSearch, setDebouncedSearch] = React.useState('');
  const [channel, setChannel] = React.useState('');
  const [hubStatus, setHubStatus] = React.useState('');
  const [funding, setFunding] = React.useState('');
  const [budgetYear, setBudgetYear] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('/sales/order-hub');

  const [detail, setDetail] = React.useState<OrderHubDetail | null>(null);
  const [detailLoading, setDetailLoading] = React.useState(false);
  const [showImport, setShowImport] = React.useState(false);
  const [csvText, setCsvText] = React.useState('');
  const [importing, setImporting] = React.useState(false);
  const [importResult, setImportResult] = React.useState<string | null>(null);
  const [syncing, setSyncing] = React.useState(false);

  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, channel, hubStatus, funding, budgetYear, pageSize, setPage]);

  const { rows, meta, loading, reload } = useErpList<OrderHubRow>(
    () =>
      listOrderHub({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        channel: (channel || undefined) as ErpSalesChannel | undefined,
        hubStatus: (hubStatus || undefined) as ErpOrderHubStage | undefined,
        fundingSource: (funding || undefined) as ErpFundingSource | undefined,
        budgetYear: budgetYear ? Number(budgetYear) : undefined,
        sortBy: 'docNumber',
        sortDir: 'desc',
      }),
    [page, pageSize, debouncedSearch, channel, hubStatus, funding, budgetYear],
  );

  const openDetail = async (row: OrderHubRow) => {
    setDetailLoading(true);
    try {
      setDetail(await getOrderHubDetail(row.id));
    } catch {
      notify('Gagal memuat detail order', 'danger');
    } finally {
      setDetailLoading(false);
    }
  };

  const runSync = async () => {
    setSyncing(true);
    try {
      const r = await syncOrderHub();
      notify(`Sinkron status selesai: ${r.checked} order diperiksa, ${r.transitionsLogged} transisi baru dicatat.`, 'success');
      reload();
    } catch {
      notify('Sinkron status gagal', 'danger');
    } finally {
      setSyncing(false);
    }
  };

  const runImport = async () => {
    const parsed = parseCsv(csvText);
    if (!parsed.length) {
      setImportResult('Tidak ada baris valid. Pastikan ada header dan kolom no pesanan + kode item.');
      return;
    }
    setImporting(true);
    try {
      const r = await importSiplahOrders(parsed);
      const detailLines = r.results
        .map((x) => `${x.externalOrderId}: ${x.status}${x.docNumber ? ` (${x.docNumber})` : ''}${x.message ? ` — ${x.message}` : ''}`)
        .join('\n');
      setImportResult(`Selesai: ${r.created} dibuat, ${r.skipped} dilewati (sudah ada), ${r.errors} gagal.\n${detailLines}`);
      reload();
    } catch {
      setImportResult('Impor gagal — periksa format file.');
    } finally {
      setImporting(false);
    }
  };

  const totalPages = meta?.totalPages ?? 1;

  return (
    <div className="space-y-4 p-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div>
          <h1 className="text-xl font-semibold">Order Hub</h1>
          <p className="text-sm text-slate-500">
            Satu antrean pesanan semua kanal dengan status terpadu: baru → dikonfirmasi → disiapkan →
            dikirim → diterima (BAST) → ditagih → lunas.
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="ghost" onClick={runSync} disabled={syncing}>
            {syncing ? 'Menyinkronkan…' : 'Sinkron Status'}
          </Button>
          <Button onClick={() => { setShowImport(true); setImportResult(null); }}>Impor SIPLah</Button>
        </div>
      </div>

      <Card className="p-3">
        <div className="flex flex-wrap gap-2">
          <Input
            placeholder="Cari no order / id eksternal…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-64"
          />
          <select className="rounded-md border px-2 py-1.5 text-sm" value={channel} onChange={(e) => setChannel(e.target.value)}>
            <option value="">Semua Kanal</option>
            {CHANNELS.map((c) => (
              <option key={c} value={c}>{CHANNEL_LABELS[c]}</option>
            ))}
          </select>
          <select className="rounded-md border px-2 py-1.5 text-sm" value={hubStatus} onChange={(e) => setHubStatus(e.target.value)}>
            <option value="">Semua Status</option>
            {HUB_STAGES.map((s) => (
              <option key={s} value={s}>{HUB_STAGE_LABELS[s]}</option>
            ))}
          </select>
          <select className="rounded-md border px-2 py-1.5 text-sm" value={funding} onChange={(e) => setFunding(e.target.value)}>
            <option value="">Semua Dana</option>
            <option value="BOS">BOS</option>
            <option value="NON_BOS">Non-BOS</option>
          </select>
          <Input
            placeholder="Tahun anggaran"
            value={budgetYear}
            onChange={(e) => setBudgetYear(e.target.value.replace(/[^0-9]/g, ''))}
            className="w-36"
          />
        </div>
      </Card>

      <Card>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-slate-50 text-left">
                <th className="px-3 py-2 font-medium">No Order</th>
                <th className="px-3 py-2 font-medium">Tanggal</th>
                <th className="px-3 py-2 font-medium">Sekolah / Pelanggan</th>
                <th className="px-3 py-2 font-medium">Kanal</th>
                <th className="px-3 py-2 font-medium">Status Hub</th>
                <th className="px-3 py-2 font-medium">Dana</th>
                <th className="px-3 py-2 font-medium">Dokumen</th>
                <th className="px-3 py-2 text-right font-medium">Total</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr><td colSpan={8} className="px-3 py-8 text-center text-slate-500">Memuat…</td></tr>
              ) : rows.length === 0 ? (
                <tr><td colSpan={8} className="px-3 py-8 text-center text-slate-500">Belum ada data</td></tr>
              ) : (
                rows.map((r) => (
                  <tr key={r.id} className="cursor-pointer border-b hover:bg-slate-50" onClick={() => openDetail(r)}>
                    <td className="px-3 py-2">
                      <div className="font-medium">{r.docNumber}</div>
                      {r.externalOrderId && <div className="text-xs text-slate-500">Ext: {r.externalOrderId}</div>}
                    </td>
                    <td className="px-3 py-2">{r.docDate?.slice(0, 10)}</td>
                    <td className="px-3 py-2">
                      <div>{r.customer?.name ?? '—'}</div>
                      <div className="text-xs text-slate-500">{r.customer?.code ?? ''}</div>
                    </td>
                    <td className="px-3 py-2"><Badge variant="info">{CHANNEL_LABELS[r.channel] ?? r.channel}</Badge></td>
                    <td className="px-3 py-2">
                      <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${stageBadgeVariant(r.hubStage)}`}>
                        {r.hubStage ? HUB_STAGE_LABELS[r.hubStage] : '—'}
                      </span>
                      <div className="mt-1"><StageStepper stage={r.hubStage} /></div>
                      {r.needsProduction && <div className="mt-1 text-xs font-medium text-orange-600">Perlu produksi</div>}
                    </td>
                    <td className="px-3 py-2">
                      {r.fundingSource ? (
                        <div>
                          <Badge variant="info">{r.fundingSource === 'BOS' ? 'BOS' : 'Non-BOS'}</Badge>
                          <div className="mt-0.5 text-xs text-slate-500">
                            {r.budgetYear ?? ''}{r.budgetStage ? ` · Tahap ${r.budgetStage}` : ''}
                          </div>
                        </div>
                      ) : '—'}
                    </td>
                    <td className="px-3 py-2 text-xs text-slate-600">
                      DO {r.hubCounts.deliveryOrders} · DR {r.hubCounts.deliveryReports} · SI {r.hubCounts.invoices}
                    </td>
                    <td className="px-3 py-2 text-right font-medium">{formatRupiah(Number(r.grandTotal))}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        <div className="flex items-center justify-between border-t px-3 py-2 text-sm">
          <span className="text-slate-500">
            {meta ? `${meta.total} order · halaman ${page} dari ${totalPages}` : ''}
          </span>
          <div className="flex items-center gap-2">
            <select
              className="rounded-md border px-2 py-1 text-sm"
              value={pageSize}
              onChange={(e) => setPageSize(Number(e.target.value))}
            >
              {[10, 25, 50, 100].map((n) => (
                <option key={n} value={n}>{n} / halaman</option>
              ))}
            </select>
            <Button variant="ghost" size="sm" disabled={page <= 1} onClick={() => setPage(page - 1)}>Sebelumnya</Button>
            <Button variant="ghost" size="sm" disabled={page >= totalPages} onClick={() => setPage(page + 1)}>Berikutnya</Button>
          </div>
        </div>
      </Card>

      {showImport && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <Card className="max-h-[90vh] w-full max-w-2xl overflow-y-auto p-4">
            <h2 className="text-lg font-semibold">Impor Pesanan SIPLah</h2>
            <p className="mt-1 text-sm text-slate-500">
              Unggah file ekspor SIPLah (CSV) atau tempel isinya. Kolom wajib: no pesanan
              (external_order_id) dan kode item (item_code). Baris dengan no pesanan sama digabung
              menjadi satu order; order yang sudah pernah diimpor akan dilewati (tidak dobel).
              Sekolah dicocokkan via kode partner, NPSN, atau nama.
            </p>
            <input
              type="file"
              accept=".csv,.txt"
              className="mt-3 block text-sm"
              onChange={async (e) => {
                const f = e.target.files?.[0];
                if (f) setCsvText(await f.text());
              }}
            />
            <textarea
              className="mt-3 h-40 w-full rounded-md border p-2 font-mono text-xs"
              placeholder={'external_order_id,order_date,school_code,item_code,quantity,unit_price\nSIP-001,2026-09-15,CUST-0001,BUKU-001,10,150000'}
              value={csvText}
              onChange={(e) => setCsvText(e.target.value)}
            />
            {importResult && (
              <pre className="mt-3 max-h-48 overflow-y-auto whitespace-pre-wrap rounded-md bg-slate-50 p-2 text-xs">{importResult}</pre>
            )}
            <div className="mt-4 flex justify-end gap-2">
              <Button variant="ghost" onClick={() => setShowImport(false)}>Tutup</Button>
              <Button onClick={runImport} disabled={importing || !csvText.trim()}>
                {importing ? 'Mengimpor…' : 'Impor Sekarang'}
              </Button>
            </div>
          </Card>
        </div>
      )}

      {(detail || detailLoading) && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={() => setDetail(null)}>
          <Card className="max-h-[90vh] w-full max-w-3xl overflow-y-auto p-4" onClick={(e) => e.stopPropagation()}>
            {detailLoading || !detail ? (
              <p className="py-10 text-center text-slate-500">Memuat detail…</p>
            ) : (
              <div className="space-y-4">
                <div className="flex items-start justify-between">
                  <div>
                    <h2 className="text-lg font-semibold">{detail.docNumber}</h2>
                    <p className="text-sm text-slate-500">
                      {detail.customer?.name} · {CHANNEL_LABELS[detail.channel]}
                      {detail.externalOrderId ? ` · Ext ${detail.externalOrderId}` : ''}
                      {detail.fundingSource ? ` · ${detail.fundingSource}${detail.budgetYear ? ` ${detail.budgetYear}` : ''}${detail.budgetStage ? ` Tahap ${detail.budgetStage}` : ''}` : ''}
                    </p>
                  </div>
                  <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${stageBadgeVariant(detail.hubStage)}`}>
                    {detail.hubStage ? HUB_STAGE_LABELS[detail.hubStage] : '—'}
                  </span>
                </div>

                <div>
                  <h3 className="mb-1 text-sm font-semibold">Rantai Dokumen</h3>
                  {detail.chain.deliveryOrders.length + detail.chain.deliveryReports.length + detail.chain.invoices.length === 0 ? (
                    <p className="text-sm text-slate-500">Belum ada dokumen lanjutan.</p>
                  ) : (
                    <ul className="space-y-1 text-sm">
                      {detail.chain.deliveryOrders.map((d) => (
                        <li key={d.id}>Surat Jalan/DO: <b>{d.docNumber}</b> · {d.docDate?.slice(0, 10)} · {d.status}{d.postingStatus ? ` · ${d.postingStatus}` : ''}</li>
                      ))}
                      {detail.chain.deliveryReports.map((d) => (
                        <li key={d.id}>Laporan Penerimaan/BAST: <b>{d.docNumber}</b> · {d.docDate?.slice(0, 10)} · {d.status}</li>
                      ))}
                      {detail.chain.invoices.map((d) => (
                        <li key={d.id}>Invoice: <b>{d.docNumber}</b> · {d.docDate?.slice(0, 10)} · {d.status} · {d.settlementStatus ?? ''} · {d.grandTotal ? formatRupiah(Number(d.grandTotal)) : ''}</li>
                      ))}
                    </ul>
                  )}
                </div>

                <div>
                  <h3 className="mb-1 text-sm font-semibold">Riwayat Status</h3>
                  {detail.statusLogs.length === 0 ? (
                    <p className="text-sm text-slate-500">Belum ada riwayat.</p>
                  ) : (
                    <ul className="space-y-1 text-sm">
                      {detail.statusLogs.map((l) => (
                        <li key={l.id}>
                          <b>{HUB_STAGE_LABELS[l.hubStatus]}</b> · {new Date(l.createdAt).toLocaleString('id-ID')} · {l.source}
                          {l.note ? ` · ${l.note}` : ''}
                        </li>
                      ))}
                    </ul>
                  )}
                </div>

                <div className="flex justify-end">
                  <Button variant="ghost" onClick={() => setDetail(null)}>Tutup</Button>
                </div>
              </div>
            )}
          </Card>
        </div>
      )}
    </div>
  );
}

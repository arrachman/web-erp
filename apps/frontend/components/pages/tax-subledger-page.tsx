'use client';

/**
 * A4 — Pajak Pengadaan: subledger PPN per transaksi (sinkron dari invoice
 * terposting), nomor faktur pajak + ekspor Coretax, status pelaporan,
 * bukti potong PPh 22/23 (tab terpisah), dan laporan pajak bulanan untuk
 * konsultan pajak. Atomic tier: Page.
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
  TAX_ENTRY_TYPE_LABELS,
  assignFaktur,
  downloadCoretax,
  downloadMonthlyReport,
  getMonthlyReport,
  listTaxEntries,
  setTaxEntryStatus,
  syncTaxSubledger,
  type MonthlyReport,
  type TaxEntry,
  type TaxEntryType,
} from '@/lib/api/tax-subledger';
import { TaxSubledgerBupot } from './tax-subledger-bupot';
import { DocModal, fmtDate } from './document-packages-shared';

const rp = (v: string | number) => formatRupiah(Number(v) || 0);
const ENTRY_TYPES = Object.keys(TAX_ENTRY_TYPE_LABELS) as TaxEntryType[];
const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'Mei', 'Jun', 'Jul', 'Agu', 'Sep', 'Okt', 'Nov', 'Des'];

function entryStatusBadge(s: TaxEntry['status']) {
  if (s === 'REPORTED') return <Badge variant="success">Dilaporkan</Badge>;
  if (s === 'CONFIRMED') return <Badge variant="info">Terkonfirmasi</Badge>;
  if (s === 'CANCELLED') return <Badge variant="default">Dibatalkan</Badge>;
  return <Badge variant="warn">Draft</Badge>;
}

export function TaxSubledgerPage() {
  const [tab, setTab] = React.useState<'subledger' | 'bupot' | 'laporan'>('subledger');

  // ── Subledger ─────────────────────────────────────────────────────────────
  const [search, setSearch] = React.useState('');
  const [debouncedSearch, setDebouncedSearch] = React.useState('');
  const [type, setType] = React.useState('');
  const [status, setStatus] = React.useState('');
  const [year, setYear] = React.useState('2026');
  const [month, setMonth] = React.useState('');
  const { page, pageSize, setPage } = useListPagination('/finance/tax-subledger');
  const [syncing, setSyncing] = React.useState(false);
  const [fakturTarget, setFakturTarget] = React.useState<TaxEntry | null>(null);
  const [fakturNo, setFakturNo] = React.useState('');
  const [fakturDate, setFakturDate] = React.useState('');

  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);
  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, type, status, year, month, pageSize, setPage]);

  const { rows, meta, loading, reload } = useErpList<TaxEntry>(
    () =>
      listTaxEntries({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        taxEntryType: (type || undefined) as TaxEntryType | undefined,
        status: status || undefined,
        year: year ? Number(year) : undefined,
        month: month ? Number(month) : undefined,
      }),
    [page, pageSize, debouncedSearch, type, status, year, month],
  );

  const runSync = async () => {
    setSyncing(true);
    try {
      const r = await syncTaxSubledger();
      notify(`Sinkron selesai: ${r.created} entri baru, ${r.skipped} sudah ada`, 'success');
      reload();
    } catch {
      notify('Sinkron gagal', 'danger');
    } finally {
      setSyncing(false);
    }
  };

  const runStatus = async (row: TaxEntry, next: string) => {
    try {
      await setTaxEntryStatus(row.id, next);
      notify(`Status entri: ${next}`, 'success');
      reload();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal mengubah status', 'danger');
    }
  };

  const runFaktur = async () => {
    if (!fakturTarget || !fakturNo.trim()) return;
    try {
      await assignFaktur(fakturTarget.id, fakturNo.trim(), fakturDate || undefined);
      notify('Nomor faktur pajak tersimpan (termasuk di invoice sumber)', 'success');
      setFakturTarget(null);
      setFakturNo('');
      setFakturDate('');
      reload();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan faktur', 'danger');
    }
  };

  // ── Laporan bulanan ───────────────────────────────────────────────────────
  const [repYear, setRepYear] = React.useState(2026);
  const [repMonth, setRepMonth] = React.useState(10);
  const [report, setReport] = React.useState<MonthlyReport | null>(null);
  React.useEffect(() => {
    if (tab !== 'laporan') return;
    getMonthlyReport(repYear, repMonth).then(setReport).catch(() => setReport(null));
  }, [tab, repYear, repMonth]);

  return (
    <div className="space-y-4 p-4">
      <div>
        <h1 className="text-xl font-semibold">Pajak Pengadaan</h1>
        <p className="text-sm text-slate-500">
          Subledger PPN per transaksi, faktur pajak &amp; ekspor Coretax, bukti potong PPh 22/23 bendahara, dan laporan bulanan.
        </p>
      </div>

      <div className="flex gap-2">
        {(
          [
            ['subledger', 'Subledger PPN'],
            ['bupot', 'Bukti Potong'],
            ['laporan', 'Laporan Bulanan'],
          ] as const
        ).map(([key, label]) => (
          <Button key={key} variant={tab === key ? 'primary' : 'ghost'} onClick={() => setTab(key)}>
            {label}
          </Button>
        ))}
      </div>

      {tab === 'bupot' && <TaxSubledgerBupot />}

      {tab === 'laporan' && (
        <Card>
          <div className="space-y-4 px-4 py-4">
            <div className="flex flex-wrap items-center gap-2">
              <select className="rounded-md border px-2 py-1.5 text-sm" value={repMonth} onChange={(e) => setRepMonth(Number(e.target.value))}>
                {MONTHS.map((m, i) => (
                  <option key={m} value={i + 1}>{m}</option>
                ))}
              </select>
              <Input value={String(repYear)} onChange={(e) => setRepYear(Number(e.target.value) || 2026)} />
              <Button variant="ghost" onClick={() => void downloadMonthlyReport(repYear, repMonth)}>Ekspor CSV</Button>
            </div>
            {report ? (
              <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
                {[
                  ['PPN Keluaran', report.ppnKeluaran.tax, `${report.ppnKeluaran.count} entri · DPP ${rp(report.ppnKeluaran.dpp)}`],
                  ['PPN Masukan', report.ppnMasukan.tax, `${report.ppnMasukan.count} entri · DPP ${rp(report.ppnMasukan.dpp)}`],
                  ['PPN Netto', report.ppnNetto, report.ppnNetto >= 0 ? 'Kurang bayar' : 'Lebih bayar'],
                  ['PPh 22 (bukti diterima)', report.pph22.certified, `Dipotong ${rp(report.pph22.tax)}`],
                  ['PPh 23 (bukti diterima)', report.pph23.certified, `Dipotong ${rp(report.pph23.tax)}`],
                  ['Entri Draft', report.draftCount, 'Belum dikonfirmasi'],
                  ['Entri Dilaporkan', report.reportedCount, 'Sudah masuk SPT'],
                ].map(([label, value, sub]) => (
                  <div key={label as string} className="rounded-md border px-3 py-3">
                    <div className="text-xs text-slate-500">{label}</div>
                    <div className="text-lg font-semibold">{typeof value === 'number' && (label as string).includes('Entri') ? value : rp(value as number)}</div>
                    <div className="text-xs text-slate-400">{sub}</div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-sm text-slate-400">Memuat laporan…</div>
            )}
          </div>
        </Card>
      )}

      {tab === 'subledger' && (
        <Card>
          <div className="space-y-3 px-4 py-4">
            <div className="flex flex-wrap items-center gap-2">
              <Input placeholder="Cari dokumen / partner / faktur…" value={search} onChange={(e) => setSearch(e.target.value)} />
              <select className="rounded-md border px-2 py-1.5 text-sm" value={type} onChange={(e) => setType(e.target.value)}>
                <option value="">Semua Jenis</option>
                {ENTRY_TYPES.map((t) => (
                  <option key={t} value={t}>{TAX_ENTRY_TYPE_LABELS[t]}</option>
                ))}
              </select>
              <select className="rounded-md border px-2 py-1.5 text-sm" value={status} onChange={(e) => setStatus(e.target.value)}>
                <option value="">Semua Status</option>
                <option value="DRAFT">Draft</option>
                <option value="CONFIRMED">Terkonfirmasi</option>
                <option value="REPORTED">Dilaporkan</option>
                <option value="CANCELLED">Dibatalkan</option>
              </select>
              <Input placeholder="Tahun" value={year} onChange={(e) => setYear(e.target.value)} />
              <select className="rounded-md border px-2 py-1.5 text-sm" value={month} onChange={(e) => setMonth(e.target.value)}>
                <option value="">Setahun</option>
                {MONTHS.map((m, i) => (
                  <option key={m} value={i + 1}>{m}</option>
                ))}
              </select>
              <span className="flex-1" />
              <Button variant="ghost" disabled={!month} onClick={() => void downloadCoretax(Number(year), Number(month))}>
                Ekspor Coretax
              </Button>
              <Button variant="primary" disabled={syncing} onClick={() => void runSync()}>
                {syncing ? 'Menyinkronkan…' : 'Sinkron dari Invoice'}
              </Button>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-slate-500">
                    <th className="py-2 pr-2">Dokumen</th>
                    <th className="py-2 pr-2">Partner</th>
                    <th className="py-2 pr-2">Jenis</th>
                    <th className="py-2 pr-2">DPP</th>
                    <th className="py-2 pr-2">Pajak</th>
                    <th className="py-2 pr-2">Faktur Pajak</th>
                    <th className="py-2 pr-2">Status</th>
                    <th className="py-2">Aksi</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.map((r) => (
                    <tr key={r.id} className="border-b">
                      <td className="py-2 pr-2 font-medium">
                        {r.docNumber}
                        <div className="text-xs text-slate-400">{fmtDate(r.transactionDate)} · {r.tax?.code ?? ''}</div>
                      </td>
                      <td className="py-2 pr-2">{r.partnerName ?? '-'}</td>
                      <td className="py-2 pr-2">{TAX_ENTRY_TYPE_LABELS[r.taxEntryType]}</td>
                      <td className="py-2 pr-2">{rp(r.dpp)}</td>
                      <td className="py-2 pr-2">{rp(r.taxAmount)}<div className="text-xs text-slate-400">{r.taxRate}%</div></td>
                      <td className="py-2 pr-2">{r.fakturNumber ?? <span className="text-slate-400">belum ada</span>}</td>
                      <td className="py-2 pr-2">{entryStatusBadge(r.status)}</td>
                      <td className="py-2">
                        <div className="flex flex-wrap gap-1">
                          {(r.taxEntryType === 'PPN_KELUARAN' || r.taxEntryType === 'PPN_MASUKAN') && r.status !== 'CANCELLED' && (
                            <Button variant="ghost" onClick={() => { setFakturTarget(r); setFakturNo(r.fakturNumber ?? ''); }}>
                              Faktur
                            </Button>
                          )}
                          {r.status === 'DRAFT' && <Button variant="ghost" onClick={() => void runStatus(r, 'CONFIRMED')}>Konfirmasi</Button>}
                          {r.status === 'CONFIRMED' && <Button variant="ghost" onClick={() => void runStatus(r, 'REPORTED')}>Laporkan</Button>}
                          {r.status !== 'REPORTED' && r.status !== 'CANCELLED' && (
                            <Button variant="ghost" onClick={() => void runStatus(r, 'CANCELLED')}>Batalkan</Button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                  {!loading && rows.length === 0 && (
                    <tr>
                      <td colSpan={8} className="py-4 text-center text-slate-400">
                        Belum ada entri pajak. Klik “Sinkron dari Invoice” untuk membangun subledger dari invoice terposting.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
            <div className="flex items-center justify-between text-sm text-slate-500">
              <span>Total {meta?.total ?? 0} entri</span>
              <span>
                Halaman {page} dari {meta?.totalPages ?? 1}{' '}
                <Button variant="ghost" disabled={page <= 1} onClick={() => setPage(page - 1)}>‹</Button>
                <Button variant="ghost" disabled={!meta || page >= meta.totalPages} onClick={() => setPage(page + 1)}>›</Button>
              </span>
            </div>
          </div>
        </Card>
      )}

      {fakturTarget && (
        <DocModal title={`Faktur Pajak — ${fakturTarget.docNumber}`} onClose={() => setFakturTarget(null)}>
          <Input placeholder="Nomor faktur pajak (dari Coretax)" value={fakturNo} onChange={(e) => setFakturNo(e.target.value)} />
          <Input type="date" value={fakturDate} onChange={(e) => setFakturDate(e.target.value)} />
          <Button variant="primary" disabled={!fakturNo.trim()} onClick={() => void runFaktur()}>Simpan Faktur</Button>
        </DocModal>
      )}
    </div>
  );
}

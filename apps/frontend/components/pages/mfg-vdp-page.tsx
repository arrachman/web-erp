'use client';

/**
 * Fase 2 P4 — Data Variabel (VDP): dataset data variabel per job cetak
 * (rapor, sertifikat, buku induk). Impor CSV, validasi kolom wajib, versi
 * teraudit; dataset membeku otomatis saat job mulai cetak. Data siswa asli
 * hanya dari sekolah — halaman ini tidak membuat data siswa.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { confirmAction, notify } from '@/lib/feedback';
import { listPrintJobs, type PrintJob } from '@/lib/api/mfg-print-jobs';
import {
  createDataset,
  deleteDataset,
  importDatasetRows,
  listDatasetRows,
  listJobDatasets,
  lockDataset,
  type VdpDataset,
  type VdpRow,
} from '@/lib/api/mfg-vdp';

const selectCls = 'h-9 rounded-md border border-input bg-background px-2 text-sm';

export function MfgVdpPage() {
  const [jobs, setJobs] = React.useState<PrintJob[]>([]);
  const [jobId, setJobId] = React.useState('');
  const [datasets, setDatasets] = React.useState<VdpDataset[]>([]);
  const [jobStage, setJobStage] = React.useState('');
  const [printQuantity, setPrintQuantity] = React.useState('0');
  const [selectedId, setSelectedId] = React.useState('');
  const [rows, setRows] = React.useState<VdpRow[]>([]);
  const [onlyInvalid, setOnlyInvalid] = React.useState(false);
  const [busy, setBusy] = React.useState(false);

  const [dsName, setDsName] = React.useState('');
  const [dsFile, setDsFile] = React.useState('');
  const [csv, setCsv] = React.useState('');

  const loadDatasets = React.useCallback(async (id: string) => {
    if (!id) return;
    try {
      const res = await listJobDatasets(id);
      setDatasets(res.data);
      setJobStage(res.jobStage);
      setPrintQuantity(res.printQuantity);
      if (res.data.length && !res.data.some((d) => d.id === selectedId)) {
        setSelectedId(res.data[0].id);
      }
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat dataset.', 'danger');
    }
  }, [selectedId]);

  const loadRows = React.useCallback(async () => {
    if (!selectedId) return setRows([]);
    try {
      setRows(await listDatasetRows(selectedId, onlyInvalid));
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat baris.', 'danger');
    }
  }, [selectedId, onlyInvalid]);

  React.useEffect(() => {
    listPrintJobs({ limit: 100 }).then((r) => setJobs((r as any)?.data ?? [])).catch(() => setJobs([]));
  }, []);

  React.useEffect(() => {
    if (jobId) void loadDatasets(jobId);
  }, [jobId, loadDatasets]);

  React.useEffect(() => {
    void loadRows();
  }, [loadRows]);

  const selected = datasets.find((d) => d.id === selectedId) ?? null;
  const locked = selected?.status === 'TERKUNCI';

  const create = async () => {
    if (!jobId || !dsName) return notify('Pilih job dan isi nama dataset.', 'danger');
    setBusy(true);
    try {
      const ds = await createDataset(jobId, { name: dsName, sourceFilename: dsFile || undefined });
      setDsName('');
      setDsFile('');
      setSelectedId(ds.id);
      notify('Dataset dibuat.', 'success');
      void loadDatasets(jobId);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal membuat dataset.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  const doImport = async () => {
    if (!selectedId || !csv.trim()) return notify('Teks CSV masih kosong.', 'danger');
    setBusy(true);
    try {
      const ds = await importDatasetRows(selectedId, { csvText: csv, sourceFilename: dsFile || undefined });
      setCsv('');
      notify(`Impor selesai: ${ds.validCount}/${ds.rowCount} baris valid (versi ${ds.version}).`, ds.invalidCount ? 'warn' : 'success');
      void loadDatasets(jobId);
      void loadRows();
    } catch (e: any) {
      notify(e?.message ?? 'Impor gagal.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-lg font-semibold">Data Variabel (VDP)</h1>
        <p className="text-sm text-muted-foreground">
          Dataset personalisasi per job (nama siswa/kelas untuk rapor, sertifikat, buku induk). Jumlah baris valid = jumlah cetak variabel. Dataset terkunci otomatis saat job mulai cetak.
        </p>
      </div>

      <Card className="p-3">
        <div className="flex flex-wrap items-end gap-3">
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Job Cetak</label>
            <select className={`${selectCls} w-96`} value={jobId} onChange={(e) => { setJobId(e.target.value); setSelectedId(''); }}>
              <option value="">— pilih job —</option>
              {jobs.map((j) => (
                <option key={j.id} value={j.id}>
                  {j.workOrderDocNumber} — {j.title ?? j.itemName ?? ''} ({j.stage})
                </option>
              ))}
            </select>
          </div>
          {jobId && (
            <div className="text-sm text-muted-foreground">
              Tahap job: <span className="font-medium text-foreground">{jobStage}</span> · Oplah: {Number(printQuantity).toLocaleString('id-ID')}
            </div>
          )}
        </div>
      </Card>

      {jobId && (
        <>
          <Card className="p-0">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b text-left text-xs uppercase text-muted-foreground">
                  <th className="px-3 py-2">Dataset</th>
                  <th className="px-3 py-2">Sumber</th>
                  <th className="px-3 py-2 text-right">Versi</th>
                  <th className="px-3 py-2 text-right">Baris</th>
                  <th className="px-3 py-2 text-right">Valid</th>
                  <th className="px-3 py-2 text-right">Invalid</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2">Aksi</th>
                </tr>
              </thead>
              <tbody>
                {datasets.map((d) => (
                  <tr
                    key={d.id}
                    className={`cursor-pointer border-b hover:bg-muted/40 ${d.id === selectedId ? 'bg-muted/60' : ''}`}
                    onClick={() => setSelectedId(d.id)}
                  >
                    <td className="px-3 py-2 font-medium">{d.name}</td>
                    <td className="px-3 py-2">{d.sourceFilename ?? '—'}</td>
                    <td className="px-3 py-2 text-right">{d.version}</td>
                    <td className="px-3 py-2 text-right">{d.rowCount}</td>
                    <td className="px-3 py-2 text-right">{d.validCount}</td>
                    <td className="px-3 py-2 text-right">{d.invalidCount}</td>
                    <td className="px-3 py-2">
                      {d.status === 'TERKUNCI' ? <Badge variant="danger">Terkunci</Badge> : <Badge variant="info">Draft</Badge>}
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex gap-1">
                        {d.status !== 'TERKUNCI' && (
                          <Button variant="ghost" disabled={busy} onClick={(e) => { e.stopPropagation(); void (async () => { await lockDataset(d.id); notify('Dataset dikunci.', 'success'); void loadDatasets(jobId); })(); }}>
                            Kunci
                          </Button>
                        )}
                        {d.status !== 'TERKUNCI' && (
                          <Button variant="ghost" disabled={busy} onClick={(e) => {
                            e.stopPropagation();
                            confirmAction({
                              message: `Hapus dataset "${d.name}" beserta barisnya?`,
                              onConfirm: () => void (async () => { await deleteDataset(d.id); notify('Dataset dihapus.', 'success'); void loadDatasets(jobId); })(),
                            });
                          }}>
                            Hapus
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
                {!datasets.length && (
                  <tr><td colSpan={8} className="px-3 py-6 text-center text-muted-foreground">Belum ada dataset untuk job ini.</td></tr>
                )}
              </tbody>
            </table>
          </Card>

          <Card className="p-4">
            <h3 className="mb-2 text-sm font-semibold">Dataset baru</h3>
            <div className="flex flex-wrap items-end gap-2">
              <Input className="w-72" placeholder="Nama dataset (mis. Nama Rapor 4A)" value={dsName} onChange={(e) => setDsName(e.target.value)} />
              <Input className="w-56" placeholder="Nama file sumber (opsional)" value={dsFile} onChange={(e) => setDsFile(e.target.value)} />
              <Button disabled={busy} onClick={() => void create()}>+ Dataset</Button>
            </div>
          </Card>

          {selected && (
            <Card className="p-4">
              <h3 className="mb-2 text-sm font-semibold">
                Baris dataset: {selected.name} {locked && <span className="text-red-600">(terkunci)</span>}
              </h3>
              {!locked && (
                <div className="mb-3 border-b pb-3">
                  <p className="mb-2 text-xs text-muted-foreground">
                    Tempel CSV (baris pertama = header kolom). Impor menggantikan baris lama dan menaikkan versi. Kolom wajib bawaan: semua kolom.
                  </p>
                  <textarea
                    className="h-28 w-full rounded-md border border-input bg-background p-2 font-mono text-xs"
                    placeholder={'Nama,Kelas,NIS\nAisyah Putri,4A,1001\nBima Pratama,4A,1002'}
                    value={csv}
                    onChange={(e) => setCsv(e.target.value)}
                  />
                  <div className="mt-2">
                    <Button disabled={busy} onClick={() => void doImport()}>Impor Baris</Button>
                  </div>
                </div>
              )}
              <div className="mb-2 flex items-center gap-2">
                <label className="flex items-center gap-1 text-sm">
                  <input type="checkbox" checked={onlyInvalid} onChange={(e) => setOnlyInvalid(e.target.checked)} />
                  Hanya baris invalid
                </label>
                <span className="text-xs text-muted-foreground">Menampilkan maks. 100 baris.</span>
              </div>
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-xs uppercase text-muted-foreground">
                    <th className="py-1 pr-2 w-12">No</th>
                    <th className="py-1 pr-2">Data</th>
                    <th className="py-1 pr-2 w-40">Validasi</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.map((r) => (
                    <tr key={r.id} className="border-b">
                      <td className="py-1 pr-2">{r.rowNo}</td>
                      <td className="py-1 pr-2">
                        {Object.entries(r.data).map(([k, v]) => (
                          <span key={k} className="mr-3"><span className="text-muted-foreground">{k}:</span> {v}</span>
                        ))}
                      </td>
                      <td className="py-1 pr-2">
                        {r.isValid ? <Badge variant="success">Valid</Badge> : <Badge variant="danger">{r.errorNote ?? 'Invalid'}</Badge>}
                      </td>
                    </tr>
                  ))}
                  {!rows.length && (
                    <tr><td colSpan={3} className="py-3 text-center text-muted-foreground">Tidak ada baris.</td></tr>
                  )}
                </tbody>
              </table>
            </Card>
          )}
        </>
      )}
    </div>
  );
}

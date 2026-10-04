'use client';

/**
 * Fase 2 P6 — Packing per Siswa: dari sebuah Packing List, buat unit kemas
 * per siswa (roster nama,kelas), tandai yang sudah dipacking, pantau progres,
 * dan ekspor CSV untuk label. Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { notify } from '@/lib/feedback';
import {
  deletePackingUnit,
  generatePackingUnits,
  getPackingOverview,
  listPackingUnits,
  setPackingUnitPacked,
  type PackingListOverview,
  type PackingUnit,
} from '@/lib/api/sls-packing-units';

const selectCls = 'h-9 rounded-md border border-input bg-background px-2 text-sm';

export function SlsPackingUnitsPage() {
  const [overview, setOverview] = React.useState<PackingListOverview[]>([]);
  const [selectedId, setSelectedId] = React.useState('');
  const [units, setUnits] = React.useState<PackingUnit[]>([]);
  const [progress, setProgress] = React.useState({ total: 0, packed: 0 });
  const [roster, setRoster] = React.useState('');
  const [busy, setBusy] = React.useState(false);

  const loadOverview = React.useCallback(async () => {
    try {
      setOverview(await getPackingOverview());
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat packing list.', 'danger');
    }
  }, []);

  const loadUnits = React.useCallback(async (id: string) => {
    if (!id) return;
    try {
      const res = await listPackingUnits(id);
      setUnits(res.data);
      setProgress(res.progress);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat unit.', 'danger');
    }
  }, []);

  React.useEffect(() => {
    void loadOverview();
  }, [loadOverview]);

  React.useEffect(() => {
    if (selectedId) void loadUnits(selectedId);
  }, [selectedId, loadUnits]);

  const generate = async () => {
    if (!selectedId) return notify('Pilih packing list dulu.', 'danger');
    if (!roster.trim()) return notify('Roster masih kosong.', 'danger');
    setBusy(true);
    try {
      const res = await generatePackingUnits(selectedId, { rosterCsv: roster });
      setUnits(res.data);
      setProgress(res.progress);
      setRoster('');
      notify(`${res.progress.total} unit pada packing list ini.`, 'success');
      void loadOverview();
    } catch (e: any) {
      notify(e?.message ?? 'Gagal membuat unit.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  const exportCsv = () => {
    const head = 'No,Siswa,Kelas,Isi,Status\n';
    const body = units
      .map((u) =>
        [
          u.sequenceNo,
          `"${u.studentName.replaceAll('"', '""')}"`,
          `"${(u.className ?? '').replaceAll('"', '""')}"`,
          `"${u.contents.map((c) => `${c.name ?? c.itemId} x${Number(c.quantity)}`).join('; ').replaceAll('"', '""')}"`,
          u.status,
        ].join(','),
      )
      .join('\n');
    const blob = new Blob([head + body], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'packing-per-siswa.csv';
    a.click();
    URL.revokeObjectURL(url);
  };

  const pct = progress.total ? Math.round((progress.packed / progress.total) * 100) : 0;

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-lg font-semibold">Packing per Siswa</h1>
        <p className="text-sm text-muted-foreground">
          Pecah packing list menjadi unit per siswa/kelas, tandai yang sudah dikemas, dan ekspor daftar label.
        </p>
      </div>

      <Card className="p-3">
        <div className="flex flex-wrap items-end gap-3">
          <div>
            <label className="mb-1 block text-xs font-medium text-muted-foreground">Packing List</label>
            <select
              className={`${selectCls} w-80`}
              value={selectedId}
              onChange={(e) => setSelectedId(e.target.value)}
            >
              <option value="">— pilih packing list —</option>
              {overview.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.docNumber} — {o.customerName ?? '—'} ({o.units.packed}/{o.units.total})
                </option>
              ))}
            </select>
          </div>
          {selectedId && (
            <div className="min-w-56 flex-1">
              <div className="mb-1 text-xs text-muted-foreground">
                Progres: {progress.packed}/{progress.total} packed ({pct}%)
              </div>
              <div className="h-2 w-full rounded bg-muted">
                <div className="h-2 rounded bg-green-600" style={{ width: `${pct}%` }} />
              </div>
            </div>
          )}
          {selectedId && units.length > 0 && (
            <Button variant="ghost" onClick={exportCsv}>
              Ekspor CSV Label
            </Button>
          )}
        </div>
      </Card>

      {selectedId && (
        <Card className="p-4">
          <h3 className="mb-2 text-sm font-semibold">Tambah unit dari roster</h3>
          <p className="mb-2 text-xs text-muted-foreground">
            Satu siswa per baris, format: Nama,Kelas — isi setiap unit mengikuti baris item Packing List.
          </p>
          <textarea
            className="h-28 w-full rounded-md border border-input bg-background p-2 text-sm"
            placeholder={'Aisyah Putri,4A\nBima Pratama,4A\nCitra Lestari,4B'}
            value={roster}
            onChange={(e) => setRoster(e.target.value)}
          />
          <div className="mt-2">
            <Button disabled={busy} onClick={() => void generate()}>
              {busy ? 'Membuat…' : 'Buat Unit'}
            </Button>
          </div>
        </Card>
      )}

      {selectedId && (
        <Card className="p-0">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-xs uppercase text-muted-foreground">
                <th className="px-3 py-2 w-12">No</th>
                <th className="px-3 py-2">Siswa</th>
                <th className="px-3 py-2">Kelas</th>
                <th className="px-3 py-2">Isi Unit</th>
                <th className="px-3 py-2">Status</th>
                <th className="px-3 py-2">Aksi</th>
              </tr>
            </thead>
            <tbody>
              {units.map((u) => (
                <tr key={u.id} className="border-b hover:bg-muted/40">
                  <td className="px-3 py-2">{u.sequenceNo}</td>
                  <td className="px-3 py-2 font-medium">{u.studentName}</td>
                  <td className="px-3 py-2">{u.className ?? '—'}</td>
                  <td className="px-3 py-2">
                    {u.contents.length
                      ? u.contents.map((c) => `${c.name ?? c.itemId} ×${Number(c.quantity)}`).join(', ')
                      : '—'}
                  </td>
                  <td className="px-3 py-2">
                    {u.status === 'PACKED' ? (
                      <Badge variant="success">Packed</Badge>
                    ) : (
                      <Badge variant="default">Pending</Badge>
                    )}
                  </td>
                  <td className="px-3 py-2">
                    <div className="flex gap-1">
                      <Button
                        variant="ghost"
                        disabled={busy}
                        onClick={async () => {
                          try {
                            await setPackingUnitPacked(u.id, u.status !== 'PACKED');
                            void loadUnits(selectedId);
                            void loadOverview();
                          } catch (e: any) {
                            notify(e?.message ?? 'Gagal mengubah status.', 'danger');
                          }
                        }}
                      >
                        {u.status === 'PACKED' ? 'Batal Packed' : 'Tandai Packed'}
                      </Button>
                      <Button
                        variant="ghost"
                        disabled={busy}
                        onClick={async () => {
                          try {
                            await deletePackingUnit(u.id);
                            notify('Unit dihapus.', 'success');
                            void loadUnits(selectedId);
                          } catch (e: any) {
                            notify(e?.message ?? 'Gagal menghapus.', 'danger');
                          }
                        }}
                      >
                        Hapus
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
              {!units.length && (
                <tr>
                  <td colSpan={6} className="px-3 py-8 text-center text-muted-foreground">
                    Belum ada unit — buat dari roster di atas.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </Card>
      )}
    </div>
  );
}

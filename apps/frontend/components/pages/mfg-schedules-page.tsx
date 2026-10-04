'use client';

/**
 * Fase 2 P3 — Jadwal Produksi: master mesin cetak + papan jadwal job per
 * mesin. Jadwal yang bentrok pada mesin yang sama ditolak server (400)
 * beserta rinciannya. Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { notify } from '@/lib/feedback';
import { listPrintJobs, type PrintJob } from '@/lib/api/mfg-print-jobs';
import {
  MACHINE_STATUSES,
  MACHINE_TYPES,
  createMachine,
  createSchedule,
  deleteMachine,
  deleteSchedule,
  listMachines,
  listSchedules,
  setScheduleStatus,
  updateMachine,
  type JobSchedule,
  type MachineStatus,
  type MachineType,
  type MfgMachine,
  type ScheduleStatus,
} from '@/lib/api/mfg-schedules';

const selectCls = 'h-9 rounded-md border border-input bg-background px-2 text-sm';
const STATUS_VARIANT: Record<ScheduleStatus, 'default' | 'info' | 'success' | 'danger'> = {
  TERJADWAL: 'default',
  BERJALAN: 'info',
  SELESAI: 'success',
  BATAL: 'danger',
};
const fmt = (iso: string) =>
  new Date(iso).toLocaleString('id-ID', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });

function defaultRange() {
  const now = new Date();
  const day = now.getDay() === 0 ? 7 : now.getDay();
  const mon = new Date(now);
  mon.setDate(now.getDate() - (day - 1));
  const sun = new Date(mon);
  sun.setDate(mon.getDate() + 6);
  const iso = (d: Date) => d.toISOString().slice(0, 10);
  return { from: iso(mon), to: iso(sun) };
}

export function MfgSchedulesPage() {
  const [machines, setMachines] = React.useState<MfgMachine[]>([]);
  const [jobs, setJobs] = React.useState<PrintJob[]>([]);
  const [schedules, setSchedules] = React.useState<JobSchedule[]>([]);
  const [range, setRange] = React.useState(defaultRange);
  const [busy, setBusy] = React.useState(false);

  const [mCode, setMCode] = React.useState('');
  const [mName, setMName] = React.useState('');
  const [mType, setMType] = React.useState<MachineType>('OFFSET');
  const [mCap, setMCap] = React.useState('');

  const [sJob, setSJob] = React.useState('');
  const [sMachine, setSMachine] = React.useState('');
  const [sStage, setSStage] = React.useState('CETAK');
  const [sStart, setSStart] = React.useState('');
  const [sEnd, setSEnd] = React.useState('');

  const loadBoard = React.useCallback(async () => {
    try {
      const [m, s] = await Promise.all([
        listMachines(),
        listSchedules({ from: `${range.from}T00:00:00`, to: `${range.to}T23:59:59` }),
      ]);
      setMachines(m);
      setSchedules(s);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat jadwal.', 'danger');
    }
  }, [range]);

  React.useEffect(() => {
    void loadBoard();
    listPrintJobs({ limit: 100 })
      .then((r) => setJobs((r as any)?.data ?? []))
      .catch(() => setJobs([]));
  }, [loadBoard]);

  const addMachine = async () => {
    if (!mCode || !mName) return notify('Kode & nama mesin wajib diisi.', 'danger');
    setBusy(true);
    try {
      await createMachine({ code: mCode, name: mName, machineType: mType, capacityPerHour: mCap || '0', capacityUnit: 'lembar/jam' });
      setMCode('');
      setMName('');
      setMCap('');
      notify('Mesin ditambahkan.', 'success');
      void loadBoard();
    } catch (e: any) {
      notify(e?.message ?? 'Gagal menambah mesin.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  const addSchedule = async () => {
    if (!sJob || !sMachine || !sStart || !sEnd) return notify('Job, mesin, dan rentang waktu wajib diisi.', 'danger');
    setBusy(true);
    try {
      await createSchedule({ jobId: sJob, machineId: sMachine, stage: sStage, plannedStart: sStart, plannedEnd: sEnd });
      notify('Job terjadwal.', 'success');
      void loadBoard();
    } catch (e: any) {
      notify(e?.message ?? 'Gagal menjadwalkan.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  const act = async (fn: () => Promise<unknown>, ok: string) => {
    setBusy(true);
    try {
      await fn();
      notify(ok, 'success');
      void loadBoard();
    } catch (e: any) {
      notify(e?.message ?? 'Aksi gagal.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-lg font-semibold">Jadwal Produksi</h1>
        <p className="text-sm text-muted-foreground">
          Setiap job terjadwal di mesin dengan kapasitas. Jadwal bentrok pada mesin yang sama ditolak otomatis.
        </p>
      </div>

      <Card className="p-4">
        <h3 className="mb-2 text-sm font-semibold">Mesin Cetak</h3>
        <table className="mb-3 w-full text-sm">
          <thead>
            <tr className="border-b text-left text-xs uppercase text-muted-foreground">
              <th className="py-1 pr-2">Kode</th>
              <th className="py-1 pr-2">Nama</th>
              <th className="py-1 pr-2">Jenis</th>
              <th className="py-1 pr-2 text-right">Kapasitas/Jam</th>
              <th className="py-1 pr-2">Jam Kerja</th>
              <th className="py-1 pr-2">Status</th>
              <th className="w-16" />
            </tr>
          </thead>
          <tbody>
            {machines.map((m) => (
              <tr key={m.id} className="border-b">
                <td className="py-1 pr-2 font-medium">{m.code}</td>
                <td className="py-1 pr-2">{m.name}</td>
                <td className="py-1 pr-2">{m.machineType}</td>
                <td className="py-1 pr-2 text-right">{Number(m.capacityPerHour).toLocaleString('id-ID')} {m.capacityUnit ?? ''}</td>
                <td className="py-1 pr-2">{m.workStart}–{m.workEnd}</td>
                <td className="py-1 pr-2">
                  <select
                    className={selectCls}
                    value={m.status}
                    disabled={busy}
                    onChange={(e) => void act(() => updateMachine(m.id, { status: e.target.value as MachineStatus }), 'Status mesin diubah.')}
                  >
                    {MACHINE_STATUSES.map((s) => (
                      <option key={s} value={s}>{s}</option>
                    ))}
                  </select>
                </td>
                <td>
                  <Button variant="ghost" disabled={busy} onClick={() => void act(() => deleteMachine(m.id), 'Mesin dihapus.')}>✕</Button>
                </td>
              </tr>
            ))}
            {!machines.length && (
              <tr><td colSpan={7} className="py-3 text-center text-muted-foreground">Belum ada mesin.</td></tr>
            )}
          </tbody>
        </table>
        <div className="flex flex-wrap items-end gap-2 border-t pt-3">
          <Input className="w-36" placeholder="Kode (MCN-…)" value={mCode} onChange={(e) => setMCode(e.target.value)} />
          <Input className="w-64" placeholder="Nama mesin" value={mName} onChange={(e) => setMName(e.target.value)} />
          <select className={selectCls} value={mType} onChange={(e) => setMType(e.target.value as MachineType)}>
            {MACHINE_TYPES.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </select>
          <Input type="number" className="w-32" placeholder="Kapasitas/jam" value={mCap} onChange={(e) => setMCap(e.target.value)} />
          <Button disabled={busy} onClick={() => void addMachine()}>+ Mesin</Button>
        </div>
      </Card>

      <Card className="p-4">
        <div className="mb-3 flex flex-wrap items-end gap-2">
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Dari</label>
            <Input type="date" value={range.from} onChange={(e) => setRange((r) => ({ ...r, from: e.target.value }))} />
          </div>
          <div>
            <label className="mb-1 block text-xs text-muted-foreground">Sampai</label>
            <Input type="date" value={range.to} onChange={(e) => setRange((r) => ({ ...r, to: e.target.value }))} />
          </div>
        </div>
        <div className="mb-4 flex flex-wrap items-end gap-2 border-b pb-3">
          <select className={`${selectCls} w-72`} value={sJob} onChange={(e) => setSJob(e.target.value)}>
            <option value="">— pilih job —</option>
            {jobs.filter((j) => j.stage !== 'SELESAI' && j.stage !== 'CANCELLED').map((j) => (
              <option key={j.id} value={j.id}>{j.workOrderDocNumber} — {j.title ?? j.itemName ?? ''}</option>
            ))}
          </select>
          <select className={`${selectCls} w-56`} value={sMachine} onChange={(e) => setSMachine(e.target.value)}>
            <option value="">— pilih mesin —</option>
            {machines.filter((m) => m.status === 'ACTIVE').map((m) => (
              <option key={m.id} value={m.id}>{m.code} — {m.name}</option>
            ))}
          </select>
          <select className={selectCls} value={sStage} onChange={(e) => setSStage(e.target.value)}>
            {['PRE_PRESS', 'CETAK', 'FINISHING', 'QC'].map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
          <Input type="datetime-local" className="w-52" value={sStart} onChange={(e) => setSStart(e.target.value)} />
          <Input type="datetime-local" className="w-52" value={sEnd} onChange={(e) => setSEnd(e.target.value)} />
          <Button disabled={busy} onClick={() => void addSchedule()}>Jadwalkan</Button>
        </div>

        {machines.map((m) => {
          const rows = schedules.filter((s) => s.machineId === m.id);
          return (
            <div key={m.id} className="mb-4">
              <div className="mb-1 text-sm font-semibold">{m.code} — {m.name}</div>
              {rows.length ? (
                <table className="w-full text-sm">
                  <tbody>
                    {rows.map((s) => (
                      <tr key={s.id} className="border-b">
                        <td className="py-1 pr-2 font-medium">{s.workOrderDocNumber}</td>
                        <td className="py-1 pr-2">{s.jobTitle ?? '—'}</td>
                        <td className="py-1 pr-2">{s.stage ?? '—'}</td>
                        <td className="py-1 pr-2">{fmt(s.plannedStart)} → {fmt(s.plannedEnd)}</td>
                        <td className="py-1 pr-2"><Badge variant={STATUS_VARIANT[s.status]}>{s.status}</Badge></td>
                        <td className="py-1">
                          <div className="flex gap-1">
                            {s.status === 'TERJADWAL' && (
                              <Button variant="ghost" disabled={busy} onClick={() => void act(() => setScheduleStatus(s.id, 'BERJALAN'), 'Jadwal dimulai.')}>Mulai</Button>
                            )}
                            {s.status === 'BERJALAN' && (
                              <Button variant="ghost" disabled={busy} onClick={() => void act(() => setScheduleStatus(s.id, 'SELESAI'), 'Jadwal selesai.')}>Selesai</Button>
                            )}
                            {(s.status === 'TERJADWAL' || s.status === 'BERJALAN') && (
                              <Button variant="ghost" disabled={busy} onClick={() => void act(() => setScheduleStatus(s.id, 'BATAL'), 'Jadwal dibatalkan.')}>Batal</Button>
                            )}
                            <Button variant="ghost" disabled={busy} onClick={() => void act(() => deleteSchedule(s.id), 'Jadwal dihapus.')}>✕</Button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              ) : (
                <p className="text-sm text-muted-foreground">Tidak ada jadwal pada rentang ini.</p>
              )}
            </div>
          );
        })}
      </Card>
    </div>
  );
}

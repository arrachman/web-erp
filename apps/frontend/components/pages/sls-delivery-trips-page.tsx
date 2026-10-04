'use client';

/**
 * Fase 2 P7 — Trip Pengiriman: master kendaraan + trip (satu kendaraan,
 * banyak Delivery Order sebagai stops). Stop ditandai TIBA dengan nama
 * penerima → acceptance BAST tertulis → Order Hub bergerak ke DITERIMA.
 * Biaya trip tercatat sebagai dasar Freight Payable. Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { apiGet } from '@/lib/api/client';
import { notify } from '@/lib/feedback';
import { formatRupiah } from '@/lib/format';
import {
  VEHICLE_STATUSES,
  addTripStop,
  arriveTripStop,
  createTrip,
  createVehicle,
  deleteVehicle,
  failTripStop,
  getTrip,
  listTrips,
  listVehicles,
  removeTripStop,
  setTripCosts,
  setTripStatus,
  updateVehicle,
  type DeliveryTrip,
  type SlsVehicle,
  type TripStatus,
  type VehicleStatus,
} from '@/lib/api/sls-delivery-trips';

const selectCls = 'h-9 rounded-md border border-input bg-background px-2 text-sm';
const rp = (v: string | number) => formatRupiah(Number(v) || 0);
const TRIP_VARIANT: Record<TripStatus, 'default' | 'info' | 'success' | 'danger'> = {
  DRAFT: 'default',
  MUAT: 'info',
  BERANGKAT: 'info',
  SELESAI: 'success',
  BATAL: 'danger',
};
const NEXT: Partial<Record<TripStatus, TripStatus>> = {
  DRAFT: 'MUAT',
  MUAT: 'BERANGKAT',
  BERANGKAT: 'SELESAI',
};

interface DoOption {
  id: string;
  docNumber: string;
}

export function SlsDeliveryTripsPage() {
  const [vehicles, setVehicles] = React.useState<SlsVehicle[]>([]);
  const [trips, setTrips] = React.useState<DeliveryTrip[]>([]);
  const [selectedId, setSelectedId] = React.useState('');
  const [trip, setTrip] = React.useState<DeliveryTrip | null>(null);
  const [doOptions, setDoOptions] = React.useState<DoOption[]>([]);
  const [busy, setBusy] = React.useState(false);

  const [vCode, setVCode] = React.useState('');
  const [vName, setVName] = React.useState('');
  const [vPlate, setVPlate] = React.useState('');
  const [tDate, setTDate] = React.useState(() => new Date().toISOString().slice(0, 10));
  const [tVehicle, setTVehicle] = React.useState('');
  const [tDriver, setTDriver] = React.useState('');
  const [stopDo, setStopDo] = React.useState('');
  const [receiver, setReceiver] = React.useState<Record<string, string>>({});
  const [costs, setCosts] = React.useState({ fuelCost: '', tollCost: '', otherCost: '' });

  const loadLists = React.useCallback(async () => {
    try {
      const [v, t] = await Promise.all([listVehicles(), listTrips()]);
      setVehicles(v);
      setTrips(t);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat data trip.', 'danger');
    }
  }, []);

  const loadTrip = React.useCallback(async (id: string) => {
    if (!id) return setTrip(null);
    try {
      const t = await getTrip(id);
      setTrip(t);
      setCosts({
        fuelCost: String(Math.trunc(Number(t.fuelCost) || 0)),
        tollCost: String(Math.trunc(Number(t.tollCost) || 0)),
        otherCost: String(Math.trunc(Number(t.otherCost) || 0)),
      });
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat detail trip.', 'danger');
    }
  }, []);

  React.useEffect(() => {
    void loadLists();
    apiGet<any>('/sls/delivery-orders?limit=100')
      .then((res) => setDoOptions(((res as any)?.data ?? []).map((d: any) => ({ id: d.id, docNumber: d.docNumber }))))
      .catch(() => setDoOptions([]));
  }, [loadLists]);

  React.useEffect(() => {
    void loadTrip(selectedId);
  }, [selectedId, loadTrip]);

  const act = async (fn: () => Promise<unknown>, ok: string) => {
    setBusy(true);
    try {
      await fn();
      notify(ok, 'success');
      void loadLists();
      if (selectedId) void loadTrip(selectedId);
    } catch (e: any) {
      notify(e?.message ?? 'Aksi gagal.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-lg font-semibold">Trip Pengiriman</h1>
        <p className="text-sm text-muted-foreground">
          Satu kendaraan mengantar banyak Delivery Order. Penerima di stop menulis acceptance BAST — Order Hub bergerak ke DITERIMA.
        </p>
      </div>

      <Card className="p-4">
        <h3 className="mb-2 text-sm font-semibold">Kendaraan</h3>
        <table className="mb-3 w-full text-sm">
          <thead>
            <tr className="border-b text-left text-xs uppercase text-muted-foreground">
              <th className="py-1 pr-2">Kode</th>
              <th className="py-1 pr-2">Nama</th>
              <th className="py-1 pr-2">Plat</th>
              <th className="py-1 pr-2">Jenis</th>
              <th className="py-1 pr-2 text-right">Kapasitas (kg)</th>
              <th className="py-1 pr-2">Status</th>
              <th className="w-10" />
            </tr>
          </thead>
          <tbody>
            {vehicles.map((v) => (
              <tr key={v.id} className="border-b">
                <td className="py-1 pr-2 font-medium">{v.code}</td>
                <td className="py-1 pr-2">{v.name}</td>
                <td className="py-1 pr-2">{v.plateNo ?? '—'}</td>
                <td className="py-1 pr-2">{v.vehicleType}</td>
                <td className="py-1 pr-2 text-right">{v.capacityKg ? Number(v.capacityKg).toLocaleString('id-ID') : '—'}</td>
                <td className="py-1 pr-2">
                  <select
                    className={selectCls}
                    value={v.status}
                    disabled={busy}
                    onChange={(e) => void act(() => updateVehicle(v.id, { status: e.target.value as VehicleStatus }), 'Status kendaraan diubah.')}
                  >
                    {VEHICLE_STATUSES.map((s) => (
                      <option key={s} value={s}>{s}</option>
                    ))}
                  </select>
                </td>
                <td>
                  <Button variant="ghost" disabled={busy} onClick={() => void act(() => deleteVehicle(v.id), 'Kendaraan dihapus.')}>✕</Button>
                </td>
              </tr>
            ))}
            {!vehicles.length && (
              <tr><td colSpan={7} className="py-3 text-center text-muted-foreground">Belum ada kendaraan.</td></tr>
            )}
          </tbody>
        </table>
        <div className="flex flex-wrap items-end gap-2 border-t pt-3">
          <Input className="w-36" placeholder="Kode (VH-…)" value={vCode} onChange={(e) => setVCode(e.target.value)} />
          <Input className="w-64" placeholder="Nama kendaraan" value={vName} onChange={(e) => setVName(e.target.value)} />
          <Input className="w-36" placeholder="No plat" value={vPlate} onChange={(e) => setVPlate(e.target.value)} />
          <Button
            disabled={busy}
            onClick={() => {
              if (!vCode || !vName) return notify('Kode & nama kendaraan wajib diisi.', 'danger');
              void act(async () => {
                await createVehicle({ code: vCode, name: vName, plateNo: vPlate || undefined });
                setVCode(''); setVName(''); setVPlate('');
              }, 'Kendaraan ditambahkan.');
            }}
          >
            + Kendaraan
          </Button>
        </div>
      </Card>

      <div className="grid gap-4 lg:grid-cols-5">
        <Card className="p-0 lg:col-span-2">
          <div className="border-b p-3">
            <div className="mb-2 text-sm font-semibold">Trip</div>
            <div className="flex flex-wrap items-end gap-2">
              <Input type="date" className="w-36" value={tDate} onChange={(e) => setTDate(e.target.value)} />
              <select className={`${selectCls} w-48`} value={tVehicle} onChange={(e) => setTVehicle(e.target.value)}>
                <option value="">— kendaraan —</option>
                {vehicles.filter((v) => v.status === 'ACTIVE').map((v) => (
                  <option key={v.id} value={v.id}>{v.code} — {v.name}</option>
                ))}
              </select>
              <Input className="w-40" placeholder="Nama sopir" value={tDriver} onChange={(e) => setTDriver(e.target.value)} />
              <Button
                disabled={busy}
                onClick={() => {
                  if (!tVehicle) return notify('Pilih kendaraan dulu.', 'danger');
                  void act(async () => {
                    const t = await createTrip({ tripDate: tDate, vehicleId: tVehicle, driverName: tDriver || undefined });
                    setSelectedId(t.id);
                  }, 'Trip dibuat.');
                }}
              >
                + Trip
              </Button>
            </div>
          </div>
          <table className="w-full text-sm">
            <tbody>
              {trips.map((t) => (
                <tr
                  key={t.id}
                  className={`cursor-pointer border-b hover:bg-muted/40 ${t.id === selectedId ? 'bg-muted/60' : ''}`}
                  onClick={() => setSelectedId(t.id)}
                >
                  <td className="px-3 py-2 font-medium">{t.docNumber}</td>
                  <td className="px-3 py-2">{t.tripDate}</td>
                  <td className="px-3 py-2">{t.vehicleCode}</td>
                  <td className="px-3 py-2">{t.arrivedCount}/{t.stopCount} stop</td>
                  <td className="px-3 py-2"><Badge variant={TRIP_VARIANT[t.status]}>{t.status}</Badge></td>
                </tr>
              ))}
              {!trips.length && (
                <tr><td className="px-3 py-6 text-center text-muted-foreground">Belum ada trip.</td></tr>
              )}
            </tbody>
          </table>
        </Card>

        <Card className="p-4 lg:col-span-3">
          {!trip ? (
            <p className="text-sm text-muted-foreground">Pilih trip untuk mengelola stops, biaya, dan statusnya.</p>
          ) : (
            <div className="space-y-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <div className="font-semibold">{trip.docNumber} — {trip.vehicleName} ({trip.plateNo ?? '—'})</div>
                  <div className="text-sm text-muted-foreground">
                    {trip.tripDate} · Sopir: {trip.driverName ?? '—'} · Biaya total {rp(trip.totalCost)}
                  </div>
                </div>
                <div className="flex gap-2">
                  {NEXT[trip.status] && (
                    <Button disabled={busy} onClick={() => void act(() => setTripStatus(trip.id, NEXT[trip.status]!), `Trip ${NEXT[trip.status]}.`)}>
                      {NEXT[trip.status] === 'MUAT' ? 'Mulai Muat' : NEXT[trip.status] === 'BERANGKAT' ? 'Berangkat' : 'Selesaikan Trip'}
                    </Button>
                  )}
                  {trip.status !== 'SELESAI' && trip.status !== 'BATAL' && (
                    <Button variant="danger" disabled={busy} onClick={() => void act(() => setTripStatus(trip.id, 'BATAL'), 'Trip dibatalkan.')}>
                      Batalkan
                    </Button>
                  )}
                  <Badge variant={TRIP_VARIANT[trip.status]}>{trip.status}</Badge>
                </div>
              </div>

              {(trip.status === 'DRAFT' || trip.status === 'MUAT') && (
                <div className="flex flex-wrap items-end gap-2 border-b pb-3">
                  <select className={`${selectCls} w-64`} value={stopDo} onChange={(e) => setStopDo(e.target.value)}>
                    <option value="">— pilih Delivery Order —</option>
                    {doOptions.map((d) => (
                      <option key={d.id} value={d.id}>{d.docNumber}</option>
                    ))}
                  </select>
                  <Button
                    disabled={busy}
                    onClick={() => {
                      if (!stopDo) return notify('Pilih DO dulu.', 'danger');
                      void act(async () => { await addTripStop(trip.id, stopDo); setStopDo(''); }, 'Stop ditambahkan.');
                    }}
                  >
                    + Stop
                  </Button>
                </div>
              )}

              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-xs uppercase text-muted-foreground">
                    <th className="py-1 pr-2 w-10">#</th>
                    <th className="py-1 pr-2">DO</th>
                    <th className="py-1 pr-2">Sekolah/Tujuan</th>
                    <th className="py-1 pr-2">Status</th>
                    <th className="py-1 pr-2">Penerima</th>
                    <th className="py-1">Aksi</th>
                  </tr>
                </thead>
                <tbody>
                  {(trip.stops ?? []).map((s) => (
                    <tr key={s.id} className="border-b align-top">
                      <td className="py-1 pr-2">{s.sequenceNo}</td>
                      <td className="py-1 pr-2 font-medium">{s.deliveryOrderDocNumber}</td>
                      <td className="py-1 pr-2">{s.customerName ?? '—'}</td>
                      <td className="py-1 pr-2">
                        {s.status === 'TIBA' ? <Badge variant="success">Tiba</Badge> : s.status === 'GAGAL' ? <Badge variant="danger">Gagal</Badge> : <Badge variant="default">Menunggu</Badge>}
                      </td>
                      <td className="py-1 pr-2">{s.receiverName ?? (s.failureNote ? `Gagal: ${s.failureNote}` : '—')}</td>
                      <td className="py-1">
                        {trip.status === 'BERANGKAT' && s.status === 'MENUNGGU' && (
                          <div className="flex flex-wrap gap-1">
                            <Input
                              className="w-44"
                              placeholder="Nama penerima"
                              value={receiver[s.id] ?? ''}
                              onChange={(e) => setReceiver((r) => ({ ...r, [s.id]: e.target.value }))}
                            />
                            <Button
                              disabled={busy}
                              onClick={() => {
                                const name = (receiver[s.id] ?? '').trim();
                                if (!name) return notify('Nama penerima wajib diisi untuk BAST.', 'danger');
                                void act(() => arriveTripStop(trip.id, s.id, { receiverName: name }), 'Stop tiba — acceptance BAST tercatat.');
                              }}
                            >
                              Tiba
                            </Button>
                            <Button
                              variant="ghost"
                              disabled={busy}
                              onClick={() => {
                                const note = window.prompt('Alasan gagal kirim:');
                                if (note) void act(() => failTripStop(trip.id, s.id, note), 'Stop ditandai gagal.');
                              }}
                            >
                              Gagal
                            </Button>
                          </div>
                        )}
                        {(trip.status === 'DRAFT' || trip.status === 'MUAT') && (
                          <Button variant="ghost" disabled={busy} onClick={() => void act(() => removeTripStop(trip.id, s.id), 'Stop dihapus.')}>✕</Button>
                        )}
                      </td>
                    </tr>
                  ))}
                  {!(trip.stops ?? []).length && (
                    <tr><td colSpan={6} className="py-3 text-center text-muted-foreground">Belum ada stop.</td></tr>
                  )}
                </tbody>
              </table>

              <div className="border-t pt-3">
                <div className="mb-2 text-sm font-semibold">Biaya Trip (dasar Freight Payable)</div>
                <div className="flex flex-wrap items-end gap-2">
                  <Input type="number" className="w-36" placeholder="BBM" value={costs.fuelCost} onChange={(e) => setCosts((c) => ({ ...c, fuelCost: e.target.value }))} />
                  <Input type="number" className="w-36" placeholder="Tol" value={costs.tollCost} onChange={(e) => setCosts((c) => ({ ...c, tollCost: e.target.value }))} />
                  <Input type="number" className="w-36" placeholder="Lain-lain" value={costs.otherCost} onChange={(e) => setCosts((c) => ({ ...c, otherCost: e.target.value }))} />
                  <Button
                    disabled={busy}
                    onClick={() => void act(() => setTripCosts(trip.id, { fuelCost: costs.fuelCost || '0', tollCost: costs.tollCost || '0', otherCost: costs.otherCost || '0' }), 'Biaya trip disimpan.')}
                  >
                    Simpan Biaya
                  </Button>
                  <span className="text-sm text-muted-foreground">Total: {rp(trip.totalCost)}</span>
                </div>
              </div>
            </div>
          )}
        </Card>
      </div>
    </div>
  );
}

'use client';

/**
 * Fase 2 T1 — Lot & Batch persediaan.
 * Saldo diturunkan dari pergerakan stok POSTED; halaman ini mengelola master
 * lot (metadata + status), memantau kadaluarsa, dan menguji rencana FEFO.
 */
import { useCallback, useEffect, useState } from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { notify } from '@/lib/feedback';
import { apiGet } from '@/lib/api/client';
import {
  createInvLot, deleteInvLot, fefoPlan, listInvLots, updateInvLot,
  type FefoPlan, type InvLot,
} from '@/lib/api/inv-lots';

interface RefOpt { id: string; name: string }

const STATUS_BADGE: Record<string, 'default' | 'info' | 'danger' | 'success'> = {
  ACTIVE: 'success', QUARANTINE: 'info', EXPIRED: 'danger', BLOCKED: 'danger',
};
const selectCls = 'h-9 w-full rounded-md border bg-white px-2 text-sm';

export default function InvLotsPage() {
  const [rows, setRows] = useState<InvLot[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [expiring, setExpiring] = useState('');
  const [items, setItems] = useState<RefOpt[]>([]);
  const [warehouses, setWarehouses] = useState<RefOpt[]>([]);
  const [loading, setLoading] = useState(false);

  const [form, setForm] = useState({
    lotNumber: '', itemId: '', supplierLotNo: '', manufactureDate: '', expiryDate: '', status: 'ACTIVE', notes: '',
  });
  const [editingId, setEditingId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const [fefo, setFefo] = useState({ itemId: '', warehouseId: '', quantity: '1' });
  const [plan, setPlan] = useState<FefoPlan | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await listInvLots({
        page, limit: 20, search: search || undefined,
        status: status || undefined,
        expiringWithinDays: expiring ? Number(expiring) : undefined,
      });
      setRows(res.data ?? []);
      setTotal(res.meta?.total ?? (res.data ?? []).length);
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal memuat lot', 'danger');
    } finally { setLoading(false); }
  }, [page, search, status, expiring]);

  useEffect(() => { void load(); }, [load]);

  useEffect(() => {
    (async () => {
      try {
        const [it, wh] = await Promise.all([
          apiGet<{ data: RefOpt[] }>('/items?limit=200'),
          apiGet<{ data: RefOpt[] }>('/warehouses?limit=100'),
        ]);
        setItems(((it as { data: RefOpt[] }).data ?? []) as RefOpt[]);
        setWarehouses(((wh as { data: RefOpt[] }).data ?? []) as RefOpt[]);
      } catch { /* lookup opsional */ }
    })();
  }, []);

  const resetForm = () => {
    setForm({ lotNumber: '', itemId: '', supplierLotNo: '', manufactureDate: '', expiryDate: '', status: 'ACTIVE', notes: '' });
    setEditingId(null);
  };

  const submit = async () => {
    if (!form.lotNumber.trim()) { notify('Nomor lot wajib diisi', 'warn'); return; }
    setSaving(true);
    try {
      if (editingId) {
        await updateInvLot(editingId, {
          lotNumber: form.lotNumber.trim(),
          supplierLotNo: form.supplierLotNo || null,
          manufactureDate: form.manufactureDate || null,
          expiryDate: form.expiryDate || null,
          status: form.status as InvLot['status'],
          notes: form.notes || null,
        });
        notify('Lot diperbarui', 'success');
      } else {
        if (!form.itemId) { notify('Item wajib dipilih', 'warn'); setSaving(false); return; }
        await createInvLot({
          lotNumber: form.lotNumber.trim(), itemId: form.itemId,
          supplierLotNo: form.supplierLotNo || undefined,
          manufactureDate: form.manufactureDate || undefined,
          expiryDate: form.expiryDate || undefined,
          status: form.status as InvLot['status'],
          notes: form.notes || undefined,
        });
        notify('Lot dibuat', 'success');
      }
      resetForm();
      await load();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan lot', 'danger');
    } finally { setSaving(false); }
  };

  const startEdit = (lot: InvLot) => {
    setEditingId(lot.id);
    setForm({
      lotNumber: lot.lotNumber, itemId: lot.itemId,
      supplierLotNo: lot.supplierLotNo ?? '',
      manufactureDate: lot.manufactureDate ? lot.manufactureDate.slice(0, 10) : '',
      expiryDate: lot.expiryDate ? lot.expiryDate.slice(0, 10) : '',
      status: lot.status, notes: lot.notes ?? '',
    });
  };

  const remove = async (lot: InvLot) => {
    try {
      await deleteInvLot(lot.id);
      notify('Lot dihapus', 'success');
      await load();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menghapus lot', 'danger');
    }
  };

  const runFefo = async () => {
    if (!fefo.itemId || !fefo.warehouseId) { notify('Pilih item dan gudang untuk uji FEFO', 'warn'); return; }
    try {
      setPlan(await fefoPlan(fefo.itemId, fefo.warehouseId, fefo.quantity || '1'));
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menghitung FEFO', 'danger');
    }
  };

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-lg font-semibold">Lot & Batch</h1>
        <p className="text-sm text-muted-foreground">
          Saldo per lot dari pergerakan stok; pengambilan mengikuti FEFO (kadaluarsa terdekat dulu).
        </p>
      </div>
      <div className="grid gap-4 lg:grid-cols-3">
        <div className="space-y-4 lg:col-span-2">
          <div className="flex flex-wrap items-end gap-2">
            <div className="min-w-48 flex-1">
              <label className="mb-1 block text-xs text-muted-foreground">Cari nomor lot / item</label>
              <Input value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }} placeholder="mis. LOT-2026…" />
            </div>
            <div className="w-36">
              <label className="mb-1 block text-xs text-muted-foreground">Status</label>
              <select className={selectCls} value={status} onChange={(e) => { setStatus(e.target.value); setPage(1); }}>
                <option value="">Semua</option>
                <option value="ACTIVE">ACTIVE</option>
                <option value="QUARANTINE">QUARANTINE</option>
                <option value="EXPIRED">EXPIRED</option>
                <option value="BLOCKED">BLOCKED</option>
              </select>
            </div>
            <div className="w-40">
              <label className="mb-1 block text-xs text-muted-foreground">Segera kadaluarsa</label>
              <select className={selectCls} value={expiring} onChange={(e) => { setExpiring(e.target.value); setPage(1); }}>
                <option value="">—</option>
                <option value="30">≤ 30 hari</option>
                <option value="60">≤ 60 hari</option>
                <option value="90">≤ 90 hari</option>
              </select>
            </div>
            <Button variant="ghost" onClick={() => void load()}>Muat ulang</Button>
          </div>

          <div className="overflow-x-auto rounded-lg border">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b bg-muted/40 text-left">
                  <th className="px-3 py-2">Lot</th>
                  <th className="px-3 py-2">Item</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2 text-right">Saldo</th>
                  <th className="px-3 py-2">Kadaluarsa</th>
                  <th className="px-3 py-2">Aksi</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((lot) => (
                  <tr key={lot.id} className="border-b last:border-0">
                    <td className="px-3 py-2">
                      <span className="font-medium">{lot.lotNumber}</span>
                      <span className="block text-xs text-muted-foreground">ID {lot.id}{lot.supplierLotNo ? ` · supplier ${lot.supplierLotNo}` : ''}</span>
                    </td>
                    <td className="px-3 py-2">{lot.itemName ?? lot.itemId}</td>
                    <td className="px-3 py-2"><Badge variant={STATUS_BADGE[lot.status] ?? 'default'}>{lot.status}</Badge></td>
                    <td className="px-3 py-2 text-right font-medium">{lot.balance}</td>
                    <td className="px-3 py-2">
                      {lot.expiryDate ? lot.expiryDate.slice(0, 10) : '—'}
                      {lot.isExpired ? <Badge variant="danger">lewat</Badge>
                        : lot.daysToExpiry != null && lot.daysToExpiry <= 30
                          ? <Badge variant="info">{lot.daysToExpiry} hari lagi</Badge> : null}
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex gap-1">
                        <Button variant="ghost" onClick={() => startEdit(lot)}>Ubah</Button>
                        <Button variant="ghost" onClick={() => void remove(lot)}>Hapus</Button>
                      </div>
                    </td>
                  </tr>
                ))}
                {!rows.length && (
                  <tr><td colSpan={6} className="px-3 py-6 text-center text-muted-foreground">
                    {loading ? 'Memuat…' : 'Belum ada lot. Lot terbentuk otomatis saat GRN berisi nomor lot diposting, atau buat manual di formulir.'}
                  </td></tr>
                )}
              </tbody>
            </table>
          </div>
          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>Total {total} lot</span>
            <div className="flex gap-2">
              <Button variant="ghost" disabled={page <= 1} onClick={() => setPage(page - 1)}>Sebelumnya</Button>
              <Button variant="ghost" disabled={rows.length < 20} onClick={() => setPage(page + 1)}>Berikutnya</Button>
            </div>
          </div>
        </div>

        <div className="space-y-4">
          <div className="rounded-lg border p-4">
            <h3 className="mb-3 font-semibold">{editingId ? `Ubah Lot (ID ${editingId})` : 'Lot Baru (manual)'}</h3>
            <div className="space-y-2">
              <div>
                <label className="mb-1 block text-xs text-muted-foreground">Nomor lot *</label>
                <Input value={form.lotNumber} onChange={(e) => setForm({ ...form, lotNumber: e.target.value })} placeholder="mis. LOT-2026-001" />
              </div>
              <div>
                <label className="mb-1 block text-xs text-muted-foreground">Item *</label>
                <select className={selectCls} value={form.itemId} disabled={!!editingId}
                  onChange={(e) => setForm({ ...form, itemId: e.target.value })}>
                  <option value="">— pilih item —</option>
                  {items.map((i) => <option key={i.id} value={i.id}>{i.name}</option>)}
                </select>
              </div>
              <div>
                <label className="mb-1 block text-xs text-muted-foreground">Lot supplier</label>
                <Input value={form.supplierLotNo} onChange={(e) => setForm({ ...form, supplierLotNo: e.target.value })} />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="mb-1 block text-xs text-muted-foreground">Tgl produksi</label>
                  <Input type="date" value={form.manufactureDate} onChange={(e) => setForm({ ...form, manufactureDate: e.target.value })} />
                </div>
                <div>
                  <label className="mb-1 block text-xs text-muted-foreground">Kadaluarsa</label>
                  <Input type="date" value={form.expiryDate} onChange={(e) => setForm({ ...form, expiryDate: e.target.value })} />
                </div>
              </div>
              <div>
                <label className="mb-1 block text-xs text-muted-foreground">Status</label>
                <select className={selectCls} value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })}>
                  <option value="ACTIVE">ACTIVE</option>
                  <option value="QUARANTINE">QUARANTINE</option>
                  <option value="EXPIRED">EXPIRED</option>
                  <option value="BLOCKED">BLOCKED</option>
                </select>
              </div>
              <div>
                <label className="mb-1 block text-xs text-muted-foreground">Catatan</label>
                <Input value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
              </div>
              <div className="flex gap-2 pt-1">
                <Button variant="primary" disabled={saving} onClick={() => void submit()}>{saving ? 'Menyimpan…' : 'Simpan'}</Button>
                {editingId && <Button variant="ghost" onClick={resetForm}>Batal ubah</Button>}
              </div>
            </div>
          </div>

          <div className="rounded-lg border p-4">
            <h3 className="mb-1 font-semibold">Uji FEFO</h3>
            <p className="mb-3 text-xs text-muted-foreground">Simulasi urutan pengambilan: kadaluarsa terdekat dulu, lot tanpa tanggal terakhir.</p>
            <div className="space-y-2">
              <select className={selectCls} value={fefo.itemId} onChange={(e) => setFefo({ ...fefo, itemId: e.target.value })}>
                <option value="">— pilih item —</option>
                {items.map((i) => <option key={i.id} value={i.id}>{i.name}</option>)}
              </select>
              <select className={selectCls} value={fefo.warehouseId} onChange={(e) => setFefo({ ...fefo, warehouseId: e.target.value })}>
                <option value="">— pilih gudang —</option>
                {warehouses.map((w) => <option key={w.id} value={w.id}>{w.name}</option>)}
              </select>
              <div className="flex gap-2">
                <Input value={fefo.quantity} onChange={(e) => setFefo({ ...fefo, quantity: e.target.value })} placeholder="Qty" />
                <Button variant="primary" onClick={() => void runFefo()}>Hitung</Button>
              </div>
              {plan && (
                <div className="rounded-md bg-muted/40 p-3 text-sm">
                  {plan.allocations.length === 0 && <p>Tidak ada lot dengan saldo di gudang ini.</p>}
                  {plan.allocations.map((a) => (
                    <p key={a.lotId} className="flex justify-between">
                      <span>{a.lotNumber} <span className="text-muted-foreground">(exp {a.expiryDate ? a.expiryDate.slice(0, 10) : '—'})</span></span>
                      <span className="font-medium">{a.quantity}</span>
                    </p>
                  ))}
                  {Number(plan.shortfall) > 0 && (
                    <p className="mt-1 text-amber-700">Kekurangan {plan.shortfall} akan diambil tanpa lot.</p>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

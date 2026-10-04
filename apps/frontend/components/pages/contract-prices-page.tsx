'use client';

/**
 * Fase 3 W7 — Harga Kontrak & Paket.
 * Harga kontrak berlapis per sekolah: harga tetap per item, atau diskon per
 * kategori, atau diskon seluruh sekolah (tanpa level yayasan). Paket/bundle
 * mendefinisikan isi item paket yang dijual lewat portal.
 */
import { useCallback, useEffect, useState } from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { notify } from '@/lib/feedback';
import { formatRupiah } from '@/lib/format';
import { apiGet } from '@/lib/api/client';
import {
  createContractPrice, deleteBundle, deleteContractPrice, listBundles, listContractPrices,
  updateContractPrice, upsertBundle,
  type ContractPrice, type ItemBundle,
} from '@/lib/api/contracts';

interface RefOpt { id: string; name: string }
const selectCls = 'h-9 w-full rounded-md border bg-white px-2 text-sm';

export default function ContractPricesPage() {
  const [schools, setSchools] = useState<RefOpt[]>([]);
  const [items, setItems] = useState<RefOpt[]>([]);
  const [categories, setCategories] = useState<RefOpt[]>([]);

  const [prices, setPrices] = useState<ContractPrice[]>([]);
  const [filterSchool, setFilterSchool] = useState('');
  const [form, setForm] = useState({
    partnerId: '', scope: 'ITEM', itemId: '', categoryId: '',
    price: '', discountPercent: '', validFrom: '', validTo: '', notes: '',
  });
  const [savingPrice, setSavingPrice] = useState(false);

  const [bundles, setBundles] = useState<ItemBundle[]>([]);
  const [bundleItemId, setBundleItemId] = useState('');
  const [bundleName, setBundleName] = useState('');
  const [bundleLines, setBundleLines] = useState<{ componentItemId: string; quantity: string }[]>([
    { componentItemId: '', quantity: '1' },
  ]);
  const [savingBundle, setSavingBundle] = useState(false);

  const loadPrices = useCallback(async () => {
    try {
      const res = await listContractPrices({ limit: 100, partnerId: filterSchool || undefined });
      setPrices(res.data ?? []);
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal memuat harga kontrak', 'danger');
    }
  }, [filterSchool]);

  const loadBundles = useCallback(async () => {
    try {
      const res = await listBundles();
      setBundles(res.data ?? []);
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal memuat paket', 'danger');
    }
  }, []);

  useEffect(() => { void loadPrices(); }, [loadPrices]);
  useEffect(() => { void loadBundles(); }, [loadBundles]);

  useEffect(() => {
    (async () => {
      try {
        const [sc, it, ct] = await Promise.all([
          apiGet<{ data: { id?: string; partnerId?: string; name: string }[] }>('/schools?limit=200'),
          apiGet<{ data: RefOpt[] }>('/items?limit=200'),
          apiGet<{ data: RefOpt[] }>('/item-categories?limit=100'),
        ]);
        setSchools((((sc as any).data ?? []) as any[]).map((s) => ({ id: String(s.partnerId ?? s.id), name: s.name })));
        setItems(((it as any).data ?? []) as RefOpt[]);
        setCategories(((ct as any).data ?? []) as RefOpt[]);
      } catch { /* lookup opsional */ }
    })();
  }, []);

  const submitPrice = async () => {
    if (!form.partnerId) { notify('Pilih sekolah dulu', 'warn'); return; }
    if (form.scope === 'ITEM' && !form.itemId) { notify('Pilih item untuk cakupan ITEM', 'warn'); return; }
    if (form.scope === 'ITEM' && !form.price) { notify('Harga tetap wajib diisi', 'warn'); return; }
    if (form.scope !== 'ITEM' && !form.discountPercent) { notify('Diskon persen wajib diisi', 'warn'); return; }
    if (form.scope === 'KATEGORI' && !form.categoryId) { notify('Pilih kategori', 'warn'); return; }
    setSavingPrice(true);
    try {
      await createContractPrice({
        partnerId: form.partnerId,
        itemId: form.scope === 'ITEM' ? form.itemId : undefined,
        categoryId: form.scope === 'KATEGORI' ? form.categoryId : undefined,
        price: form.scope === 'ITEM' ? form.price : undefined,
        discountPercent: form.scope !== 'ITEM' ? form.discountPercent : undefined,
        validFrom: form.validFrom || undefined,
        validTo: form.validTo || undefined,
        notes: form.notes || undefined,
      });
      notify('Baris harga kontrak ditambahkan', 'success');
      setForm({ ...form, itemId: '', categoryId: '', price: '', discountPercent: '', notes: '' });
      await loadPrices();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan harga kontrak', 'danger');
    } finally { setSavingPrice(false); }
  };

  const togglePrice = async (row: ContractPrice) => {
    try {
      await updateContractPrice(row.id, { isActive: !row.isActive });
      await loadPrices();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal mengubah status', 'danger');
    }
  };

  const removePrice = async (row: ContractPrice) => {
    try {
      await deleteContractPrice(row.id);
      notify('Baris harga kontrak dihapus', 'success');
      await loadPrices();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menghapus', 'danger');
    }
  };

  const submitBundle = async () => {
    if (!bundleItemId) { notify('Pilih item paket dulu', 'warn'); return; }
    const lines = bundleLines.filter((l) => l.componentItemId);
    if (!lines.length) { notify('Paket butuh minimal satu komponen', 'warn'); return; }
    setSavingBundle(true);
    try {
      await upsertBundle({
        itemId: bundleItemId,
        name: bundleName || undefined,
        lines: lines.map((l) => ({ componentItemId: l.componentItemId, quantity: l.quantity || '1' })),
      });
      notify('Paket disimpan', 'success');
      setBundleItemId('');
      setBundleName('');
      setBundleLines([{ componentItemId: '', quantity: '1' }]);
      await loadBundles();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan paket', 'danger');
    } finally { setSavingBundle(false); }
  };

  const editBundle = (b: ItemBundle) => {
    setBundleItemId(b.itemId);
    setBundleName(b.name ?? '');
    setBundleLines(b.lines.map((l) => ({ componentItemId: l.componentItemId, quantity: l.quantity })));
  };

  const removeBundle = async (b: ItemBundle) => {
    try {
      await deleteBundle(b.id);
      notify('Paket dihapus', 'success');
      await loadBundles();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal menghapus paket', 'danger');
    }
  };

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-lg font-semibold">Harga Kontrak & Paket</h1>
        <p className="text-sm text-muted-foreground">
          Harga portal per sekolah diselesaikan berlapis: harga item kontrak → diskon kategori → diskon sekolah → harga standar.
        </p>
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        <div className="space-y-3 lg:col-span-2">
          <div className="flex items-end gap-2">
            <div className="min-w-56 flex-1">
              <label className="mb-1 block text-xs text-muted-foreground">Filter sekolah</label>
              <select className={selectCls} value={filterSchool} onChange={(e) => setFilterSchool(e.target.value)}>
                <option value="">Semua sekolah</option>
                {schools.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>
            </div>
            <Button variant="ghost" onClick={() => void loadPrices()}>Muat ulang</Button>
          </div>
          <div className="overflow-x-auto rounded-lg border">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b bg-muted/40 text-left">
                  <th className="px-3 py-2">Sekolah</th>
                  <th className="px-3 py-2">Cakupan</th>
                  <th className="px-3 py-2">Harga / Diskon</th>
                  <th className="px-3 py-2">Berlaku</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2">Aksi</th>
                </tr>
              </thead>
              <tbody>
                {prices.map((r) => (
                  <tr key={r.id} className="border-b last:border-0">
                    <td className="px-3 py-2 font-medium">{r.partnerName ?? r.partnerId}</td>
                    <td className="px-3 py-2">
                      <Badge variant="info">{r.scope}</Badge>
                      <span className="block text-xs text-muted-foreground">
                        {r.scope === 'ITEM' ? r.itemName : r.scope === 'KATEGORI' ? r.categoryName : 'Seluruh sekolah'}
                      </span>
                    </td>
                    <td className="px-3 py-2">
                      {r.price != null ? formatRupiah(Number(r.price)) : `Diskon ${r.discountPercent}%`}
                    </td>
                    <td className="px-3 py-2 text-xs">
                      {r.validFrom ?? '…'} s/d {r.validTo ?? '…'}
                    </td>
                    <td className="px-3 py-2">
                      <Badge variant={r.isActive ? 'success' : 'default'}>{r.isActive ? 'Aktif' : 'Nonaktif'}</Badge>
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex gap-1">
                        <Button variant="ghost" onClick={() => void togglePrice(r)}>{r.isActive ? 'Nonaktifkan' : 'Aktifkan'}</Button>
                        <Button variant="ghost" onClick={() => void removePrice(r)}>Hapus</Button>
                      </div>
                    </td>
                  </tr>
                ))}
                {!prices.length && (
                  <tr><td colSpan={6} className="px-3 py-6 text-center text-muted-foreground">
                    Belum ada harga kontrak — semua sekolah memakai harga standar.
                  </td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        <div className="rounded-lg border p-4">
          <h3 className="mb-3 font-semibold">Baris Kontrak Baru</h3>
          <div className="space-y-2">
            <div>
              <label className="mb-1 block text-xs text-muted-foreground">Sekolah *</label>
              <select className={selectCls} value={form.partnerId} onChange={(e) => setForm({ ...form, partnerId: e.target.value })}>
                <option value="">— pilih sekolah —</option>
                {schools.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>
            </div>
            <div>
              <label className="mb-1 block text-xs text-muted-foreground">Cakupan</label>
              <select className={selectCls} value={form.scope} onChange={(e) => setForm({ ...form, scope: e.target.value })}>
                <option value="ITEM">Item (harga tetap)</option>
                <option value="KATEGORI">Kategori (diskon)</option>
                <option value="SEKOLAH">Seluruh sekolah (diskon)</option>
              </select>
            </div>
            {form.scope === 'ITEM' && (
              <>
                <div>
                  <label className="mb-1 block text-xs text-muted-foreground">Item *</label>
                  <select className={selectCls} value={form.itemId} onChange={(e) => setForm({ ...form, itemId: e.target.value })}>
                    <option value="">— pilih item —</option>
                    {items.map((i) => <option key={i.id} value={i.id}>{i.name}</option>)}
                  </select>
                </div>
                <div>
                  <label className="mb-1 block text-xs text-muted-foreground">Harga kontrak (Rp) *</label>
                  <Input value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} placeholder="mis. 45000" />
                </div>
              </>
            )}
            {form.scope === 'KATEGORI' && (
              <div>
                <label className="mb-1 block text-xs text-muted-foreground">Kategori *</label>
                <select className={selectCls} value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })}>
                  <option value="">— pilih kategori —</option>
                  {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
            )}
            {form.scope !== 'ITEM' && (
              <div>
                <label className="mb-1 block text-xs text-muted-foreground">Diskon (%) *</label>
                <Input value={form.discountPercent} onChange={(e) => setForm({ ...form, discountPercent: e.target.value })} placeholder="mis. 10" />
              </div>
            )}
            <div className="grid grid-cols-2 gap-2">
              <div>
                <label className="mb-1 block text-xs text-muted-foreground">Berlaku dari</label>
                <Input type="date" value={form.validFrom} onChange={(e) => setForm({ ...form, validFrom: e.target.value })} />
              </div>
              <div>
                <label className="mb-1 block text-xs text-muted-foreground">Sampai</label>
                <Input type="date" value={form.validTo} onChange={(e) => setForm({ ...form, validTo: e.target.value })} />
              </div>
            </div>
            <div>
              <label className="mb-1 block text-xs text-muted-foreground">Catatan</label>
              <Input value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
            </div>
            <Button variant="primary" disabled={savingPrice} onClick={() => void submitPrice()}>
              {savingPrice ? 'Menyimpan…' : 'Tambah baris kontrak'}
            </Button>
          </div>
        </div>
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        <div className="space-y-3 lg:col-span-2">
          <h3 className="font-semibold">Paket / Bundle Terdaftar</h3>
          {bundles.map((b) => (
            <div key={b.id} className="rounded-lg border p-3">
              <div className="flex items-center justify-between">
                <div>
                  <span className="font-medium">{b.name ?? b.itemName}</span>
                  <span className="block text-xs text-muted-foreground">{b.itemName} · {b.lines.length} komponen</span>
                </div>
                <div className="flex gap-1">
                  <Button variant="ghost" onClick={() => editBundle(b)}>Ubah</Button>
                  <Button variant="ghost" onClick={() => void removeBundle(b)}>Hapus</Button>
                </div>
              </div>
              <ul className="mt-2 list-inside list-disc text-sm text-muted-foreground">
                {b.lines.map((l) => (
                  <li key={l.id ?? l.componentItemId}>{l.componentName ?? l.componentItemId} × {l.quantity}</li>
                ))}
              </ul>
            </div>
          ))}
          {!bundles.length && <p className="text-sm text-muted-foreground">Belum ada paket terdaftar.</p>}
        </div>

        <div className="rounded-lg border p-4">
          <h3 className="mb-3 font-semibold">Definisikan Paket</h3>
          <div className="space-y-2">
            <div>
              <label className="mb-1 block text-xs text-muted-foreground">Item paket *</label>
              <select className={selectCls} value={bundleItemId} onChange={(e) => setBundleItemId(e.target.value)}>
                <option value="">— pilih item paket —</option>
                {items.map((i) => <option key={i.id} value={i.id}>{i.name}</option>)}
              </select>
            </div>
            <div>
              <label className="mb-1 block text-xs text-muted-foreground">Nama tampilan paket</label>
              <Input value={bundleName} onChange={(e) => setBundleName(e.target.value)} placeholder="mis. Paket Kelas 1 Lengkap" />
            </div>
            <label className="block text-xs text-muted-foreground">Komponen</label>
            {bundleLines.map((l, idx) => (
              <div key={idx} className="flex gap-2">
                <select
                  className={selectCls}
                  value={l.componentItemId}
                  onChange={(e) => setBundleLines(bundleLines.map((x, i) => (i === idx ? { ...x, componentItemId: e.target.value } : x)))}
                >
                  <option value="">— komponen —</option>
                  {items.map((i) => <option key={i.id} value={i.id}>{i.name}</option>)}
                </select>
                <Input
                  className="w-20"
                  value={l.quantity}
                  onChange={(e) => setBundleLines(bundleLines.map((x, i) => (i === idx ? { ...x, quantity: e.target.value } : x)))}
                  placeholder="Qty"
                />
                <Button variant="ghost" onClick={() => setBundleLines(bundleLines.filter((_, i) => i !== idx))}>×</Button>
              </div>
            ))}
            <Button variant="ghost" onClick={() => setBundleLines([...bundleLines, { componentItemId: '', quantity: '1' }])}>
              + Komponen
            </Button>
            <div>
              <Button variant="primary" disabled={savingBundle} onClick={() => void submitBundle()}>
                {savingBundle ? 'Menyimpan…' : 'Simpan paket'}
              </Button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

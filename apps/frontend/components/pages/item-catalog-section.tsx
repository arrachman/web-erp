'use client';

/**
 * D1 — "Katalog Sekolah" section of the item form: publisher, jenjang /
 * grade / curriculum / subject targeting, HET, custom-print flag, stock
 * class (D3) and per-channel display status. Self-contained: loads and
 * saves its own profile via /item-catalog/:itemId, independent of the
 * item form payload. Only meaningful for a saved item.
 */

import * as React from 'react';
import { toast } from 'sonner';
import { Section } from './items-form-parts';
import {
  getItemCatalog,
  upsertItemCatalog,
  type ItemCatalogProfile,
} from '@/lib/api/item-catalog';

const inputCls =
  'w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-2 focus:ring-ring';
const labelCls = 'mb-1 block text-xs font-medium text-muted-foreground';

const JENJANG_OPTIONS = [
  { value: '', label: '— Semua / tidak spesifik —' },
  { value: 'PAUD', label: 'PAUD' },
  { value: 'TK', label: 'TK / RA' },
  { value: 'SD', label: 'SD / MI' },
  { value: 'SMP', label: 'SMP / MTs' },
  { value: 'SMA', label: 'SMA / MA' },
  { value: 'SMK', label: 'SMK' },
  { value: 'SLB', label: 'SLB' },
  { value: 'OTHER', label: 'Lainnya' },
];
const CURRICULUM_OPTIONS = [
  { value: '', label: '— Tidak spesifik —' },
  { value: 'MERDEKA', label: 'Kurikulum Merdeka' },
  { value: 'KURIKULUM_2013', label: 'Kurikulum 2013' },
  { value: 'KTSP_2006', label: 'KTSP 2006' },
  { value: 'LAINNYA', label: 'Lainnya' },
];
const STOCK_CLASS_OPTIONS = [
  { value: 'DAGANGAN', label: 'Barang Dagangan' },
  { value: 'BAHAN_BAKU', label: 'Bahan Baku' },
  { value: 'BARANG_JADI', label: 'Barang Jadi' },
  { value: 'KONSINYASI', label: 'Konsinyasi (titipan supplier)' },
];

export function ItemCatalogSection({ itemId }: { itemId?: string }) {
  const [profile, setProfile] = React.useState<ItemCatalogProfile | null>(null);
  const [loading, setLoading] = React.useState(false);
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    if (!itemId) return;
    let alive = true;
    setLoading(true);
    getItemCatalog(itemId)
      .then((row) => { if (alive) setProfile(row.profile); })
      .catch(() => { if (alive) toast.error('Gagal memuat atribut katalog'); })
      .finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; };
  }, [itemId]);

  if (!itemId) {
    return (
      <Section title="Katalog Sekolah" icon="tag">
        <p className="text-sm text-muted-foreground">
          Simpan item terlebih dahulu — atribut katalog (penerbit, jenjang, HET,
          kelas stok) diisi setelah item tersimpan.
        </p>
      </Section>
    );
  }
  if (loading || !profile) {
    return (
      <Section title="Katalog Sekolah" icon="tag">
        <p className="text-sm text-muted-foreground">Memuat atribut katalog…</p>
      </Section>
    );
  }

  const set = <K extends keyof ItemCatalogProfile>(k: K, v: ItemCatalogProfile[K]) =>
    setProfile((p) => (p ? { ...p, [k]: v } : p));
  const channelOn = (key: string) => profile.channels?.[key] !== false;
  const setChannel = (key: string, on: boolean) =>
    set('channels', { ...(profile.channels ?? {}), [key]: on });

  const save = async () => {
    setSaving(true);
    try {
      const row = await upsertItemCatalog(itemId, {
        publisherName: profile.publisherName || undefined,
        jenjang: (profile.jenjang || undefined) as ItemCatalogProfile['jenjang'],
        gradeLevel: profile.gradeLevel || undefined,
        curriculum: (profile.curriculum || undefined) as ItemCatalogProfile['curriculum'],
        subject: profile.subject || undefined,
        hetPrice: profile.hetPrice ?? undefined,
        isCustomPrint: profile.isCustomPrint,
        stockClass: profile.stockClass,
        channels: profile.channels ?? undefined,
        isActive: profile.isActive,
      });
      setProfile(row.profile);
      toast.success('Atribut katalog tersimpan');
    } catch {
      toast.error('Gagal menyimpan atribut katalog');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Section title="Katalog Sekolah" icon="tag" hint="Atribut D1: penargetan sekolah, HET, dan klasifikasi stok (D3). Tersimpan terpisah dari form item.">
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div>
          <label className={labelCls} htmlFor="cat-publisher">Penerbit</label>
          <input id="cat-publisher" className={inputCls} value={profile.publisherName ?? ''}
            onChange={(e) => set('publisherName', e.target.value)} placeholder="cth: Erlangga, Tiga Serangkai" />
        </div>
        <div>
          <label className={labelCls} htmlFor="cat-subject">Mata Pelajaran</label>
          <input id="cat-subject" className={inputCls} value={profile.subject ?? ''}
            onChange={(e) => set('subject', e.target.value)} placeholder="cth: Matematika, Bahasa Indonesia" />
        </div>
        <div>
          <label className={labelCls} htmlFor="cat-jenjang">Jenjang</label>
          <select id="cat-jenjang" className={inputCls} value={profile.jenjang ?? ''}
            onChange={(e) => set('jenjang', (e.target.value || null) as ItemCatalogProfile['jenjang'])}>
            {JENJANG_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </div>
        <div>
          <label className={labelCls} htmlFor="cat-grade">Kelas</label>
          <input id="cat-grade" className={inputCls} value={profile.gradeLevel ?? ''}
            onChange={(e) => set('gradeLevel', e.target.value)} placeholder="cth: 1–6, Kelas 4, Semua kelas" />
        </div>
        <div>
          <label className={labelCls} htmlFor="cat-curriculum">Kurikulum</label>
          <select id="cat-curriculum" className={inputCls} value={profile.curriculum ?? ''}
            onChange={(e) => set('curriculum', (e.target.value || null) as ItemCatalogProfile['curriculum'])}>
            {CURRICULUM_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </div>
        <div>
          <label className={labelCls} htmlFor="cat-het">HET (Rp)</label>
          <input id="cat-het" type="number" min={0} className={inputCls}
            value={profile.hetPrice ?? ''}
            onChange={(e) => set('hetPrice', e.target.value === '' ? null : Number(e.target.value))}
            placeholder="Harga eceran tertinggi" />
        </div>
        <div>
          <label className={labelCls} htmlFor="cat-stockclass">Kelas Stok</label>
          <select id="cat-stockclass" className={inputCls} value={profile.stockClass}
            onChange={(e) => set('stockClass', e.target.value as ItemCatalogProfile['stockClass'])}>
            {STOCK_CLASS_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </div>
        <div className="flex flex-col gap-2 pt-5">
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={profile.isCustomPrint}
              onChange={(e) => set('isCustomPrint', e.target.checked)} />
            Item cetak custom (bukan dagangan standar)
          </label>
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={channelOn('LANGSUNG')}
              onChange={(e) => setChannel('LANGSUNG', e.target.checked)} />
            Tayang di kanal Langsung
          </label>
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={channelOn('SIPLAH')}
              onChange={(e) => setChannel('SIPLAH', e.target.checked)} />
            Tayang di kanal SIPLah
          </label>
        </div>
      </div>
      <div className="mt-4">
        <button type="button" onClick={save} disabled={saving}
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50">
          {saving ? 'Menyimpan…' : 'Simpan Atribut Katalog'}
        </button>
      </div>
    </Section>
  );
}

'use client';

/**
 * Fase 2 P1 — Form Estimasi Cetak: spesifikasi pekerjaan + baris komponen
 * biaya. Total dihitung server; pratinjau di sini hanya cerminan lokal.
 */

import * as React from 'react';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { apiGet } from '@/lib/api/client';
import { notify } from '@/lib/feedback';
import { formatRupiah } from '@/lib/format';
import {
  PRINT_COMPONENTS,
  PRINT_COMPONENT_LABELS,
  createPrintEstimate,
  getPrintEstimate,
  updatePrintEstimate,
  type PrintComponent,
  type PrintEstimateLine,
} from '@/lib/api/mfg-print-estimates';

const rp = (v: number) => formatRupiah(v || 0);
const selectCls =
  'h-9 w-full rounded-md border border-input bg-background px-2 text-sm';
const labelCls = 'mb-1 block text-xs font-medium text-muted-foreground';

interface Option {
  id: string;
  label: string;
}

interface LineState {
  component: PrintComponent;
  description: string;
  quantity: string;
  unitCost: string;
}

export function PrintEstimateForm({
  estimateId,
  onDone,
}: {
  estimateId?: string;
  onDone: () => void;
}) {
  const [branches, setBranches] = React.useState<Option[]>([
    { id: '1033', label: 'Bahtera Madani' },
  ]);
  const [customers, setCustomers] = React.useState<Option[]>([]);
  const [items, setItems] = React.useState<Option[]>([]);
  const [branchId, setBranchId] = React.useState('1033');
  const [docDate, setDocDate] = React.useState(
    () => new Date().toISOString().slice(0, 10),
  );
  const [customerId, setCustomerId] = React.useState('');
  const [itemId, setItemId] = React.useState('');
  const [title, setTitle] = React.useState('');
  const [paperSize, setPaperSize] = React.useState('');
  const [pageCount, setPageCount] = React.useState('');
  const [printQuantity, setPrintQuantity] = React.useState('1000');
  const [colorSpec, setColorSpec] = React.useState('');
  const [finishing, setFinishing] = React.useState('');
  const [marginPercent, setMarginPercent] = React.useState('20');
  const [notes, setNotes] = React.useState('');
  const [lines, setLines] = React.useState<LineState[]>([
    { component: 'KERTAS', description: '', quantity: '1', unitCost: '0' },
    { component: 'CETAK', description: '', quantity: '1', unitCost: '0' },
  ]);
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    (async () => {
      try {
        const b: any = await apiGet('/branches?limit=100');
        const rows = b?.data ?? b;
        if (Array.isArray(rows) && rows.length)
          setBranches(rows.map((r: any) => ({ id: String(r.id), label: r.name ?? r.code ?? String(r.id) })));
      } catch { /* fallback tetap tersedia */ }
      try {
        const p: any = await apiGet('/partners?limit=200');
        const rows = p?.data ?? p;
        if (Array.isArray(rows))
          setCustomers(rows.map((r: any) => ({ id: String(r.id), label: r.name ?? String(r.id) })));
      } catch { /* abaikan */ }
      try {
        const it: any = await apiGet('/items?limit=200');
        const rows = it?.data ?? it;
        if (Array.isArray(rows))
          setItems(rows.map((r: any) => ({ id: String(r.id), label: r.name ?? String(r.id) })));
      } catch { /* abaikan */ }
    })();
  }, []);

  React.useEffect(() => {
    if (!estimateId) return;
    (async () => {
      try {
        const est = await getPrintEstimate(estimateId);
        setBranchId(est.branchId);
        setDocDate(est.docDate);
        setCustomerId(est.customerId ?? '');
        setItemId(est.itemId ?? '');
        setTitle(est.title);
        setPaperSize(est.paperSize ?? '');
        setPageCount(est.pageCount != null ? String(est.pageCount) : '');
        setPrintQuantity(est.printQuantity);
        setColorSpec(est.colorSpec ?? '');
        setFinishing(est.finishing ?? '');
        setMarginPercent(est.marginPercent);
        setNotes(est.notes ?? '');
        if (est.lines?.length)
          setLines(
            est.lines.map((l: PrintEstimateLine) => ({
              component: l.component,
              description: l.description ?? '',
              quantity: l.quantity,
              unitCost: l.unitCost,
            })),
          );
      } catch (e: any) {
        notify(e?.message ?? 'Gagal memuat estimasi.', 'danger');
      }
    })();
  }, [estimateId]);

  const totalCost = lines.reduce(
    (s, l) => s + (Number(l.quantity) || 0) * (Number(l.unitCost) || 0),
    0,
  );
  const totalPrice = totalCost * (1 + (Number(marginPercent) || 0) / 100);
  const unitPrice =
    (Number(printQuantity) || 0) > 0 ? totalPrice / Number(printQuantity) : 0;

  const setLine = (i: number, patch: Partial<LineState>) =>
    setLines((prev) => prev.map((l, idx) => (idx === i ? { ...l, ...patch } : l)));

  const save = async () => {
    if (!title.trim()) return notify('Judul pekerjaan wajib diisi.', 'danger');
    if (!lines.length) return notify('Minimal 1 baris komponen biaya.', 'danger');
    setSaving(true);
    try {
      const payload = {
        docDate,
        customerId: customerId || undefined,
        itemId: itemId || undefined,
        title: title.trim(),
        paperSize: paperSize || undefined,
        pageCount: pageCount ? Number(pageCount) : undefined,
        printQuantity,
        colorSpec: colorSpec || undefined,
        finishing: finishing || undefined,
        marginPercent: marginPercent || '0',
        notes: notes || undefined,
        lines: lines.map((l, i) => ({
          lineNo: i + 1,
          component: l.component,
          description: l.description || undefined,
          quantity: l.quantity || '0',
          unitCost: l.unitCost || '0',
        })),
      };
      if (estimateId) {
        await updatePrintEstimate(estimateId, payload);
        notify('Estimasi diperbarui.', 'success');
      } else {
        await createPrintEstimate({ branchId, ...payload });
        notify('Estimasi dibuat.', 'success');
      }
      onDone();
    } catch (e: any) {
      notify(e?.message ?? 'Gagal menyimpan estimasi.', 'danger');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Card className="p-4">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-base font-semibold">
          {estimateId ? 'Ubah Estimasi Cetak' : 'Estimasi Cetak Baru'}
        </h2>
        <div className="flex gap-2">
          <Button variant="ghost" onClick={onDone}>
            Kembali
          </Button>
          <Button onClick={save} disabled={saving}>
            {saving ? 'Menyimpan…' : 'Simpan'}
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
        <div>
          <label className={labelCls}>Cabang</label>
          <select className={selectCls} value={branchId} onChange={(e) => setBranchId(e.target.value)} disabled={!!estimateId}>
            {branches.map((o) => (
              <option key={o.id} value={o.id}>{o.label}</option>
            ))}
          </select>
        </div>
        <div>
          <label className={labelCls}>Tanggal</label>
          <Input type="date" value={docDate} onChange={(e) => setDocDate(e.target.value)} />
        </div>
        <div>
          <label className={labelCls}>Pelanggan</label>
          <select className={selectCls} value={customerId} onChange={(e) => setCustomerId(e.target.value)}>
            <option value="">— belum dipilih —</option>
            {customers.map((o) => (
              <option key={o.id} value={o.id}>{o.label}</option>
            ))}
          </select>
        </div>
        <div>
          <label className={labelCls}>Item hasil cetak</label>
          <select className={selectCls} value={itemId} onChange={(e) => setItemId(e.target.value)}>
            <option value="">— belum dipilih —</option>
            {items.map((o) => (
              <option key={o.id} value={o.id}>{o.label}</option>
            ))}
          </select>
        </div>
        <div className="col-span-2">
          <label className={labelCls}>Judul pekerjaan *</label>
          <Input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="cth. Cetak LKS Matematika Kelas 4" />
        </div>
        <div>
          <label className={labelCls}>Ukuran kertas</label>
          <Input value={paperSize} onChange={(e) => setPaperSize(e.target.value)} placeholder="A4 / F4 / B5" />
        </div>
        <div>
          <label className={labelCls}>Jumlah halaman</label>
          <Input type="number" value={pageCount} onChange={(e) => setPageCount(e.target.value)} />
        </div>
        <div>
          <label className={labelCls}>Oplah (qty cetak)</label>
          <Input type="number" value={printQuantity} onChange={(e) => setPrintQuantity(e.target.value)} />
        </div>
        <div>
          <label className={labelCls}>Warna</label>
          <Input value={colorSpec} onChange={(e) => setColorSpec(e.target.value)} placeholder="4/0, 4/4, 1/1" />
        </div>
        <div>
          <label className={labelCls}>Finishing</label>
          <Input value={finishing} onChange={(e) => setFinishing(e.target.value)} placeholder="jilid lem, staples…" />
        </div>
        <div>
          <label className={labelCls}>Margin %</label>
          <Input type="number" value={marginPercent} onChange={(e) => setMarginPercent(e.target.value)} />
        </div>
        <div className="col-span-2 md:col-span-4">
          <label className={labelCls}>Catatan</label>
          <Input value={notes} onChange={(e) => setNotes(e.target.value)} />
        </div>
      </div>

      <h3 className="mb-2 mt-6 text-sm font-semibold">Komponen biaya</h3>
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b text-left text-xs text-muted-foreground">
            <th className="py-1 pr-2">Komponen</th>
            <th className="py-1 pr-2">Keterangan</th>
            <th className="py-1 pr-2 w-28">Qty</th>
            <th className="py-1 pr-2 w-36">Biaya satuan</th>
            <th className="py-1 pr-2 w-36 text-right">Jumlah</th>
            <th className="w-10" />
          </tr>
        </thead>
        <tbody>
          {lines.map((l, i) => (
            <tr key={i} className="border-b">
              <td className="py-1 pr-2">
                <select className={selectCls} value={l.component} onChange={(e) => setLine(i, { component: e.target.value as PrintComponent })}>
                  {PRINT_COMPONENTS.map((c) => (
                    <option key={c} value={c}>{PRINT_COMPONENT_LABELS[c]}</option>
                  ))}
                </select>
              </td>
              <td className="py-1 pr-2">
                <Input value={l.description} onChange={(e) => setLine(i, { description: e.target.value })} />
              </td>
              <td className="py-1 pr-2">
                <Input type="number" value={l.quantity} onChange={(e) => setLine(i, { quantity: e.target.value })} />
              </td>
              <td className="py-1 pr-2">
                <Input type="number" value={l.unitCost} onChange={(e) => setLine(i, { unitCost: e.target.value })} />
              </td>
              <td className="py-1 pr-2 text-right">{rp((Number(l.quantity) || 0) * (Number(l.unitCost) || 0))}</td>
              <td>
                <Button variant="ghost" onClick={() => setLines((prev) => prev.filter((_, idx) => idx !== i))}>
                  ✕
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <Button
        variant="ghost"
        className="mt-2"
        onClick={() => setLines((prev) => [...prev, { component: 'LAIN', description: '', quantity: '1', unitCost: '0' }])}
      >
        + Tambah baris
      </Button>

      <div className="mt-4 flex flex-wrap gap-6 border-t pt-3 text-sm">
        <span>Total biaya: <b>{rp(totalCost)}</b></span>
        <span>Harga jual: <b>{rp(totalPrice)}</b></span>
        <span>Harga satuan: <b>{rp(unitPrice)}</b></span>
      </div>
    </Card>
  );
}

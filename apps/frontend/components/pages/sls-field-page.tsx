'use client';

/**
 * Fase 2 P8 — Sales Lapangan (PWA): layar ringkas mobile-first di frontend
 * yang sama, login & role yang sama. Sales bisa mencatat kunjungan sekolah
 * (A1 — menggerakkan lastVisitAt), membuat Sales Order cepat (channel
 * SALES → tampil di Order Hub sebagai BARU), dan melihat piutang sekolah.
 * Kunjungan yang gagal terkirim (sinyal hilang) masuk antrean lokal dan
 * disinkronkan saat online lagi. Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { notify } from '@/lib/feedback';
import { formatRupiah } from '@/lib/format';
import {
  createFieldOrder,
  listFieldItems,
  listFieldSchools,
  listSchoolInvoices,
  postSchoolVisit,
  type FieldInvoice,
  type FieldItem,
  type FieldSchool,
} from '@/lib/api/sls-field';

const selectCls = 'h-11 w-full rounded-md border border-input bg-background px-2 text-base';
const QUEUE_KEY = 'field-visit-queue-v1';

interface QueuedVisit {
  schoolId: string;
  schoolName: string;
  notes: string;
  at: string;
}

interface OrderLine {
  itemId: string;
  quantity: string;
  unitPrice: string;
}

function readQueue(): QueuedVisit[] {
  try {
    return JSON.parse(localStorage.getItem(QUEUE_KEY) ?? '[]');
  } catch {
    return [];
  }
}

export function SlsFieldPage() {
  const [schools, setSchools] = React.useState<FieldSchool[]>([]);
  const [items, setItems] = React.useState<FieldItem[]>([]);
  const [schoolId, setSchoolId] = React.useState('');
  const [visitNotes, setVisitNotes] = React.useState('');
  const [queue, setQueue] = React.useState<QueuedVisit[]>([]);
  const [lines, setLines] = React.useState<OrderLine[]>([{ itemId: '', quantity: '1', unitPrice: '' }]);
  const [lastOrder, setLastOrder] = React.useState<string | null>(null);
  const [invoices, setInvoices] = React.useState<FieldInvoice[] | null>(null);
  const [busy, setBusy] = React.useState(false);

  React.useEffect(() => {
    if ('serviceWorker' in navigator) {
      navigator.serviceWorker
        .register('/field-sw.js', { scope: '/app/sales/' })
        .catch(() => undefined);
    }
    setQueue(readQueue());
    listFieldSchools().then(setSchools).catch(() => notify('Gagal memuat daftar sekolah.', 'danger'));
    listFieldItems().then(setItems).catch(() => setItems([]));
  }, []);

  const syncQueue = React.useCallback(async () => {
    const q = readQueue();
    if (!q.length) return;
    const remaining: QueuedVisit[] = [];
    for (const v of q) {
      try {
        await postSchoolVisit(v.schoolId, v.notes);
      } catch {
        remaining.push(v);
      }
    }
    localStorage.setItem(QUEUE_KEY, JSON.stringify(remaining));
    setQueue(remaining);
    if (remaining.length < q.length) notify('Antrean kunjungan tersinkron.', 'success');
  }, []);

  React.useEffect(() => {
    void syncQueue();
    window.addEventListener('online', syncQueue);
    return () => window.removeEventListener('online', syncQueue);
  }, [syncQueue]);

  const schoolName = (id: string) => schools.find((s) => s.id === id)?.name ?? id;

  const submitVisit = async () => {
    if (!schoolId) return notify('Pilih sekolah dulu.', 'danger');
    if (!visitNotes.trim()) return notify('Catatan kunjungan wajib diisi.', 'danger');
    setBusy(true);
    try {
      await postSchoolVisit(schoolId, visitNotes.trim());
      notify(`Kunjungan ke ${schoolName(schoolId)} tercatat.`, 'success');
      setVisitNotes('');
    } catch {
      const q = [...readQueue(), { schoolId, schoolName: schoolName(schoolId), notes: visitNotes.trim(), at: new Date().toISOString() }];
      localStorage.setItem(QUEUE_KEY, JSON.stringify(q));
      setQueue(q);
      notify('Tidak ada sinyal — kunjungan masuk antrean dan akan disinkronkan.', 'warn');
      setVisitNotes('');
    } finally {
      setBusy(false);
    }
  };

  const setLine = (idx: number, patch: Partial<OrderLine>) =>
    setLines((ls) => ls.map((l, i) => (i === idx ? { ...l, ...patch } : l)));

  const pickItem = (idx: number, itemId: string) => {
    const item = items.find((i) => i.id === itemId);
    setLine(idx, { itemId, unitPrice: item ? String(item.salePrice || '') : '' });
  };

  const orderTotal = lines.reduce((s, l) => s + (Number(l.quantity) || 0) * (Number(l.unitPrice) || 0), 0);

  const submitOrder = async () => {
    if (!schoolId) return notify('Pilih sekolah dulu.', 'danger');
    const valid = lines.filter((l) => l.itemId && Number(l.quantity) > 0 && Number(l.unitPrice) >= 0);
    if (!valid.length) return notify('Isi minimal satu baris item.', 'danger');
    setBusy(true);
    try {
      const res = await createFieldOrder({
        customerId: schoolId,
        lines: valid.map((l) => {
          const item = items.find((i) => i.id === l.itemId);
          return {
            itemId: l.itemId,
            unitId: item?.baseUnitId ?? '115',
            quantity: l.quantity,
            unitPrice: l.unitPrice || '0',
          };
        }),
      });
      setLastOrder(res.docNumber);
      notify(`Order ${res.docNumber} dibuat — muncul di Order Hub sebagai BARU.`, 'success');
      setLines([{ itemId: '', quantity: '1', unitPrice: '' }]);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal membuat order.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  const loadInvoices = async () => {
    if (!schoolId) return notify('Pilih sekolah dulu.', 'danger');
    setBusy(true);
    try {
      setInvoices(await listSchoolInvoices(schoolId));
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat piutang.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  const outstanding = (invoices ?? [])
    .filter((i) => i.status === 'POSTED' && i.settlementStatus !== 'PAID')
    .reduce((s, i) => s + (Number(i.grandTotal) || 0), 0);

  return (
    <div className="mx-auto max-w-md space-y-4 pb-10">
      <div>
        <h1 className="text-lg font-semibold">Sales Lapangan</h1>
        <p className="text-sm text-muted-foreground">
          Kunjungan, order cepat, dan piutang sekolah — langsung dari ponsel. Bisa dipasang di layar utama (PWA).
        </p>
      </div>

      <Card className="p-4">
        <label className="mb-1 block text-sm font-medium">Sekolah</label>
        <select className={selectCls} value={schoolId} onChange={(e) => { setSchoolId(e.target.value); setInvoices(null); }}>
          <option value="">— pilih sekolah —</option>
          {schools.map((s) => (
            <option key={s.id} value={s.id}>{s.name}</option>
          ))}
        </select>
      </Card>

      <Card className="p-4">
        <h2 className="mb-2 text-base font-semibold">Catat Kunjungan</h2>
        <textarea
          className="h-24 w-full rounded-md border border-input bg-background p-2 text-base"
          placeholder="Bertemu kepala sekolah & bendahara, bahas kebutuhan semester…"
          value={visitNotes}
          onChange={(e) => setVisitNotes(e.target.value)}
        />
        <Button className="mt-2 h-11 w-full" disabled={busy} onClick={() => void submitVisit()}>
          Simpan Kunjungan
        </Button>
        {queue.length > 0 && (
          <div className="mt-3 rounded-md bg-muted p-2 text-sm">
            <div className="mb-1 font-medium">{queue.length} kunjungan menunggu sinkron</div>
            {queue.map((q, i) => (
              <div key={i} className="text-muted-foreground">• {q.schoolName} — {q.notes.slice(0, 60)}</div>
            ))}
            <Button variant="ghost" className="mt-1" disabled={busy} onClick={() => void syncQueue()}>
              Sinkronkan Sekarang
            </Button>
          </div>
        )}
      </Card>

      <Card className="p-4">
        <h2 className="mb-2 text-base font-semibold">Order Cepat</h2>
        <div className="space-y-2">
          {lines.map((l, idx) => (
            <div key={idx} className="flex gap-2">
              <select className={`${selectCls} flex-1`} value={l.itemId} onChange={(e) => pickItem(idx, e.target.value)}>
                <option value="">— item —</option>
                {items.map((it) => (
                  <option key={it.id} value={it.id}>{it.name}</option>
                ))}
              </select>
              <Input type="number" className="h-11 w-20 text-base" value={l.quantity} onChange={(e) => setLine(idx, { quantity: e.target.value })} />
              <Input type="number" className="h-11 w-28 text-base" placeholder="Harga" value={l.unitPrice} onChange={(e) => setLine(idx, { unitPrice: e.target.value })} />
              {lines.length > 1 && (
                <Button variant="ghost" className="h-11" onClick={() => setLines((ls) => ls.filter((_, i) => i !== idx))}>✕</Button>
              )}
            </div>
          ))}
        </div>
        <div className="mt-2 flex items-center justify-between">
          <Button variant="ghost" onClick={() => setLines((ls) => [...ls, { itemId: '', quantity: '1', unitPrice: '' }])}>
            + Baris
          </Button>
          <span className="text-sm font-medium">Total {formatRupiah(orderTotal)}</span>
        </div>
        <Button className="mt-2 h-11 w-full" disabled={busy} onClick={() => void submitOrder()}>
          Buat Order (DRAFT)
        </Button>
        {lastOrder && (
          <p className="mt-2 text-sm text-muted-foreground">
            Order terakhir: <span className="font-medium text-foreground">{lastOrder}</span> — cek di Order Hub.
          </p>
        )}
      </Card>

      <Card className="p-4">
        <div className="mb-2 flex items-center justify-between">
          <h2 className="text-base font-semibold">Piutang Sekolah</h2>
          <Button variant="ghost" disabled={busy} onClick={() => void loadInvoices()}>
            Muat
          </Button>
        </div>
        {invoices === null ? (
          <p className="text-sm text-muted-foreground">Pilih sekolah lalu muat untuk melihat tagihannya.</p>
        ) : (
          <>
            <div className="mb-2 text-sm">
              Belum lunas (POSTED): <span className="font-semibold">{formatRupiah(outstanding)}</span>
            </div>
            <div className="space-y-1">
              {invoices.map((inv) => (
                <div key={inv.id} className="flex items-center justify-between border-b py-1 text-sm">
                  <span>{inv.docNumber} · {inv.docDate ?? ''}</span>
                  <span className="flex items-center gap-2">
                    {formatRupiah(Number(inv.grandTotal) || 0)}
                    <Badge variant={inv.settlementStatus === 'PAID' ? 'success' : 'default'}>{inv.settlementStatus}</Badge>
                  </span>
                </div>
              ))}
              {!invoices.length && <p className="text-sm text-muted-foreground">Tidak ada faktur untuk sekolah ini.</p>}
            </div>
          </>
        )}
      </Card>
    </div>
  );
}

'use client';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { api, fmtIDR, PortalOrder } from '@/lib/api';
import { cartRef, clearCart, loadCart, saveCart, CartLine } from '@/lib/cart';

export default function KebutuhanPage() {
  const router = useRouter();
  const [lines, setLines] = useState<CartLine[]>([]);
  const [funding, setFunding] = useState('BOS');
  const [budgetYear, setBudgetYear] = useState(String(new Date().getFullYear()));
  const [notes, setNotes] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    setLines(loadCart());
  }, []);

  function update(next: CartLine[]) {
    setLines(next);
    saveCart(next);
  }

  const total = lines.reduce((s, l) => s + Number(l.salePrice) * l.qty, 0);

  async function checkout() {
    setBusy(true);
    setError('');
    try {
      const order = await api<PortalOrder>('/portal/orders', {
        method: 'POST',
        body: {
          lines: lines.map((l) => ({ itemId: l.itemId, quantity: l.qty })),
          fundingSource: funding,
          budgetYear: Number(budgetYear) || undefined,
          notes: notes || undefined,
          clientRef: cartRef(),
        },
      });
      clearCart();
      router.push(`/pesanan/${order.id}?baru=1`);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Gagal membuat pesanan');
    } finally {
      setBusy(false);
    }
  }

  if (lines.length === 0)
    return (
      <div>
        <h1 className="page">Daftar Kebutuhan</h1>
        <div className="card" style={{ marginTop: 16 }}>
          <h2 className="sec">Daftar kebutuhan masih kosong</h2>
          <p className="muted">
            Tambahkan buku dari <Link href="/katalog">katalog</Link> terlebih dahulu.
          </p>
        </div>
      </div>
    );

  return (
    <div>
      <h1 className="page">Daftar Kebutuhan</h1>
      <p className="muted" style={{ marginTop: 0 }}>
        Periksa kembali kebutuhan sekolah Anda, lalu kirim sebagai pesanan.
        Pesanan masuk ke antrean Order Hub Bahtera Madani untuk diproses.
      </p>
      {error && <div className="error">{error}</div>}
      <div className="card" style={{ marginTop: 14 }}>
        <table className="list">
          <thead>
            <tr>
              <th>Item</th>
              <th>Harga</th>
              <th>Jumlah</th>
              <th>Subtotal</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {lines.map((l) => (
              <tr key={l.itemId}>
                <td>
                  <div style={{ fontWeight: 700 }}>{l.name}</div>
                  <div className="small muted">
                    {l.code} · per {l.unit ?? 'unit'}
                  </div>
                </td>
                <td>{fmtIDR(l.salePrice)}</td>
                <td>
                  <span className="qty">
                    <button
                      onClick={() =>
                        update(lines.map((x) => (x.itemId === l.itemId ? { ...x, qty: Math.max(1, x.qty - 1) } : x)))
                      }
                    >
                      −
                    </button>
                    <input
                      value={l.qty}
                      onChange={(e) => {
                        const v = Math.max(1, Number(e.target.value) || 1);
                        update(lines.map((x) => (x.itemId === l.itemId ? { ...x, qty: v } : x)));
                      }}
                    />
                    <button
                      onClick={() =>
                        update(lines.map((x) => (x.itemId === l.itemId ? { ...x, qty: x.qty + 1 } : x)))
                      }
                    >
                      +
                    </button>
                  </span>
                </td>
                <td style={{ fontWeight: 700 }}>{fmtIDR(Number(l.salePrice) * l.qty)}</td>
                <td>
                  <button className="btn btn-ghost btn-sm" onClick={() => update(lines.filter((x) => x.itemId !== l.itemId))}>
                    Hapus
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        <div style={{ textAlign: 'right', fontSize: 18, fontWeight: 800, color: 'var(--navy)', marginTop: 14 }}>
          Total {fmtIDR(total)}
        </div>
      </div>

      <div className="card" style={{ marginTop: 14 }}>
        <h2 className="sec">Detail pesanan</h2>
        <div className="grid2">
          <div className="field">
            <label>Sumber dana</label>
            <select value={funding} onChange={(e) => setFunding(e.target.value)}>
              <option value="BOS">Dana BOS</option>
              <option value="NON_BOS">Non-BOS (yayasan/komite/lainnya)</option>
            </select>
          </div>
          <div className="field">
            <label>Tahun anggaran</label>
            <input value={budgetYear} onChange={(e) => setBudgetYear(e.target.value)} inputMode="numeric" />
          </div>
        </div>
        <div className="field">
          <label>Catatan untuk Bahtera Madani (opsional)</label>
          <textarea rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="Mis. kirim sebelum semester dimulai" />
        </div>
        <button className="btn btn-amber" onClick={checkout} disabled={busy} style={{ width: '100%' }}>
          {busy ? 'Mengirim pesanan…' : 'Kirim sebagai pesanan'}
        </button>
        <p className="small muted" style={{ textAlign: 'center' }}>
          Harga mengikuti katalog saat pesanan dibuat. Penawaran resmi dan
          konfirmasi stok akan diterbitkan tim kami pada pesanan Anda.
        </p>
      </div>
    </div>
  );
}

'use client';
import { useEffect, useState } from 'react';
import Link from 'next/link';
import { api, fmtDate, fmtIDR, PortalInvoice, PortalOrder } from '@/lib/api';

export default function DashboardPage() {
  const [orders, setOrders] = useState<PortalOrder[]>([]);
  const [invoices, setInvoices] = useState<PortalInvoice[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    Promise.all([
      api<{ data: PortalOrder[] }>('/portal/orders'),
      api<{ data: PortalInvoice[] }>('/portal/invoices'),
    ])
      .then(([o, i]) => {
        setOrders(o.data);
        setInvoices(i.data);
      })
      .catch((e) => setError(e.message));
  }, []);

  const open = invoices.filter((i) => i.settlementStatus !== 'PAID');
  const openTotal = open.reduce((s, i) => s + Number(i.grandTotal), 0);
  const active = orders.filter((o) => !['LUNAS'].includes(o.stage));

  return (
    <div>
      <h1 className="page">Dashboard</h1>
      <p className="muted" style={{ marginTop: 0 }}>
        Ringkasan pemesanan dan tagihan sekolah Anda.
      </p>
      {error && <div className="error">{error}</div>}
      <div className="stat-grid">
        <div className="card stat">
          <div className="muted small">Pesanan berjalan</div>
          <div className="num">{active.length}</div>
          <Link href="/pesanan" className="small" style={{ fontWeight: 700 }}>
            Lihat pesanan →
          </Link>
        </div>
        <div className="card stat">
          <div className="muted small">Tagihan belum lunas</div>
          <div className="num">{open.length}</div>
          <div className="small muted">Total {fmtIDR(openTotal)}</div>
        </div>
        <div className="card stat">
          <div className="muted small">Total pesanan</div>
          <div className="num">{orders.length}</div>
          <Link href="/katalog" className="small" style={{ fontWeight: 700 }}>
            Buat pesanan baru →
          </Link>
        </div>
      </div>

      <div className="card">
        <h2 className="sec">Pesanan terakhir</h2>
        {orders.length === 0 ? (
          <p className="muted">
            Belum ada pesanan. Jelajahi <Link href="/katalog">katalog buku</Link> untuk memulai.
          </p>
        ) : (
          <table className="list">
            <thead>
              <tr>
                <th>Nomor</th>
                <th>Tanggal</th>
                <th>Total</th>
                <th>Tahap</th>
              </tr>
            </thead>
            <tbody>
              {orders.slice(0, 5).map((o) => (
                <tr key={o.id}>
                  <td>
                    <Link href={`/pesanan/${o.id}`} style={{ fontWeight: 700 }}>
                      {o.docNumber}
                    </Link>
                  </td>
                  <td>{fmtDate(o.docDate)}</td>
                  <td>{fmtIDR(o.grandTotal)}</td>
                  <td>
                    <span className={`chip ${o.stage === 'LUNAS' ? 'chip-ok' : o.stage === 'DITAGIH' ? 'chip-amber' : ''}`}>
                      {o.stageLabel}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

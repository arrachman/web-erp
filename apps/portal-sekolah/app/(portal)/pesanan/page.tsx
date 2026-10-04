'use client';
import { useEffect, useState } from 'react';
import Link from 'next/link';
import { api, fmtDate, fmtIDR, PortalOrder } from '@/lib/api';

export default function PesananPage() {
  const [orders, setOrders] = useState<PortalOrder[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    api<{ data: PortalOrder[] }>('/portal/orders')
      .then((r) => setOrders(r.data))
      .catch((e) => setError(e.message));
  }, []);

  return (
    <div>
      <h1 className="page">Pesanan</h1>
      <p className="muted" style={{ marginTop: 0 }}>
        Semua pesanan sekolah Anda beserta tahap prosesnya.
      </p>
      {error && <div className="error">{error}</div>}
      <div className="card" style={{ marginTop: 14 }}>
        {orders.length === 0 ? (
          <p className="muted">
            Belum ada pesanan. Mulai dari <Link href="/katalog">katalog</Link>.
          </p>
        ) : (
          <table className="list">
            <thead>
              <tr>
                <th>Nomor</th>
                <th>Tanggal</th>
                <th>Sumber dana</th>
                <th>Total</th>
                <th>Tahap</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((o) => (
                <tr key={o.id}>
                  <td>
                    <Link href={`/pesanan/${o.id}`} style={{ fontWeight: 700 }}>
                      {o.docNumber}
                    </Link>
                  </td>
                  <td>{fmtDate(o.docDate)}</td>
                  <td>
                    {o.fundingSource === 'BOS'
                      ? `BOS ${o.budgetYear ?? ''}`
                      : o.fundingSource === 'NON_BOS'
                        ? 'Non-BOS'
                        : '—'}
                  </td>
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

'use client';
import { useEffect, useState } from 'react';
import Link from 'next/link';
import { api, fmtDate, fmtIDR, PortalInvoice } from '@/lib/api';

export default function TagihanPage() {
  const [invoices, setInvoices] = useState<PortalInvoice[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    api<{ data: PortalInvoice[] }>('/portal/invoices')
      .then((r) => setInvoices(r.data))
      .catch((e) => setError(e.message));
  }, []);

  const open = invoices.filter((i) => i.settlementStatus !== 'PAID');
  const openTotal = open.reduce((s, i) => s + Number(i.grandTotal), 0);

  return (
    <div>
      <h1 className="page">Tagihan</h1>
      <p className="muted" style={{ marginTop: 0 }}>
        Invoice resmi sekolah Anda dari CV Bahtera Madani.
      </p>
      {error && <div className="error">{error}</div>}
      {open.length > 0 && (
        <div className="card" style={{ marginTop: 14, borderColor: '#f3d48a', background: '#fffaf0' }}>
          <strong>{open.length} tagihan belum lunas</strong> dengan total{' '}
          <strong>{fmtIDR(openTotal)}</strong>. Pembayaran mengikuti instruksi
          pada invoice resmi; status di sini terbarui otomatis setelah
          pembayaran Anda diterima dan dicatat.
        </div>
      )}
      <div className="card" style={{ marginTop: 14 }}>
        {invoices.length === 0 ? (
          <p className="muted">Belum ada tagihan.</p>
        ) : (
          <table className="list">
            <thead>
              <tr>
                <th>Invoice</th>
                <th>Tanggal</th>
                <th>Jatuh tempo</th>
                <th>Pesanan</th>
                <th>Total</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {invoices.map((v) => (
                <tr key={v.id}>
                  <td style={{ fontWeight: 700 }}>{v.docNumber}</td>
                  <td>{fmtDate(v.docDate)}</td>
                  <td>{fmtDate(v.dueDate)}</td>
                  <td>
                    {v.orderId ? (
                      <Link href={`/pesanan/${v.orderId}`}>{v.orderDocNumber ?? 'Lihat'}</Link>
                    ) : (
                      '—'
                    )}
                  </td>
                  <td>{fmtIDR(v.grandTotal)}</td>
                  <td>
                    <span className={`chip ${v.settlementStatus === 'PAID' ? 'chip-ok' : v.settlementStatus === 'PARTIAL' ? 'chip-amber' : 'chip-danger'}`}>
                      {v.settlementStatus === 'PAID' ? 'Lunas' : v.settlementStatus === 'PARTIAL' ? 'Sebagian' : 'Belum dibayar'}
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

'use client';
import { useEffect, useState } from 'react';
import { useParams, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { api, fmtDate, fmtIDR } from '@/lib/api';

const STAGES = ['BARU', 'DIKONFIRMASI', 'DISIAPKAN', 'DIKIRIM', 'DITERIMA', 'DITAGIH', 'LUNAS'];
const STAGE_LABELS: Record<string, string> = {
  BARU: 'Baru',
  DIKONFIRMASI: 'Dikonfirmasi',
  DISIAPKAN: 'Disiapkan',
  DIKIRIM: 'Dikirim',
  DITERIMA: 'Diterima (BAST)',
  DITAGIH: 'Ditagih',
  LUNAS: 'Lunas',
};

interface OrderDetail {
  id: string;
  docNumber: string;
  docDate: string;
  grandTotal: string;
  stage: string;
  stageLabel: string;
  fundingSource: string | null;
  budgetYear: number | null;
  notes: string | null;
  lines: { id: string; itemName: string | null; quantity: string; unitPrice: string; lineTotal: string }[];
  deliveryOrders: { id: string; docNumber: string; docDate: string; status: string }[];
  invoices: {
    id: string;
    docNumber: string;
    docDate: string;
    dueDate: string | null;
    grandTotal: string;
    settlementStatus: string;
  }[];
}

export default function PesananDetailPage() {
  const params = useParams<{ id: string }>();
  const search = useSearchParams();
  const [order, setOrder] = useState<OrderDetail | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    api<OrderDetail>(`/portal/orders/${params.id}`)
      .then(setOrder)
      .catch((e) => setError(e.message));
  }, [params.id]);

  if (error) return <div className="error">{error}</div>;
  if (!order) return <p className="muted">Memuat pesanan…</p>;

  const stageIdx = STAGES.indexOf(order.stage);

  return (
    <div>
      <Link href="/pesanan" className="small" style={{ fontWeight: 700 }}>
        ← Semua pesanan
      </Link>
      <h1 className="page" style={{ marginTop: 8 }}>
        Pesanan {order.docNumber}
      </h1>
      <p className="muted" style={{ marginTop: 0 }}>
        {fmtDate(order.docDate)}
        {order.fundingSource ? ` · ${order.fundingSource === 'BOS' ? `Dana BOS ${order.budgetYear ?? ''}` : 'Non-BOS'}` : ''}
      </p>
      {search.get('baru') && (
        <div className="success" style={{ marginBottom: 14 }}>
          Pesanan terkirim. Tim Bahtera Madani akan mengonfirmasi dan
          menerbitkan penawaran resmi pada pesanan ini.
        </div>
      )}

      <div className="card">
        <div className="stepper">
          {STAGES.map((s, i) => (
            <span key={s} style={{ display: 'flex', alignItems: 'center' }}>
              <span className={`step ${i <= stageIdx ? 'done' : ''}`}>
                <span className="dot">{i < stageIdx ? '✓' : i + 1}</span>
                {STAGE_LABELS[s]}
              </span>
              {i < STAGES.length - 1 && <span className="bar" />}
            </span>
          ))}
        </div>
        {order.notes && (
          <p className="small muted" style={{ marginBottom: 0 }}>
            Catatan: {order.notes}
          </p>
        )}
      </div>

      <div className="card" style={{ marginTop: 14 }}>
        <h2 className="sec">Rincian item</h2>
        <table className="list">
          <thead>
            <tr>
              <th>Item</th>
              <th>Jumlah</th>
              <th>Harga</th>
              <th>Subtotal</th>
            </tr>
          </thead>
          <tbody>
            {order.lines.map((l) => (
              <tr key={l.id}>
                <td style={{ fontWeight: 600 }}>{l.itemName ?? '—'}</td>
                <td>{Number(l.quantity)}</td>
                <td>{fmtIDR(l.unitPrice)}</td>
                <td style={{ fontWeight: 700 }}>{fmtIDR(l.lineTotal)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        <div style={{ textAlign: 'right', fontWeight: 800, color: 'var(--navy)', fontSize: 17, marginTop: 12 }}>
          Total {fmtIDR(order.grandTotal)}
        </div>
      </div>

      {order.deliveryOrders.length > 0 && (
        <div className="card" style={{ marginTop: 14 }}>
          <h2 className="sec">Pengiriman (surat jalan)</h2>
          <table className="list">
            <tbody>
              {order.deliveryOrders.map((d) => (
                <tr key={d.id}>
                  <td style={{ fontWeight: 700 }}>{d.docNumber}</td>
                  <td>{fmtDate(d.docDate)}</td>
                  <td>
                    <span className="chip">{d.status}</span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {order.invoices.length > 0 && (
        <div className="card" style={{ marginTop: 14 }}>
          <h2 className="sec">Tagihan pesanan ini</h2>
          <table className="list">
            <tbody>
              {order.invoices.map((v) => (
                <tr key={v.id}>
                  <td style={{ fontWeight: 700 }}>{v.docNumber}</td>
                  <td>{fmtDate(v.docDate)}</td>
                  <td>{fmtIDR(v.grandTotal)}</td>
                  <td>
                    <span className={`chip ${v.settlementStatus === 'PAID' ? 'chip-ok' : 'chip-amber'}`}>
                      {v.settlementStatus === 'PAID' ? 'Lunas' : v.settlementStatus === 'PARTIAL' ? 'Sebagian' : 'Belum dibayar'}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

'use client';
import { useCallback, useEffect, useState } from 'react';
import Link from 'next/link';
import { api, fmtDate, fmtIDR, PortalInvoice, PortalPayment } from '@/lib/api';

export default function TagihanPage() {
  const [invoices, setInvoices] = useState<PortalInvoice[]>([]);
  const [payments, setPayments] = useState<PortalPayment[]>([]);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState('');
  const [notice, setNotice] = useState('');

  const load = useCallback(() => {
    api<{ data: PortalInvoice[] }>('/portal/invoices')
      .then((r) => setInvoices(r.data))
      .catch((e) => setError(e.message));
    api<PortalPayment[]>('/portal/payments')
      .then((r) => setPayments(Array.isArray(r) ? r : []))
      .catch(() => undefined);
  }, []);

  useEffect(load, [load]);

  const open = invoices.filter((i) => i.settlementStatus !== 'PAID');
  const openTotal = open.reduce((s, i) => s + Number(i.grandTotal), 0);
  const pendingByInvoice = new Map(
    payments.filter((p) => p.status === 'PENDING').map((p) => [p.invoiceId, p]),
  );

  async function bayar(inv: PortalInvoice) {
    setBusy(inv.id);
    setNotice('');
    try {
      const p = await api<PortalPayment>(`/portal/invoices/${inv.id}/pay`, { method: 'POST' });
      setPayments((prev) => [p, ...prev.filter((x) => x.id !== p.id)]);
      setNotice(
        `Nomor VA untuk ${inv.docNumber} dibuat. Transfer tepat ${fmtIDR(p.amount)} ke VA ${p.vaNumber} — status terbarui otomatis setelah pembayaran diterima.`,
      );
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setBusy('');
    }
  }

  return (
    <div>
      <h1 className="page">Tagihan</h1>
      <p className="muted" style={{ marginTop: 0 }}>
        Invoice resmi sekolah Anda dari CV Bahtera Madani.
      </p>
      {error && <div className="error">{error}</div>}
      {notice && (
        <div className="card" style={{ marginTop: 14, borderColor: '#bfe3c8', background: '#f2fbf4' }}>
          {notice}
        </div>
      )}
      {open.length > 0 && (
        <div className="card" style={{ marginTop: 14, borderColor: '#f3d48a', background: '#fffaf0' }}>
          <strong>{open.length} tagihan belum lunas</strong> dengan total{' '}
          <strong>{fmtIDR(openTotal)}</strong>. Klik <strong>Bayar</strong> pada
          tagihan untuk mendapatkan nomor Virtual Account; status di sini
          terbarui otomatis setelah pembayaran Anda diterima.
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
                <th></th>
              </tr>
            </thead>
            <tbody>
              {invoices.map((v) => {
                const pending = pendingByInvoice.get(v.id);
                return (
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
                    <td style={{ minWidth: 210 }}>
                      {v.settlementStatus !== 'PAID' && v.status === 'POSTED' ? (
                        pending ? (
                          <div style={{ fontSize: 13 }}>
                            <div>
                              VA <strong>{pending.vaNumber}</strong> · {fmtIDR(pending.amount)}
                            </div>
                            <div className="muted">
                              Menunggu pembayaran
                              {pending.expiresAt ? ` · s.d. ${fmtDate(pending.expiresAt)}` : ''}
                            </div>
                          </div>
                        ) : (
                          <button
                            className="btn"
                            disabled={busy === v.id}
                            onClick={() => bayar(v)}
                          >
                            {busy === v.id ? 'Memproses…' : 'Bayar'}
                          </button>
                        )
                      ) : null}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
      <p className="muted" style={{ fontSize: 12 }}>
        Pembayaran online saat ini berjalan dalam mode simulasi (VA uji) —
        kanal pembayaran resmi diaktifkan setelah provider payment gateway
        dipilih.
      </p>
    </div>
  );
}

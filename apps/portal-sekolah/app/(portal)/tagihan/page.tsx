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
  const paymentByInvoice = new Map(
    payments
      .filter((p) => p.status !== 'EXPIRED')
      .map((p) => [p.invoiceId, p]),
  );

  async function bayar(inv: PortalInvoice) {
    setBusy(inv.id);
    setNotice('');
    try {
      const p = await api<PortalPayment>(`/portal/invoices/${inv.id}/pay`, { method: 'POST' });
      setPayments((prev) => [p, ...prev.filter((x) => x.id !== p.id)]);
      setNotice(
        `Instruksi transfer untuk ${inv.docNumber} dibuat. Transfer tepat ${fmtIDR(p.amount)} lalu klik "Saya Sudah Transfer".`,
      );
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setBusy('');
    }
  }

  async function confirmSent(p: PortalPayment) {
    setBusy(p.id);
    setNotice('');
    try {
      const updated = await api<PortalPayment>(`/portal/payments/${p.id}/confirm-sent`, {
        method: 'POST',
        body: JSON.stringify({}),
      });
      setPayments((prev) => prev.map((x) => (x.id === updated.id ? updated : x)));
      setNotice('Terima kasih — klaim transfer Anda diteruskan ke admin untuk diverifikasi. Status berubah menjadi Lunas setelah dikonfirmasi.');
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
          tagihan untuk melihat instruksi transfer, lalu klik{' '}
          <strong>Saya Sudah Transfer</strong> — admin kami memverifikasi dan
          status berubah menjadi Lunas setelah dikonfirmasi.
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
                const payment = paymentByInvoice.get(v.id);
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
                    <td style={{ minWidth: 260 }}>
                      {v.settlementStatus !== 'PAID' && v.status === 'POSTED' ? (
                        !payment ? (
                          <button
                            className="btn"
                            disabled={busy === v.id}
                            onClick={() => bayar(v)}
                          >
                            {busy === v.id ? 'Memproses…' : 'Bayar'}
                          </button>
                        ) : payment.status === 'MENUNGGU_KONFIRMASI' ? (
                          <div style={{ fontSize: 13 }}>
                            <strong>Menunggu konfirmasi admin</strong>
                            <div className="muted">
                              Klaim transfer {fmtIDR(payment.amount)} sedang diverifikasi.
                            </div>
                          </div>
                        ) : (
                          <div style={{ fontSize: 13 }}>
                            {payment.status === 'DITOLAK' && (
                              <div style={{ color: '#b91c1c', marginBottom: 4 }}>
                                Klaim ditolak: {payment.rejectedReason ?? 'dana tidak ditemukan'}. Silakan transfer ulang lalu klaim kembali.
                              </div>
                            )}
                            {payment.instructions ? (
                              <div>
                                Transfer ke <strong>{payment.instructions.bankName} {payment.instructions.accountNumber}</strong>
                                <br />a.n. {payment.instructions.accountHolder} · {fmtIDR(payment.amount)}
                                <br />
                                <span className="muted">Cantumkan kode: </span>
                                <strong>{payment.instructions.reference}</strong>
                              </div>
                            ) : (
                              <div className="muted">Rekening tujuan belum diatur — hubungi admin.</div>
                            )}
                            <button
                              className="btn"
                              style={{ marginTop: 6 }}
                              disabled={busy === payment.id}
                              onClick={() => confirmSent(payment)}
                            >
                              {busy === payment.id ? 'Mengirim…' : 'Saya Sudah Transfer'}
                            </button>
                          </div>
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
        Pembayaran dilakukan via transfer bank manual ke rekening di atas dan
        dikonfirmasi oleh admin CV Bahtera Madani (maksimal 1×24 jam kerja).
      </p>
    </div>
  );
}

'use client';

import { useCallback, useEffect, useState } from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import {
  confirmAdminPayment,
  getTransferTarget,
  listAdminPayments,
  rejectAdminPayment,
  type PortalPaymentAdmin,
  type TransferTarget,
} from '@/lib/api/fin-portal-payments';
import { notify } from '@/lib/feedback';
import { formatRupiah } from '@/lib/format';

const STATUS_BADGE: Record<string, { variant: 'default' | 'info' | 'danger' | 'success'; label: string }> = {
  MENUNGGU_KONFIRMASI: { variant: 'info', label: 'Menunggu konfirmasi' },
  PENDING: { variant: 'default', label: 'Belum diklaim' },
  PAID: { variant: 'success', label: 'Lunas' },
  DITOLAK: { variant: 'danger', label: 'Ditolak' },
  EXPIRED: { variant: 'default', label: 'Kedaluwarsa' },
};

function fmtDateTime(v: string | null): string {
  if (!v) return '—';
  return new Date(v).toLocaleString('id-ID', { dateStyle: 'medium', timeStyle: 'short' });
}

export default function FinPaymentConfirmationsPage() {
  const [rows, setRows] = useState<PortalPaymentAdmin[]>([]);
  const [target, setTarget] = useState<TransferTarget | null>(null);
  const [busy, setBusy] = useState('');
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [list, tgt] = await Promise.all([listAdminPayments(), getTransferTarget()]);
      setRows(list);
      setTarget(tgt);
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal memuat pembayaran', 'danger');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  async function confirm(row: PortalPaymentAdmin) {
    setBusy(row.id);
    try {
      await confirmAdminPayment(row.id);
      notify(`Pembayaran ${row.invoiceNumber ?? ''} dikonfirmasi — invoice lunas, kwitansi (IP) terbit.`, 'success');
      await load();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Konfirmasi gagal', 'danger');
    } finally {
      setBusy('');
    }
  }

  async function reject(row: PortalPaymentAdmin) {
    const reason = window.prompt(
      `Tolak klaim transfer ${row.providerRef}? Alasan (terlihat oleh sekolah):`,
      'Dana tidak ditemukan di mutasi bank.',
    );
    if (!reason) return;
    setBusy(row.id);
    try {
      await rejectAdminPayment(row.id, reason);
      notify('Klaim ditolak. Sekolah dapat mengklaim ulang setelah transfer.', 'info');
      await load();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Penolakan gagal', 'danger');
    } finally {
      setBusy('');
    }
  }

  const waiting = rows.filter((r) => r.status === 'MENUNGGU_KONFIRMASI');
  const others = rows.filter((r) => r.status !== 'MENUNGGU_KONFIRMASI');

  function table(data: PortalPaymentAdmin[], withActions: boolean) {
    if (data.length === 0) return <p className="text-sm text-muted-foreground">Tidak ada data.</p>;
    return (
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-muted-foreground">
              <th className="py-2 pr-3">Invoice</th>
              <th className="py-2 pr-3">Sekolah</th>
              <th className="py-2 pr-3">Kode Referensi</th>
              <th className="py-2 pr-3">Nominal</th>
              <th className="py-2 pr-3">Diklaim</th>
              <th className="py-2 pr-3">Status</th>
              {withActions && <th className="py-2"></th>}
            </tr>
          </thead>
          <tbody>
            {data.map((r) => {
              const badge = STATUS_BADGE[r.status] ?? STATUS_BADGE.PENDING;
              return (
                <tr key={r.id} className="border-t">
                  <td className="py-2 pr-3 font-medium">{r.invoiceNumber ?? r.invoiceId}</td>
                  <td className="py-2 pr-3">{r.partnerName ?? r.partnerId}</td>
                  <td className="py-2 pr-3 font-mono text-xs">{r.providerRef}</td>
                  <td className="py-2 pr-3">{formatRupiah(Number(r.amount))}</td>
                  <td className="py-2 pr-3">
                    {fmtDateTime(r.claimedAt)}
                    {r.claimNote ? <div className="text-xs text-muted-foreground">{r.claimNote}</div> : null}
                    {r.status === 'DITOLAK' && r.rejectedReason ? (
                      <div className="text-xs text-red-600">{r.rejectedReason}</div>
                    ) : null}
                  </td>
                  <td className="py-2 pr-3"><Badge variant={badge.variant}>{badge.label}</Badge></td>
                  {withActions && (
                    <td className="py-2 whitespace-nowrap">
                      <Button variant="primary" disabled={busy === r.id} onClick={() => confirm(r)}>
                        Konfirmasi
                      </Button>{' '}
                      <Button variant="ghost" disabled={busy === r.id} onClick={() => reject(r)}>
                        Tolak
                      </Button>
                    </td>
                  )}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-xl font-semibold">Konfirmasi Pembayaran Portal</h1>
        <p className="text-sm text-muted-foreground">
          Pembayaran tagihan portal via transfer manual. Verifikasi mutasi bank, lalu konfirmasi —
          kwitansi penerimaan (IP) terbit otomatis dan invoice menjadi lunas.
        </p>
      </div>

      <Card className="p-4">
        <div className="text-sm">
          <span className="text-muted-foreground">Rekening tujuan yang ditampilkan ke sekolah: </span>
          {target ? (
            <strong>
              {target.bankName} {target.accountNumber} a.n. {target.accountHolder}
            </strong>
          ) : (
            <strong className="text-red-600">belum diatur (atur rekening primer perusahaan)</strong>
          )}
        </div>
      </Card>

      <Card className="p-4 space-y-3">
        <h2 className="font-semibold">Menunggu Konfirmasi ({waiting.length})</h2>
        {loading ? <p className="text-sm text-muted-foreground">Memuat…</p> : table(waiting, true)}
      </Card>

      <Card className="p-4 space-y-3">
        <h2 className="font-semibold">Semua Pembayaran</h2>
        {loading ? <p className="text-sm text-muted-foreground">Memuat…</p> : table(others, false)}
      </Card>
    </div>
  );
}

'use client';

/**
 * Fase 3 W3 — Akun Portal Sekolah: antrean persetujuan pendaftaran portal.
 * Menyetujui akun membuat/menautkan partner CUST-SCHOOL + profil sekolah (A1)
 * di backend; akun lalu bisa login ke Portal Sekolah (port 3221).
 * Di bawahnya: lead dari landing page (W1) yang sudah menjadi partner PROSPEK.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { confirmAction, notify } from '@/lib/feedback';
import {
  activatePortalAccount,
  approvePortalAccount,
  listPortalAccounts,
  listPortalLeads,
  rejectPortalAccount,
  suspendPortalAccount,
  type PortalAccount,
  type PortalLead,
} from '@/lib/api/portal-admin';

const selectCls = 'h-9 rounded-md border border-input bg-background px-2 text-sm';

const STATUS_BADGE: Record<PortalAccount['status'], 'info' | 'success' | 'danger' | 'default'> = {
  PENDING: 'info',
  ACTIVE: 'success',
  REJECTED: 'danger',
  SUSPENDED: 'default',
};

const ROLE_LABEL: Record<PortalAccount['role'], string> = {
  KEPALA_SEKOLAH: 'Kepala Sekolah',
  BENDAHARA: 'Bendahara',
  OPERATOR: 'Operator / TU',
};

function fmtDate(v?: string | null): string {
  if (!v) return '—';
  const d = new Date(v);
  return Number.isNaN(d.getTime())
    ? String(v)
    : d.toLocaleDateString('id-ID', { day: 'numeric', month: 'short', year: 'numeric' });
}

export function PortalAccountsPage() {
  const [accounts, setAccounts] = React.useState<PortalAccount[]>([]);
  const [leads, setLeads] = React.useState<PortalLead[]>([]);
  const [status, setStatus] = React.useState('PENDING');
  const [search, setSearch] = React.useState('');
  const [busy, setBusy] = React.useState(false);

  const load = React.useCallback(async () => {
    setBusy(true);
    try {
      const [acc, ld] = await Promise.all([
        listPortalAccounts(status || undefined, search || undefined),
        listPortalLeads(),
      ]);
      setAccounts(acc.data);
      setLeads(ld.data);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat akun portal.', 'danger');
    } finally {
      setBusy(false);
    }
  }, [status, search]);

  React.useEffect(() => {
    load();
  }, [load]);

  const run = (fn: () => Promise<unknown>, okMsg: string) => async () => {
    try {
      await fn();
      notify(okMsg, 'success');
      load();
    } catch (e: any) {
      notify(e?.message ?? 'Aksi gagal.', 'danger');
    }
  };

  return (
    <div className="space-y-4 p-4">
      <div className="flex flex-wrap items-center gap-2">
        <h1 className="text-lg font-bold">Akun Portal Sekolah</h1>
        <span className="text-sm text-muted-foreground">
          Pendaftaran dari portal (port 3221) menunggu persetujuan sebelum bisa login.
        </span>
      </div>

      <Card className="p-4">
        <div className="mb-3 flex flex-wrap items-center gap-2">
          <select className={selectCls} value={status} onChange={(e) => setStatus(e.target.value)}>
            <option value="PENDING">Menunggu persetujuan</option>
            <option value="ACTIVE">Aktif</option>
            <option value="REJECTED">Ditolak</option>
            <option value="SUSPENDED">Ditangguhkan</option>
            <option value="">Semua status</option>
          </select>
          <Input
            placeholder="Cari sekolah / email / nama…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="h-9 w-64"
          />
          <Button size="sm" variant="ghost" onClick={load} disabled={busy}>
            Muat ulang
          </Button>
        </div>

        {accounts.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            {busy ? 'Memuat…' : 'Tidak ada akun pada filter ini.'}
          </p>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-muted-foreground">
                <th className="py-2 pr-3">Sekolah</th>
                <th className="py-2 pr-3">Penanggung jawab</th>
                <th className="py-2 pr-3">Email</th>
                <th className="py-2 pr-3">Status</th>
                <th className="py-2 pr-3">Terdaftar</th>
                <th className="py-2">Aksi</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((a) => (
                <tr key={a.id} className="border-b align-top">
                  <td className="py-2 pr-3">
                    <div className="font-semibold">{a.schoolName}</div>
                    <div className="text-muted-foreground">
                      {a.jenjang ?? '—'}
                      {a.npsn ? ` · NPSN ${a.npsn}` : ''}
                      {a.partnerId ? ' · tertaut mitra' : ''}
                    </div>
                  </td>
                  <td className="py-2 pr-3">
                    {a.fullName}
                    <div className="text-muted-foreground">{ROLE_LABEL[a.role] ?? a.role}</div>
                  </td>
                  <td className="py-2 pr-3">{a.email}</td>
                  <td className="py-2 pr-3">
                    <Badge variant={STATUS_BADGE[a.status]}>{a.status}</Badge>
                  </td>
                  <td className="py-2 pr-3">{fmtDate(a.createdAt)}</td>
                  <td className="py-2">
                    <div className="flex flex-wrap gap-1">
                      {(a.status === 'PENDING' || a.status === 'REJECTED') && (
                        <Button
                          size="sm"
                          onClick={() =>
                            confirmAction({
                              message: `Setujui akun portal ${a.schoolName}? Partner sekolah + profil CRM akan dibuat/ditautkan.`,
                              onConfirm: run(() => approvePortalAccount(a.id), 'Akun disetujui dan diaktifkan.'),
                            })
                          }
                        >
                          Setujui
                        </Button>
                      )}
                      {a.status === 'PENDING' && (
                        <Button
                          size="sm"
                          variant="ghost"
                          onClick={() =>
                            confirmAction({
                              message: `Tolak pendaftaran ${a.schoolName}?`,
                              onConfirm: run(() => rejectPortalAccount(a.id), 'Pendaftaran ditolak.'),
                            })
                          }
                        >
                          Tolak
                        </Button>
                      )}
                      {a.status === 'ACTIVE' && (
                        <Button
                          size="sm"
                          variant="ghost"
                          onClick={() =>
                            confirmAction({
                              message: `Tangguhkan akun ${a.schoolName}? Akun tidak bisa login sampai diaktifkan lagi.`,
                              onConfirm: run(() => suspendPortalAccount(a.id), 'Akun ditangguhkan.'),
                            })
                          }
                        >
                          Tangguhkan
                        </Button>
                      )}
                      {a.status === 'SUSPENDED' && (
                        <Button
                          size="sm"
                          onClick={run(() => activatePortalAccount(a.id), 'Akun diaktifkan kembali.')}
                        >
                          Aktifkan
                        </Button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Card>

      <Card className="p-4">
        <h2 className="mb-1 font-semibold">Lead landing page</h2>
        <p className="mb-3 text-sm text-muted-foreground">
          Permintaan penawaran dari landing page (port 3226). Setiap lead otomatis
          menjadi partner PROSPEK di CRM Sekolah.
        </p>
        {leads.length === 0 ? (
          <p className="text-sm text-muted-foreground">Belum ada lead.</p>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-muted-foreground">
                <th className="py-2 pr-3">Sekolah</th>
                <th className="py-2 pr-3">Kontak</th>
                <th className="py-2 pr-3">Kebutuhan</th>
                <th className="py-2">Waktu</th>
              </tr>
            </thead>
            <tbody>
              {leads.slice(0, 20).map((l) => (
                <tr key={l.id} className="border-b align-top">
                  <td className="py-2 pr-3">
                    <div className="font-semibold">{l.schoolName}</div>
                    <div className="text-muted-foreground">{l.jenjang ?? '—'}</div>
                  </td>
                  <td className="py-2 pr-3">
                    {l.contactName}
                    <div className="text-muted-foreground">{l.phone ?? l.email ?? '—'}</div>
                  </td>
                  <td className="py-2 pr-3">{l.message ?? '—'}</td>
                  <td className="py-2">{fmtDate(l.createdAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Card>
    </div>
  );
}

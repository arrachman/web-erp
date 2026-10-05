'use client';

/**
 * Fase 3 W6 — Pengaturan WhatsApp: status gateway, pairing device (QR),
 * kirim tes, dan kill-switch notifikasi otomatis. Template & log ada di
 * komponen pendamping (whatsapp-templates-section).
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { confirmAction, notify } from '@/lib/feedback';
import {
  activateWaDevice,
  addWaDevice,
  checkWaDevice,
  getWaDeviceQr,
  getWaHealth,
  getWaStats,
  listWaDevices,
  listWaTemplates,
  removeWaDevice,
  sendWaTest,
  updateWaSettings,
  type WaDevice,
  type WaHealth,
  type WaTemplate,
} from '@/lib/api/whatsapp';
import { WhatsappTemplatesSection } from './whatsapp-templates-section';

export function WhatsappPage() {
  const [health, setHealth] = React.useState<WaHealth | null>(null);
  const [devices, setDevices] = React.useState<WaDevice[]>([]);
  const [stats, setStats] = React.useState<{ sentToday: number; readToday: number; failedToday: number; readRate: number } | null>(null);
  const [templates, setTemplates] = React.useState<WaTemplate[]>([]);
  const [busy, setBusy] = React.useState(false);

  // Form tambah device + pairing
  const [devName, setDevName] = React.useState('WA Bahtera Madani');
  const [devPhone, setDevPhone] = React.useState('');
  const [pairing, setPairing] = React.useState<{ token: string; phone: string } | null>(null);
  const [qrUrl, setQrUrl] = React.useState<string | null>(null);
  const [qrNote, setQrNote] = React.useState('');

  // Kirim tes
  const [testPhone, setTestPhone] = React.useState('');
  const [testTemplateId, setTestTemplateId] = React.useState('');
  const [testBody, setTestBody] = React.useState('Tes WhatsApp dari ERP Bahtera Madani.');

  const load = React.useCallback(async () => {
    setBusy(true);
    try {
      const [h, d, s, t] = await Promise.all([
        getWaHealth(),
        listWaDevices().catch(() => ({ devices: [] as WaDevice[], hasActiveDevice: false })),
        getWaStats().catch(() => null),
        listWaTemplates().catch(() => [] as WaTemplate[]),
      ]);
      setHealth(h);
      setDevices(d.devices);
      setStats(s);
      setTemplates(t);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat pengaturan WhatsApp.', 'danger');
    } finally {
      setBusy(false);
    }
  }, []);

  React.useEffect(() => {
    load();
  }, [load]);

  // Alur pairing: ambil QR (refresh ~8 dtk bila belum siap) + polling status 4 dtk.
  React.useEffect(() => {
    if (!pairing) return;
    let stopped = false;
    let qrTimer: ReturnType<typeof setTimeout> | undefined;
    let pollTimer: ReturnType<typeof setInterval> | undefined;

    const fetchQr = async () => {
      try {
        const r = await getWaDeviceQr(pairing.token);
        if (stopped) return;
        if (r.alreadyConnected) {
          setQrNote('Device sudah terhubung — mengaktifkan…');
          await finishActivate();
          return;
        }
        if (r.qrUrl) {
          setQrUrl(r.qrUrl);
          setQrNote('Scan dengan WhatsApp → Perangkat tertaut → Tautkan perangkat.');
        }
      } catch (e: any) {
        if (stopped) return;
        setQrNote(`${e?.message ?? 'QR belum siap'} — mencoba lagi…`);
        qrTimer = setTimeout(fetchQr, 8000);
      }
    };

    const finishActivate = async () => {
      try {
        await activateWaDevice(pairing.token, pairing.phone);
        notify('Device WhatsApp aktif. Notifikasi siap dikirim.', 'success');
      } catch (e: any) {
        notify(e?.message ?? 'Gagal mengaktifkan device.', 'danger');
      } finally {
        setPairing(null);
        setQrUrl(null);
        load();
      }
    };

    const poll = async () => {
      try {
        const r = await checkWaDevice(pairing.token);
        if (!stopped && r.connected) {
          if (pollTimer) clearInterval(pollTimer);
          if (qrTimer) clearTimeout(qrTimer);
          setQrNote('Terhubung! Mengaktifkan device…');
          await finishActivate();
        }
      } catch {
        // abaikan — polling berikutnya
      }
    };

    fetchQr();
    pollTimer = setInterval(poll, 4000);
    return () => {
      stopped = true;
      if (qrTimer) clearTimeout(qrTimer);
      if (pollTimer) clearInterval(pollTimer);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pairing]);

  const handleAddDevice = async () => {
    if (!devName.trim() || !devPhone.trim()) {
      notify('Nama dan nomor device wajib diisi.', 'danger');
      return;
    }
    try {
      const r = await addWaDevice({ name: devName.trim(), phone: devPhone.trim(), autoread: 'off' });
      setPairing({ token: r.deviceToken, phone: r.devicePhone });
      setQrUrl(null);
      setQrNote('Menyiapkan QR…');
    } catch (e: any) {
      notify(e?.message ?? 'Gagal menambah device.', 'danger');
    }
  };

  const handleSendTest = async () => {
    if (!testPhone.trim()) {
      notify('Nomor tujuan tes wajib diisi.', 'danger');
      return;
    }
    try {
      const r = await sendWaTest({
        phone: testPhone.trim(),
        templateId: testTemplateId || undefined,
        body: testTemplateId ? undefined : testBody,
      });
      notify(
        r.status === 'terkirim' ? 'Pesan tes terkirim.' : `Pesan tes gagal (log ${r.logId}).`,
        r.status === 'terkirim' ? 'success' : 'danger',
      );
      load();
    } catch (e: any) {
      notify(e?.message ?? 'Kirim tes gagal.', 'danger');
    }
  };

  const toggleSend = async () => {
    if (!health) return;
    try {
      const s = await updateWaSettings(!health.sendEnabled);
      setHealth({ ...health, sendEnabled: s.sendEnabled });
      notify(
        s.sendEnabled ? 'Notifikasi otomatis WhatsApp AKTIF.' : 'Notifikasi otomatis DIMATIKAN.',
        'success',
      );
    } catch (e: any) {
      notify(e?.message ?? 'Gagal mengubah pengaturan.', 'danger');
    }
  };

  return (
    <div className="space-y-4 p-4">
      <div className="flex flex-wrap items-center gap-2">
        <h1 className="text-lg font-bold">WhatsApp</h1>
        <span className="text-sm text-muted-foreground">
          Gateway self-hosted untuk notifikasi sekolah (order, pengiriman, tagihan).
        </span>
        <Button size="sm" variant="ghost" onClick={load} disabled={busy}>
          Muat ulang
        </Button>
      </div>

      <Card className="p-4">
        <h2 className="mb-2 font-semibold">Status koneksi</h2>
        {!health ? (
          <p className="text-sm text-muted-foreground">Memuat…</p>
        ) : (
          <div className="space-y-2 text-sm">
            <div className="flex flex-wrap items-center gap-2">
              <span>Gateway {health.gatewayUrl}:</span>
              <Badge variant={health.gateway.up ? 'success' : 'danger'}>
                {health.gateway.up ? 'online' : 'offline'}
              </Badge>
              <span>Device aktif:</span>
              <Badge variant={health.activeDevice.connected ? 'success' : 'default'}>
                {health.activeDevice.connected
                  ? `terhubung (${health.activeDevice.devicePhone ?? health.senderNumber ?? ''})`
                  : health.activeDeviceConfigured
                    ? 'belum terhubung'
                    : 'belum di-pairing'}
              </Badge>
              <span>Token akun:</span>
              <Badge variant={health.accountConfigured ? 'success' : 'danger'}>
                {health.accountConfigured ? 'terkonfigurasi' : 'belum diatur'}
              </Badge>
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <span>Notifikasi otomatis (order/tagihan):</span>
              <Badge variant={health.sendEnabled ? 'success' : 'default'}>
                {health.sendEnabled ? 'AKTIF' : 'MATI'}
              </Badge>
              <Button size="sm" variant="ghost" onClick={toggleSend}>
                {health.sendEnabled ? 'Matikan' : 'Aktifkan'}
              </Button>
              {stats && (
                <span className="text-muted-foreground">
                  Hari ini: {stats.sentToday} terkirim · {stats.readToday} dibaca · {stats.failedToday} gagal
                </span>
              )}
            </div>
            <p className="text-muted-foreground">
              Gunakan nomor WhatsApp Business khusus perusahaan. Nyalakan notifikasi otomatis
              hanya setelah device terhubung dan kirim tes berhasil.
            </p>
          </div>
        )}
      </Card>

      <Card className="p-4">
        <h2 className="mb-2 font-semibold">Device WhatsApp</h2>
        {devices.length === 0 ? (
          <p className="mb-3 text-sm text-muted-foreground">Belum ada device terdaftar di gateway.</p>
        ) : (
          <table className="mb-3 w-full text-sm">
            <thead>
              <tr className="border-b text-left text-muted-foreground">
                <th className="py-2 pr-3">Nama</th>
                <th className="py-2 pr-3">Nomor</th>
                <th className="py-2 pr-3">Status</th>
                <th className="py-2">Aksi</th>
              </tr>
            </thead>
            <tbody>
              {devices.map((d) => (
                <tr key={d.token ?? d.device} className="border-b">
                  <td className="py-2 pr-3">{d.name ?? '—'}</td>
                  <td className="py-2 pr-3">{d.device ?? '—'}</td>
                  <td className="py-2 pr-3">
                    <Badge variant={d.status === 'connect' ? 'success' : 'default'}>
                      {d.status === 'connect' ? 'connect' : 'disconnect'}
                    </Badge>{' '}
                    {d.isActive && <Badge variant="info">device aktif</Badge>}
                  </td>
                  <td className="py-2">
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() =>
                        d.device &&
                        confirmAction({
                          message: `Hapus device ${d.device}? Sesi WhatsApp-nya akan diputus.`,
                          onConfirm: async () => {
                            try {
                              await removeWaDevice(d.device!);
                              notify('Device dihapus.', 'success');
                              load();
                            } catch (e: any) {
                              notify(e?.message ?? 'Gagal menghapus device.', 'danger');
                            }
                          },
                        })
                      }
                    >
                      Hapus
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {!pairing ? (
          <div className="flex flex-wrap items-end gap-2">
            <label className="text-sm">
              Nama device
              <Input value={devName} onChange={(e) => setDevName(e.target.value)} className="mt-1 h-9 w-56" />
            </label>
            <label className="text-sm">
              Nomor WhatsApp (62…)
              <Input value={devPhone} onChange={(e) => setDevPhone(e.target.value)} placeholder="62812…" className="mt-1 h-9 w-52" />
            </label>
            <Button size="sm" onClick={handleAddDevice}>
              Tambah & pairing
            </Button>
          </div>
        ) : (
          <div className="flex flex-wrap items-start gap-4">
            {qrUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img src={qrUrl} alt="QR pairing WhatsApp" className="h-56 w-56 rounded border" />
            ) : (
              <div className="flex h-56 w-56 items-center justify-center rounded border text-sm text-muted-foreground">
                Menyiapkan QR…
              </div>
            )}
            <div className="max-w-sm space-y-2 text-sm">
              <p className="font-semibold">Pairing {pairing.phone}</p>
              <p className="text-muted-foreground">{qrNote}</p>
              <p className="text-muted-foreground">
                QR diperbarui otomatis bila kedaluwarsa. Setelah terhubung, device langsung
                diaktifkan sebagai pengirim.
              </p>
              <Button
                size="sm"
                variant="ghost"
                onClick={() => {
                  setPairing(null);
                  setQrUrl(null);
                  load();
                }}
              >
                Batalkan pairing
              </Button>
            </div>
          </div>
        )}
      </Card>

      <Card className="p-4">
        <h2 className="mb-2 font-semibold">Kirim tes</h2>
        <div className="flex flex-wrap items-end gap-2">
          <label className="text-sm">
            Nomor tujuan
            <Input value={testPhone} onChange={(e) => setTestPhone(e.target.value)} placeholder="62812…" className="mt-1 h-9 w-52" />
          </label>
          <label className="text-sm">
            Template (opsional)
            <select
              className="mt-1 block h-9 rounded-md border border-input bg-background px-2 text-sm"
              value={testTemplateId}
              onChange={(e) => setTestTemplateId(e.target.value)}
            >
              <option value="">— pesan manual —</option>
              {templates.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))}
            </select>
          </label>
          {!testTemplateId && (
            <label className="flex-1 text-sm">
              Isi pesan
              <Input value={testBody} onChange={(e) => setTestBody(e.target.value)} className="mt-1 h-9 min-w-64" />
            </label>
          )}
          <Button size="sm" onClick={handleSendTest}>
            Kirim tes
          </Button>
        </div>
      </Card>

      <WhatsappTemplatesSection onChanged={load} />
    </div>
  );
}

'use client';

/**
 * Bagian template & log untuk halaman Pengaturan WhatsApp (Fase 3 W6).
 * Template memakai variabel {{mustache}}; log menampilkan status pengiriman
 * terakhir dengan aksi kirim ulang untuk yang gagal.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { confirmAction, notify } from '@/lib/feedback';
import {
  createWaTemplate,
  listWaLogs,
  listWaTemplates,
  removeWaTemplate,
  resendWaLog,
  updateWaTemplate,
  type WaLog,
  type WaTemplate,
} from '@/lib/api/whatsapp';

const STATUS_BADGE: Record<string, 'info' | 'success' | 'danger' | 'default'> = {
  queued: 'default',
  terkirim: 'info',
  sampai: 'success',
  dibaca: 'success',
  gagal: 'danger',
};

const emptyForm = { name: '', category: 'umum', triggerEvent: '', body: '', isActive: true };

export function WhatsappTemplatesSection({ onChanged }: { onChanged?: () => void }) {
  const [templates, setTemplates] = React.useState<WaTemplate[]>([]);
  const [logs, setLogs] = React.useState<WaLog[]>([]);
  const [form, setForm] = React.useState(emptyForm);
  const [editingId, setEditingId] = React.useState<string | null>(null);

  const load = React.useCallback(async () => {
    try {
      const [t, l] = await Promise.all([listWaTemplates(), listWaLogs()]);
      setTemplates(t);
      setLogs(l);
    } catch (e: any) {
      notify(e?.message ?? 'Gagal memuat template/log WhatsApp.', 'danger');
    }
  }, []);

  React.useEffect(() => {
    load();
  }, [load]);

  const save = async () => {
    if (!form.name.trim() || !form.body.trim()) {
      notify('Nama dan isi template wajib diisi.', 'danger');
      return;
    }
    try {
      if (editingId) {
        await updateWaTemplate(editingId, { ...form, triggerEvent: form.triggerEvent || undefined });
        notify('Template diperbarui.', 'success');
      } else {
        await createWaTemplate({ ...form, triggerEvent: form.triggerEvent || undefined });
        notify('Template dibuat.', 'success');
      }
      setForm(emptyForm);
      setEditingId(null);
      load();
      onChanged?.();
    } catch (e: any) {
      notify(e?.message ?? 'Gagal menyimpan template.', 'danger');
    }
  };

  return (
    <>
      <Card className="p-4">
        <h2 className="mb-2 font-semibold">Template pesan</h2>
        {templates.length > 0 && (
          <table className="mb-3 w-full text-sm">
            <thead>
              <tr className="border-b text-left text-muted-foreground">
                <th className="py-2 pr-3">Nama</th>
                <th className="py-2 pr-3">Kategori / peristiwa</th>
                <th className="py-2 pr-3">Isi</th>
                <th className="py-2 pr-3">Aktif</th>
                <th className="py-2">Aksi</th>
              </tr>
            </thead>
            <tbody>
              {templates.map((t) => (
                <tr key={t.id} className="border-b align-top">
                  <td className="py-2 pr-3 font-medium">{t.name}</td>
                  <td className="py-2 pr-3">
                    {t.category}
                    <div className="text-muted-foreground">{t.triggerEvent ?? '—'}</div>
                  </td>
                  <td className="max-w-md py-2 pr-3 text-muted-foreground">{t.body}</td>
                  <td className="py-2 pr-3">
                    <Badge variant={t.isActive ? 'success' : 'default'}>
                      {t.isActive ? 'aktif' : 'nonaktif'}
                    </Badge>
                  </td>
                  <td className="py-2">
                    <div className="flex gap-1">
                      <Button
                        size="sm"
                        variant="ghost"
                        onClick={() => {
                          setEditingId(t.id);
                          setForm({
                            name: t.name,
                            category: t.category,
                            triggerEvent: t.triggerEvent ?? '',
                            body: t.body,
                            isActive: t.isActive,
                          });
                        }}
                      >
                        Ubah
                      </Button>
                      <Button
                        size="sm"
                        variant="ghost"
                        onClick={() =>
                          confirmAction({
                            message: `Hapus template ${t.name}?`,
                            onConfirm: async () => {
                              try {
                                await removeWaTemplate(t.id);
                                notify('Template dihapus.', 'success');
                                load();
                                onChanged?.();
                              } catch (e: any) {
                                notify(e?.message ?? 'Gagal menghapus template.', 'danger');
                              }
                            },
                          })
                        }
                      >
                        Hapus
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        <div className="grid gap-2 md:grid-cols-2">
          <label className="text-sm">
            Nama template
            <Input
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              placeholder="order_diterima"
              className="mt-1 h-9"
            />
          </label>
          <label className="text-sm">
            Kategori
            <Input
              value={form.category}
              onChange={(e) => setForm({ ...form, category: e.target.value })}
              placeholder="pesanan / pengiriman / tagihan / umum"
              className="mt-1 h-9"
            />
          </label>
          <label className="text-sm md:col-span-2">
            Isi pesan — variabel: {'{{sekolah}}'}, {'{{nomor_order}}'}, {'{{total}}'}, {'{{nomor_do}}'},{' '}
            {'{{nomor_invoice}}'}, {'{{jatuh_tempo}}'}
            <textarea
              value={form.body}
              onChange={(e) => setForm({ ...form, body: e.target.value })}
              rows={3}
              className="mt-1 w-full rounded-md border border-input bg-background px-2 py-1 text-sm"
            />
          </label>
        </div>
        <div className="mt-2 flex items-center gap-2">
          <label className="flex items-center gap-1 text-sm">
            <input
              type="checkbox"
              checked={form.isActive}
              onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
            />
            Aktif
          </label>
          <Button size="sm" onClick={save}>
            {editingId ? 'Simpan perubahan' : 'Tambah template'}
          </Button>
          {editingId && (
            <Button
              size="sm"
              variant="ghost"
              onClick={() => {
                setEditingId(null);
                setForm(emptyForm);
              }}
            >
              Batal ubah
            </Button>
          )}
        </div>
      </Card>

      <Card className="p-4">
        <h2 className="mb-2 font-semibold">Log pengiriman</h2>
        {logs.length === 0 ? (
          <p className="text-sm text-muted-foreground">Belum ada pengiriman tercatat.</p>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-muted-foreground">
                <th className="py-2 pr-3">Waktu</th>
                <th className="py-2 pr-3">Tujuan</th>
                <th className="py-2 pr-3">Pesan</th>
                <th className="py-2 pr-3">Status</th>
                <th className="py-2">Aksi</th>
              </tr>
            </thead>
            <tbody>
              {logs.map((l) => (
                <tr key={l.id} className="border-b align-top">
                  <td className="py-2 pr-3 whitespace-nowrap">
                    {new Date(l.createdAt).toLocaleString('id-ID', {
                      day: 'numeric',
                      month: 'short',
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </td>
                  <td className="py-2 pr-3">
                    {l.recipientPhone}
                    <div className="text-muted-foreground">{l.templateName ?? l.recipientType}</div>
                  </td>
                  <td className="max-w-md py-2 pr-3 text-muted-foreground">
                    {l.body.length > 140 ? `${l.body.slice(0, 140)}…` : l.body}
                    {l.errorReason && <div className="text-destructive">{l.errorReason}</div>}
                  </td>
                  <td className="py-2 pr-3">
                    <Badge variant={STATUS_BADGE[l.status] ?? 'default'}>{l.status}</Badge>
                  </td>
                  <td className="py-2">
                    {l.status === 'gagal' && (
                      <Button
                        size="sm"
                        variant="ghost"
                        onClick={async () => {
                          try {
                            const r = await resendWaLog(l.id);
                            notify(
                              r.status === 'terkirim' ? 'Kirim ulang berhasil.' : 'Kirim ulang gagal lagi.',
                              r.status === 'terkirim' ? 'success' : 'danger',
                            );
                            load();
                          } catch (e: any) {
                            notify(e?.message ?? 'Kirim ulang gagal.', 'danger');
                          }
                        }}
                      >
                        Kirim ulang
                      </Button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Card>
    </>
  );
}

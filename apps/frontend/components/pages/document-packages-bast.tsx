'use client';

/** A3 — Seksi penerimaan BAST: daftar laporan penerimaan, pencatatan
 * penerimaan (nama/jabatan/tanggal), unggah foto serah terima, dan
 * pintasan buat BAST untuk order terkait. */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { notify } from '@/lib/feedback';
import { uploadTransactionAttachment } from '@/lib/api/transaction-attachments';
import {
  listDocDeliveryReports,
  recordAcceptance,
  type DocDeliveryReport,
} from '@/lib/api/document-packages';
import { DocModal, fmtDate } from './document-packages-shared';

export function DocumentPackagesBast({ onGenerateBast }: { onGenerateBast: (orderId: string) => void }) {
  const [drFilter, setDrFilter] = React.useState('pending');
  const [drs, setDrs] = React.useState<DocDeliveryReport[]>([]);
  const [acceptTarget, setAcceptTarget] = React.useState<DocDeliveryReport | null>(null);
  const [accName, setAccName] = React.useState('');
  const [accTitle, setAccTitle] = React.useState('');
  const [accNotes, setAccNotes] = React.useState('');

  const loadDrs = React.useCallback(async () => {
    try {
      setDrs(await listDocDeliveryReports(drFilter || undefined));
    } catch {
      setDrs([]);
    }
  }, [drFilter]);
  React.useEffect(() => {
    void loadDrs();
  }, [loadDrs]);

  const runAccept = async () => {
    if (!acceptTarget || !accName.trim()) return;
    try {
      await recordAcceptance(acceptTarget.id, {
        acceptedByName: accName.trim(),
        acceptedByTitle: accTitle || undefined,
        notes: accNotes || undefined,
      });
      notify('Penerimaan BAST tercatat — status order menjadi Diterima', 'success');
      setAcceptTarget(null);
      setAccName('');
      setAccTitle('');
      setAccNotes('');
      void loadDrs();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'Gagal mencatat penerimaan', 'danger');
    }
  };

  const uploadPhoto = async (dr: DocDeliveryReport, file: File | null) => {
    if (!file) return;
    try {
      await uploadTransactionAttachment('sls', 'DR', dr.id, file, 'Foto serah terima (BAST)');
      notify('Foto serah terima terunggah', 'success');
    } catch {
      notify('Gagal mengunggah foto', 'danger');
    }
  };

  return (
    <Card>
      <div className="space-y-3 px-4 py-4">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div className="font-semibold">Penerimaan Barang (BAST)</div>
          <select className="rounded-md border px-2 py-1.5 text-sm" value={drFilter} onChange={(e) => setDrFilter(e.target.value)}>
            <option value="pending">Belum diterima</option>
            <option value="done">Sudah diterima</option>
            <option value="">Semua</option>
          </select>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-slate-500">
                <th className="py-2 pr-2">Laporan Penerimaan</th>
                <th className="py-2 pr-2">Sekolah</th>
                <th className="py-2 pr-2">Tanggal</th>
                <th className="py-2 pr-2">Penerimaan</th>
                <th className="py-2">Aksi</th>
              </tr>
            </thead>
            <tbody>
              {drs.map((dr) => (
                <tr key={dr.id} className="border-b">
                  <td className="py-2 pr-2 font-medium">{dr.docNumber}</td>
                  <td className="py-2 pr-2">{dr.customer?.name ?? '-'}</td>
                  <td className="py-2 pr-2">{fmtDate(dr.docDate)}</td>
                  <td className="py-2 pr-2">
                    {dr.acceptedAt ? (
                      <span>{dr.acceptedByName} · {fmtDate(dr.acceptedAt)}</span>
                    ) : (
                      <Badge variant="default">Belum diterima</Badge>
                    )}
                  </td>
                  <td className="py-2">
                    <div className="flex flex-wrap gap-1">
                      {!dr.acceptedAt && (
                        <Button variant="ghost" onClick={() => setAcceptTarget(dr)}>Catat Penerimaan</Button>
                      )}
                      <label className="cursor-pointer rounded-md px-2 py-1.5 text-sm hover:bg-slate-100">
                        Unggah Foto
                        <input type="file" accept="image/*" className="hidden" onChange={(e) => void uploadPhoto(dr, e.target.files?.[0] ?? null)} />
                      </label>
                      {dr.deliveryOrder?.orderId && (
                        <Button variant="ghost" onClick={() => onGenerateBast(dr.deliveryOrder!.orderId!)}>
                          Buat BAST
                        </Button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
              {drs.length === 0 && (
                <tr><td colSpan={5} className="py-4 text-center text-slate-400">Tidak ada laporan penerimaan.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {acceptTarget && (
        <DocModal title={`Catat Penerimaan — ${acceptTarget.docNumber}`} onClose={() => setAcceptTarget(null)}>
          <Input placeholder="Nama penerima di sekolah" value={accName} onChange={(e) => setAccName(e.target.value)} />
          <Input placeholder="Jabatan (Kepala Sekolah/Bendahara/…)" value={accTitle} onChange={(e) => setAccTitle(e.target.value)} />
          <Input placeholder="Catatan penerimaan (opsional)" value={accNotes} onChange={(e) => setAccNotes(e.target.value)} />
          <Button variant="primary" disabled={!accName.trim()} onClick={() => void runAccept()}>Simpan Penerimaan</Button>
        </DocModal>
      )}
    </Card>
  );
}

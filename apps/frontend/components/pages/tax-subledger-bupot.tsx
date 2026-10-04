'use client';

/** A4 — Tab Bukti Potong: ekspektasi potongan PPh 22/23 oleh bendahara
 * sekolah vs bukti potong yang diterima (rekonsiliasi), pencatatan
 * potongan & bukti potong, unggah berkas bukti, pembatalan. */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { notify } from '@/lib/feedback';
import { formatRupiah } from '@/lib/format';
import { listPartners, type ErpPartner } from '@/lib/api/partners';
import { listTaxes, type ErpTax } from '@/lib/api/taxes';
import { uploadTransactionAttachment } from '@/lib/api/transaction-attachments';
import {
  cancelCertificate,
  createCertificate,
  createTaxEntry,
  getWithholding,
  listCertificates,
  type WhtCertificate,
  type WithholdingRow,
} from '@/lib/api/tax-subledger';
import { DocModal, fmtDate } from './document-packages-shared';

const rp = (v: string | number) => formatRupiah(Number(v) || 0);

function PartnerPicker({ value, onPick }: { value: ErpPartner | null; onPick: (p: ErpPartner | null) => void }) {
  const [q, setQ] = React.useState('');
  const [results, setResults] = React.useState<ErpPartner[]>([]);
  React.useEffect(() => {
    if (!q.trim() || value) {
      setResults([]);
      return;
    }
    const t = setTimeout(async () => {
      try {
        const res = await listPartners({ search: q.trim(), limit: 8 });
        setResults(res.data);
      } catch {
        setResults([]);
      }
    }, 300);
    return () => clearTimeout(t);
  }, [q, value]);
  if (value) {
    return (
      <div className="flex items-center gap-2 text-sm">
        <span className="font-medium">{value.name}</span>
        <Button variant="ghost" onClick={() => onPick(null)}>Ganti</Button>
      </div>
    );
  }
  return (
    <div className="relative">
      <Input placeholder="Cari sekolah / partner…" value={q} onChange={(e) => setQ(e.target.value)} />
      {results.length > 0 && (
        <div className="absolute z-10 mt-1 w-full rounded-md border bg-white shadow">
          {results.map((p) => (
            <button key={p.id} className="block w-full px-3 py-2 text-left text-sm hover:bg-slate-50" onClick={() => onPick(p)}>
              <span className="font-medium">{p.code}</span> — {p.name}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

function reconBadge(r: WithholdingRow) {
  if (r.reconcileStatus === 'LENGKAP') return <Badge variant="success">Lengkap</Badge>;
  if (r.reconcileStatus === 'BELUM_ADA_BUKTI') return <Badge variant="warn">Belum ada bukti</Badge>;
  if (r.reconcileStatus === 'KURANG') return <Badge variant="danger">Kurang</Badge>;
  return <Badge variant="info">Lebih</Badge>;
}

export function TaxSubledgerBupot() {
  const [rows, setRows] = React.useState<WithholdingRow[]>([]);
  const [certs, setCerts] = React.useState<WhtCertificate[]>([]);
  const [taxes, setTaxes] = React.useState<ErpTax[]>([]);
  const [showEntry, setShowEntry] = React.useState(false);
  const [showCert, setShowCert] = React.useState(false);

  const reload = React.useCallback(async () => {
    try {
      const [w, c] = await Promise.all([getWithholding(), listCertificates()]);
      setRows(w);
      setCerts(c);
    } catch {
      notify('Gagal memuat data bukti potong', 'danger');
    }
  }, []);
  React.useEffect(() => {
    void reload();
    listTaxes({ limit: 200 }).then((r) => setTaxes(r.data)).catch(() => setTaxes([]));
  }, [reload]);

  // form state — entri potongan
  const [ePartner, setEPartner] = React.useState<ErpPartner | null>(null);
  const [eDoc, setEDoc] = React.useState('');
  const [eDate, setEDate] = React.useState('');
  const [eTaxId, setETaxId] = React.useState('');
  const [eType, setEType] = React.useState('PPH_22');
  const [eDpp, setEDpp] = React.useState('');
  const [eAmount, setEAmount] = React.useState('');

  const submitEntry = async () => {
    try {
      await createTaxEntry({
        docNumber: eDoc, transactionDate: eDate, taxId: eTaxId, taxEntryType: eType,
        dpp: eDpp, taxAmount: eAmount, partnerId: ePartner?.id, module: 'FIN',
      });
      notify('Ekspektasi potongan tercatat', 'success');
      setShowEntry(false);
      void reload();
    } catch (err) {
      notify(err instanceof Error ? err.message : 'Gagal mencatat potongan', 'danger');
    }
  };

  // form state — bukti potong
  const [cPartner, setCPartner] = React.useState<ErpPartner | null>(null);
  const [cNumber, setCNumber] = React.useState('');
  const [cType, setCType] = React.useState('PPH_22');
  const [cDate, setCDate] = React.useState('');
  const [cDpp, setCDpp] = React.useState('');
  const [cRate, setCRate] = React.useState('');
  const [cAmount, setCAmount] = React.useState('');
  const [cEntryId, setCEntryId] = React.useState('');

  const submitCert = async () => {
    try {
      await createCertificate({
        certNumber: cNumber, pphType: cType, transactionDate: cDate, partnerId: cPartner?.id,
        dpp: cDpp, rate: cRate, amountWithheld: cAmount, taxEntryId: cEntryId || undefined,
      });
      notify('Bukti potong tercatat', 'success');
      setShowCert(false);
      void reload();
    } catch (err) {
      notify(err instanceof Error ? err.message : 'Gagal mencatat bukti potong', 'danger');
    }
  };

  const pphTaxes = taxes.filter((t) => t.code.startsWith('PPH22') || t.code.startsWith('PPH23'));

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="text-sm text-slate-500">
          Potongan PPh 22/23 oleh bendahara sekolah: bandingkan nilai yang dipotong dengan bukti potong yang diterima.
        </div>
        <div className="flex gap-2">
          <Button variant="ghost" onClick={() => setShowEntry(true)}>Catat Potongan</Button>
          <Button variant="primary" onClick={() => setShowCert(true)}>Tambah Bukti Potong</Button>
        </div>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b text-left text-slate-500">
              <th className="py-2 pr-2">Dokumen</th>
              <th className="py-2 pr-2">Sekolah</th>
              <th className="py-2 pr-2">Jenis</th>
              <th className="py-2 pr-2">Diharapkan</th>
              <th className="py-2 pr-2">Bukti Diterima</th>
              <th className="py-2 pr-2">Selisih</th>
              <th className="py-2">Rekonsiliasi</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => (
              <tr key={r.id} className="border-b">
                <td className="py-2 pr-2 font-medium">{r.docNumber}<div className="text-xs text-slate-400">{fmtDate(r.transactionDate)}</div></td>
                <td className="py-2 pr-2">{r.partnerName ?? '-'}</td>
                <td className="py-2 pr-2">{r.taxEntryType.replace('_', ' ')}</td>
                <td className="py-2 pr-2">{rp(r.taxAmount)}</td>
                <td className="py-2 pr-2">{rp(r.certifiedTotal)}</td>
                <td className="py-2 pr-2">{rp(r.diff)}</td>
                <td className="py-2">{reconBadge(r)}</td>
              </tr>
            ))}
            {rows.length === 0 && (
              <tr><td colSpan={7} className="py-4 text-center text-slate-400">Belum ada catatan potongan PPh.</td></tr>
            )}
          </tbody>
        </table>
      </div>

      <div>
        <div className="mb-2 font-semibold">Bukti Potong Diterima</div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-slate-500">
                <th className="py-2 pr-2">Nomor Bukti</th>
                <th className="py-2 pr-2">Sekolah</th>
                <th className="py-2 pr-2">Tanggal</th>
                <th className="py-2 pr-2">DPP</th>
                <th className="py-2 pr-2">Tarif</th>
                <th className="py-2 pr-2">Dipotong</th>
                <th className="py-2 pr-2">Status</th>
                <th className="py-2">Aksi</th>
              </tr>
            </thead>
            <tbody>
              {certs.map((c) => (
                <tr key={c.id} className="border-b">
                  <td className="py-2 pr-2 font-medium">{c.certNumber}</td>
                  <td className="py-2 pr-2">{c.partnerName}</td>
                  <td className="py-2 pr-2">{fmtDate(c.transactionDate)}</td>
                  <td className="py-2 pr-2">{rp(c.dpp)}</td>
                  <td className="py-2 pr-2">{c.rate}%</td>
                  <td className="py-2 pr-2">{rp(c.amountWithheld)}</td>
                  <td className="py-2 pr-2">{c.status === 'ISSUED' ? <Badge variant="success">Diterima</Badge> : <Badge variant="default">Dibatalkan</Badge>}</td>
                  <td className="py-2">
                    <div className="flex gap-1">
                      <label className="cursor-pointer rounded-md px-2 py-1.5 hover:bg-slate-100">
                        Unggah
                        <input
                          type="file"
                          className="hidden"
                          onChange={async (e) => {
                            const f = e.target.files?.[0];
                            if (!f) return;
                            try {
                              await uploadTransactionAttachment('fin', 'WHT', c.id, f, 'Bukti potong bendahara');
                              notify('Berkas bukti potong terunggah', 'success');
                            } catch {
                              notify('Gagal mengunggah berkas', 'danger');
                            }
                          }}
                        />
                      </label>
                      {c.status === 'ISSUED' && (
                        <Button
                          variant="ghost"
                          onClick={async () => {
                            await cancelCertificate(c.id);
                            notify('Bukti potong dibatalkan', 'success');
                            void reload();
                          }}
                        >
                          Batalkan
                        </Button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
              {certs.length === 0 && (
                <tr><td colSpan={8} className="py-4 text-center text-slate-400">Belum ada bukti potong diterima.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {showEntry && (
        <DocModal title="Catat Potongan Bendahara (Ekspektasi)" onClose={() => setShowEntry(false)}>
          <PartnerPicker value={ePartner} onPick={setEPartner} />
          <Input placeholder="Nomor dokumen (invoice)" value={eDoc} onChange={(e) => setEDoc(e.target.value)} />
          <Input type="date" value={eDate} onChange={(e) => setEDate(e.target.value)} />
          <div className="flex gap-2">
            <select className="rounded-md border px-2 py-1.5 text-sm" value={eType} onChange={(e) => setEType(e.target.value)}>
              <option value="PPH_22">PPh 22</option>
              <option value="PPH_23">PPh 23</option>
            </select>
            <select className="flex-1 rounded-md border px-2 py-1.5 text-sm" value={eTaxId} onChange={(e) => setETaxId(e.target.value)}>
              <option value="">Pilih tarif pajak…</option>
              {pphTaxes.map((t) => (
                <option key={t.id} value={t.id}>{t.code} — {t.rate}%</option>
              ))}
            </select>
          </div>
          <Input placeholder="DPP (dasar pengenaan)" value={eDpp} onChange={(e) => setEDpp(e.target.value)} />
          <Input placeholder="Jumlah dipotong" value={eAmount} onChange={(e) => setEAmount(e.target.value)} />
          <Button variant="primary" disabled={!ePartner || !eDoc || !eDate || !eTaxId || !eDpp || !eAmount} onClick={() => void submitEntry()}>
            Simpan Potongan
          </Button>
        </DocModal>
      )}

      {showCert && (
        <DocModal title="Tambah Bukti Potong" onClose={() => setShowCert(false)}>
          <PartnerPicker value={cPartner} onPick={setCPartner} />
          <Input placeholder="Nomor bukti potong" value={cNumber} onChange={(e) => setCNumber(e.target.value)} />
          <div className="flex gap-2">
            <select className="rounded-md border px-2 py-1.5 text-sm" value={cType} onChange={(e) => setCType(e.target.value)}>
              <option value="PPH_22">PPh 22</option>
              <option value="PPH_23">PPh 23</option>
            </select>
            <Input type="date" value={cDate} onChange={(e) => setCDate(e.target.value)} />
          </div>
          <select className="w-full rounded-md border px-2 py-1.5 text-sm" value={cEntryId} onChange={(e) => setCEntryId(e.target.value)}>
            <option value="">Tautkan ke catatan potongan (opsional)…</option>
            {rows.map((r) => (
              <option key={r.id} value={r.id}>{r.docNumber} — {r.partnerName ?? ''} — {rp(r.taxAmount)}</option>
            ))}
          </select>
          <Input placeholder="DPP" value={cDpp} onChange={(e) => setCDpp(e.target.value)} />
          <div className="flex gap-2">
            <Input placeholder="Tarif %" value={cRate} onChange={(e) => setCRate(e.target.value)} />
            <Input placeholder="Jumlah dipotong" value={cAmount} onChange={(e) => setCAmount(e.target.value)} />
          </div>
          <Button variant="primary" disabled={!cPartner || !cNumber || !cDate || !cDpp || !cRate || !cAmount} onClick={() => void submitCert()}>
            Simpan Bukti Potong
          </Button>
        </DocModal>
      )}
    </div>
  );
}

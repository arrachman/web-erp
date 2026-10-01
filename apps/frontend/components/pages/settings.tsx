'use client';

import * as React from 'react';
import { Icon, type IconName } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { CABANGS, type Translator } from '@/lib/mock';

interface SettingsPageProps {
  t: Translator;
}

interface ToggleProps {
  on: boolean;
  onChange: (next: boolean) => void;
}

function Toggle({ on, onChange }: ToggleProps) {
  return (
    <button
      type="button"
      onClick={() => onChange(!on)}
      style={{
        width: 34,
        height: 20,
        borderRadius: 10,
        border: '1px solid var(--border)',
        background: on ? 'var(--primary)' : 'var(--panel-2)',
        position: 'relative',
        cursor: 'pointer',
        padding: 0,
        transition: 'background 120ms',
        flexShrink: 0,
      }}
    >
      <span
        style={{
          position: 'absolute',
          top: 1,
          left: on ? 15 : 1,
          width: 16,
          height: 16,
          borderRadius: '50%',
          background: '#fff',
          transition: 'left 120ms',
          boxShadow: '0 1px 2px rgba(0,0,0,0.25)',
        }}
      />
    </button>
  );
}

interface SetRowProps {
  label: string;
  hint?: string;
  children: React.ReactNode;
}

function SetRow({ label, hint, children }: SetRowProps) {
  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: '200px 1fr',
        gap: 16,
        alignItems: 'center',
        padding: '10px 0',
        borderTop: '1px solid var(--border)',
      }}
    >
      <div>
        <div style={{ fontSize: 'calc(12.5px * var(--font-scale, 1))' }}>{label}</div>
        {hint && (
          <div className="muted" style={{ fontSize: 'calc(11px * var(--font-scale, 1))', marginTop: 2 }}>
            {hint}
          </div>
        )}
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
        {children}
      </div>
    </div>
  );
}

interface SetCardProps {
  icon: IconName;
  title: string;
  sub?: string;
  children: React.ReactNode;
}

function SetCard({ icon, title, sub, children }: SetCardProps) {
  return (
    <div className="card" style={{ gridColumn: 'span 6' }}>
      <div className="card-h">
        <span
          style={{
            display: 'inline-flex',
            width: 24,
            height: 24,
            alignItems: 'center',
            justifyContent: 'center',
            background: 'var(--primary-soft)',
            color: 'var(--primary-soft-fg)',
            borderRadius: 5,
          }}
        >
          <Icon name={icon} size={13} />
        </span>
        <div>
          <div className="title">{title}</div>
          {sub && (
            <div className="sub" style={{ marginTop: 1 }}>
              {sub}
            </div>
          )}
        </div>
      </div>
      <div className="card-b" style={{ paddingTop: 2 }}>
        {children}
      </div>
    </div>
  );
}

const inputStyle: React.CSSProperties = {
  height: 28,
  padding: '0 8px',
  background: 'var(--panel)',
  border: '1px solid var(--border)',
  borderRadius: 'var(--radius)',
  color: 'var(--fg)',
  font: 'inherit',
  fontSize: 'calc(12.5px * var(--font-scale, 1))',
  outline: 'none',
  minWidth: 200,
};

interface SelProps {
  value: string;
  opts: readonly string[];
  onChange: React.ChangeEventHandler<HTMLSelectElement>;
}

function Sel({ value, opts, onChange }: SelProps) {
  return (
    <select style={inputStyle} value={value} onChange={onChange}>
      {opts.map((o) => (
        <option key={o}>{o}</option>
      ))}
    </select>
  );
}

interface SettingsState {
  company: string;
  branch: string;
  lang: string;
  tz: string;
  dateFmt: string;
  fyStart: string;
  invMethod: string;
  costMethod: string;
  baseCur: string;
  rounding: string;
  prefCR: string;
  prefCD: string;
  prefPO: string;
  prefSI: string;
  notifApprove: boolean;
  notifPost: boolean;
  notifLowStock: boolean;
  notifEmail: boolean;
  autoBackup: boolean;
}

type TextKey =
  | 'company'
  | 'branch'
  | 'lang'
  | 'tz'
  | 'dateFmt'
  | 'fyStart'
  | 'invMethod'
  | 'costMethod'
  | 'baseCur'
  | 'rounding'
  | 'prefCR'
  | 'prefCD'
  | 'prefPO'
  | 'prefSI';

type BoolKey =
  | 'notifApprove'
  | 'notifPost'
  | 'notifLowStock'
  | 'notifEmail'
  | 'autoBackup';

/** Setting → Preferensi : application preferences form — ported from prototype `pages/settings.jsx`. */
export function SettingsPage({ t }: SettingsPageProps) {
  const [s, setS] = React.useState<SettingsState>({
    company: 'PT Sentient Manufaktur Indonesia',
    branch: 'PCI',
    lang: 'Indonesia',
    tz: '(GMT+7) Jakarta',
    dateFmt: 'DD/MM/YYYY',
    fyStart: 'Januari',
    invMethod: 'FIFO',
    costMethod: 'AVG',
    baseCur: 'IDR',
    rounding: '0 desimal',
    prefCR: 'CR',
    prefCD: 'CD',
    prefPO: 'PO',
    prefSI: 'SI',
    notifApprove: true,
    notifPost: true,
    notifLowStock: true,
    notifEmail: false,
    autoBackup: true,
  });

  const setText =
    (k: TextKey) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
      setS((prev) => ({ ...prev, [k]: e.target.value }));
  const setBool = (k: BoolKey) => (next: boolean) =>
    setS((prev) => ({ ...prev, [k]: next }));


  return (
    <div className="page">
      <div className="page-header">
        <h1 className="page-title">
          {t('Preferensi')}
          <span className="code-tag">PR</span>
        </h1>
        <div className="page-actions">
          <button className="btn ghost">
            <Icon name="refresh" size={12} /> {t('Reset')}
          </button>
          <button className="btn primary">
            <Icon name="save" size={12} /> {t('Simpan')} <Kbd>⌘S</Kbd>
          </button>
        </div>
      </div>

      <div
        className="dash-grid scrollbar"
        style={{ overflow: 'auto', flex: 1, alignContent: 'start' }}
      >
        <SetCard icon="info" title="Umum" sub="Identitas & lokal">
          <SetRow label="Nama Perusahaan">
            <input
              style={{ ...inputStyle, minWidth: 320 }}
              value={s.company}
              onChange={setText('company')}
            />
          </SetRow>
          <SetRow label="Cabang Default">
            <Sel value={s.branch} opts={CABANGS} onChange={setText('branch')} />
          </SetRow>
          <SetRow label="Bahasa">
            <Sel value={s.lang} opts={['Indonesia', 'English']} onChange={setText('lang')} />
          </SetRow>
          <SetRow label="Zona Waktu">
            <Sel value={s.tz} opts={['(GMT+7) Jakarta', '(GMT+8) Makassar', '(GMT+9) Jayapura']} onChange={setText('tz')} />
          </SetRow>
          <SetRow label="Format Tanggal">
            <Sel value={s.dateFmt} opts={['DD/MM/YYYY', 'YYYY-MM-DD', 'DD MMM YYYY']} onChange={setText('dateFmt')} />
          </SetRow>
        </SetCard>

        <SetCard icon="coins" title="Akuntansi" sub="Kebijakan pembukuan">
          <SetRow label="Awal Tahun Buku">
            <Sel value={s.fyStart} opts={['Januari', 'April', 'Juli', 'Oktober']} onChange={setText('fyStart')} />
          </SetRow>
          <SetRow label="Metode Persediaan" hint="Berlaku untuk valuasi stok">
            <Sel value={s.invMethod} opts={['FIFO', 'Average', 'LIFO']} onChange={setText('invMethod')} />
          </SetRow>
          <SetRow label="Metode HPP" hint="Rumus penghitungan harga pokok saat keluar stok">
            <Sel value={s.costMethod} opts={['AVG', 'FIFO', 'STD']} onChange={setText('costMethod')} />
          </SetRow>
          <SetRow label="Mata Uang Dasar">
            <Sel value={s.baseCur} opts={['IDR', 'USD', 'SGD']} onChange={setText('baseCur')} />
          </SetRow>
          <SetRow label="Pembulatan Nilai">
            <Sel value={s.rounding} opts={['0 desimal', '2 desimal', 'Bulat ribuan']} onChange={setText('rounding')} />
          </SetRow>
        </SetCard>

        <SetCard
          icon="receipt"
          title="Penomoran Dokumen"
          sub="Prefix nomor transaksi"
        >
          <SetRow label="Kas Masuk">
            <input
              style={inputStyle}
              value={s.prefCR}
              onChange={setText('prefCR')}
            />
            <span
              className="muted"
              style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))', fontFamily: 'Geist Mono, monospace' }}
            >
              → {s.prefCR}-2605-2400
            </span>
          </SetRow>
          <SetRow label="Kas Keluar">
            <input
              style={inputStyle}
              value={s.prefCD}
              onChange={setText('prefCD')}
            />
            <span
              className="muted"
              style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))', fontFamily: 'Geist Mono, monospace' }}
            >
              → {s.prefCD}-2605-1640
            </span>
          </SetRow>
          <SetRow label="PO Pembelian">
            <input
              style={inputStyle}
              value={s.prefPO}
              onChange={setText('prefPO')}
            />
            <span
              className="muted"
              style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))', fontFamily: 'Geist Mono, monospace' }}
            >
              → {s.prefPO}-2605-0312
            </span>
          </SetRow>
          <SetRow label="Faktur Penjualan">
            <input
              style={inputStyle}
              value={s.prefSI}
              onChange={setText('prefSI')}
            />
            <span
              className="muted"
              style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))', fontFamily: 'Geist Mono, monospace' }}
            >
              → {s.prefSI}-2605-0918
            </span>
          </SetRow>
        </SetCard>

        <SetCard
          icon="bell"
          title="Notifikasi & Sistem"
          sub="Pemberitahuan otomatis"
        >
          <SetRow
            label="Butuh Approval"
            hint="Notif saat dokumen perlu disetujui"
          >
            <Toggle on={s.notifApprove} onChange={setBool('notifApprove')} />
          </SetRow>
          <SetRow label="Dokumen Diposting">
            <Toggle on={s.notifPost} onChange={setBool('notifPost')} />
          </SetRow>
          <SetRow label="Stok Menipis" hint="Alert saat stok di bawah minimum">
            <Toggle on={s.notifLowStock} onChange={setBool('notifLowStock')} />
          </SetRow>
          <SetRow label="Kirim ke Email">
            <Toggle on={s.notifEmail} onChange={setBool('notifEmail')} />
          </SetRow>
          <SetRow label="Auto Backup Harian" hint="Backup DB tiap 02:00 WIB">
            <Toggle on={s.autoBackup} onChange={setBool('autoBackup')} />
          </SetRow>
        </SetCard>
      </div>

      <div className="pager">
        <span className="muted">
          Setting · {t('Preferensi')}
        </span>
        <div className="spacer" />
        <span className="muted">
          Perubahan berlaku untuk semua user di cabang {s.branch}
        </span>
      </div>
    </div>
  );
}

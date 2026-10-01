'use client';

/**
 * TrxForm — generic "Tambah" page for every Keuangan transaksi
 * (kas/bank/giro/jurnal/saldo awal). Ported from prototype
 * `pages/trx-form.jsx` with a multi-file split so every module stays
 * under the 400-line cap:
 *
 *   - `trx-form-config.ts`   TRX_CFG + line/column types
 *   - `trx-form-header.tsx`  back/title/draft pill + action cluster
 *   - `trx-form-fields.tsx`  left + middle form columns
 *   - `trx-form-summary.tsx` right column (status + ringkasan card)
 *   - `trx-form-tabs.tsx`    tab bar + Info/Audit panels + footer
 *   - `trx-form-lines.tsx`   editable detail grid
 *   - `trx-form.tsx`         this file — state, callbacks, layout
 *
 * Strict TS, no `window.*` globals, `useTabKey` so keyboard shortcuts
 * auto-detach when the host tab is inactive.
 */

import * as React from 'react';
import { type Translator } from '@/lib/mock';
import { notify } from '@/lib/feedback';
import { useTabKey } from '@/lib/tab-context';
import {
  ContactPicker,
  type Contact,
} from '@/components/organisms/contact-picker';
import {
  LookupModal,
  type LookupKind,
  type LookupRow,
} from '@/components/organisms/lookup-modal';
import {
  resolveTrxConfig,
  type TrxConfig,
  type TrxLine,
} from './trx-form-config';
import { TrxLines, type TrxEditing } from './trx-form-lines';
import { TrxFormHeader } from './trx-form-header';
import {
  TrxFormFieldsLeft,
  TrxFormFieldsMid,
  type TrxHeadState,
  type TrxLookupTarget,
} from './trx-form-fields';
import { TrxFormSummary } from './trx-form-summary';
import {
  TrxAuditPanel,
  TrxFooterShortcuts,
  TrxFormTabs,
  TrxInfoPanel,
  type TrxTabKey,
} from './trx-form-tabs';

export interface TrxFormProps {
  moduleId: string;
  t: Translator;
  lang: 'id' | 'en' | 'ja';
  onNavigate: (route: string) => void;
}

type LookupState = { kind: LookupKind; target: TrxLookupTarget } | null;

const TODAY_STR = '12/05/2026';

function seedLines(cfg: TrxConfig): TrxLine[] {
  if (cfg.journal) {
    return [
      {
        id: 1,
        no: '110101.101',
        nama: 'Kas Besar',
        debit: 5000000,
        kredit: 0,
        catatan: 'Koreksi saldo',
        cc: 'ADM',
      },
      {
        id: 2,
        no: '410101.101',
        nama: 'Pendapatan Lain-lain',
        debit: 0,
        kredit: 5000000,
        catatan: '',
        cc: 'ADM',
      },
    ];
  }
  return [
    {
      id: 1,
      no: '410101.101',
      nama: 'Penjualan Barang Jadi',
      total: 4250000,
      catatan: 'INV-2026-1024',
      cc: 'PCI-SALES',
      proyek: '—',
    },
    {
      id: 2,
      no: '210105.001',
      nama: 'PPN Keluaran',
      total: 467500,
      catatan: '',
      cc: 'PCI-SALES',
      proyek: '—',
    },
  ];
}

function initialHead(cfg: TrxConfig): TrxHeadState {
  return {
    party: '',
    akun: cfg.acctVal ?? '',
    uraian: '',
    lokasi: 'T - FG',
    tanggal: TODAY_STR,
    auto: true,
    uang: 'IDR',
    kurs: '1,00',
    status: 'Need Approve',
    noGiro: '',
    jatuhTempo: '20/06/2026',
  };
}

export function TrxForm({
  moduleId,
  t,
  onNavigate,
}: TrxFormProps): React.ReactElement {
  const cfg = React.useMemo(() => resolveTrxConfig(moduleId), [moduleId]);
  const backTo = moduleId;

  const [head, setHead] = React.useState<TrxHeadState>(() => initialHead(cfg));
  const [tab, setTab] = React.useState<TrxTabKey>('detail');
  const [lines, setLines] = React.useState<TrxLine[]>(() => seedLines(cfg));
  const [editing, setEditing] = React.useState<TrxEditing>(null);
  const [coaQuery, setCoaQuery] = React.useState('');
  const [pickerOpen, setPickerOpen] = React.useState(false);
  const [lookup, setLookup] = React.useState<LookupState>(null);
  // Selected contact is kept for future preview hooks (legacy prototype
  // showed `${contact.code} · ${contact.cat}` under the field). Wired but
  // unread for now.
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [_contact, setContact] = React.useState<Contact | null>(null);

  const total = lines.reduce(
    (s, l) =>
      s + (cfg.journal ? Number(l.debit ?? 0) : Number(l.total ?? 0)),
    0,
  );
  const totalKredit = cfg.journal
    ? lines.reduce((s, l) => s + Number(l.kredit ?? 0), 0)
    : 0;
  const balanced = !cfg.journal || total === totalKredit;
  const headerErr = cfg.party ? !head.party.trim() : false;

  const addLine = React.useCallback((): void => {
    const id = (lines[lines.length - 1]?.id ?? 0) + 1;
    const blank: TrxLine = cfg.journal
      ? { id, no: '', nama: '', debit: 0, kredit: 0, catatan: '', cc: '' }
      : { id, no: '', nama: '', total: 0, catatan: '', cc: '', proyek: '' };
    setLines((arr) => [...arr, blank]);
    window.setTimeout(() => setEditing({ rowId: id, col: 'no' }), 10);
  }, [cfg.journal, lines]);

  const addLineFromCoa = React.useCallback(
    (row: LookupRow): void => {
      const id = (lines[lines.length - 1]?.id ?? 0) + 1;
      const base: TrxLine = cfg.journal
        ? {
            id,
            no: row.code,
            nama: row.name,
            debit: 0,
            kredit: 0,
            catatan: '',
            cc: '',
          }
        : {
            id,
            no: row.code,
            nama: row.name,
            total: 0,
            catatan: '',
            cc: '',
            proyek: '',
          };
      setLines((arr) => [...arr, base]);
      notify(`Baris ditambah: ${row.code} ${row.name}`, 'info');
    },
    [cfg.journal, lines],
  );

  const reset = React.useCallback((): void => {
    setLines(seedLines(cfg));
    setHead((h) => ({ ...h, party: '', uraian: '' }));
    setContact(null);
    notify('Form direset', 'info');
  }, [cfg]);

  const save = React.useCallback(
    (andNew: boolean): void => {
      if (headerErr) {
        notify(`${cfg.party} wajib diisi.`, 'danger');
        return;
      }
      if (!balanced) {
        notify('Jurnal belum balance — Debit ≠ Kredit.', 'danger');
        return;
      }
      notify(`${t(cfg.label)} tersimpan`, 'success');
      if (andNew) {
        setHead((h) => ({ ...h, party: '', uraian: '' }));
        setLines(seedLines(cfg));
        setContact(null);
      } else {
        onNavigate(backTo);
      }
    },
    [headerErr, balanced, cfg, t, onNavigate, backTo],
  );

  const onKey = React.useCallback(
    (e: KeyboardEvent): void => {
      if (pickerOpen || lookup) return;
      const target = e.target as HTMLElement | null;
      const tag = target?.tagName ?? '';
      const inField = ['INPUT', 'TEXTAREA', 'SELECT'].includes(tag);
      if (e.key === 'Escape' && !editing && !inField) {
        onNavigate(backTo);
        return;
      }
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 's') {
        e.preventDefault();
        save(e.shiftKey);
        return;
      }
      if (e.key === '+' && !inField) {
        e.preventDefault();
        addLine();
      }
    },
    [pickerOpen, lookup, editing, onNavigate, backTo, save, addLine],
  );
  useTabKey(onKey);

  const openPicker = React.useCallback(() => setPickerOpen(true), []);
  const openLookup = React.useCallback(
    (kind: LookupKind, target: TrxLookupTarget) => setLookup({ kind, target }),
    [],
  );

  return (
    <div className="page" onClick={() => setEditing(null)}>
      <TrxFormHeader
        cfg={cfg}
        t={t}
        onBack={() => onNavigate(backTo)}
        onReset={reset}
        onSave={save}
      />

      <div className="form-section" style={{ alignItems: 'start' }}>
        <TrxFormFieldsLeft
          cfg={cfg}
          t={t}
          head={head}
          setHead={setHead}
          headerErr={headerErr}
          openPicker={openPicker}
          openLookup={openLookup}
        />
        <TrxFormFieldsMid
          cfg={cfg}
          t={t}
          head={head}
          setHead={setHead}
          headerErr={headerErr}
          openPicker={openPicker}
          openLookup={openLookup}
        />
        <TrxFormSummary
          cfg={cfg}
          t={t}
          head={head}
          setHead={setHead}
          total={total}
          totalKredit={totalKredit}
          balanced={balanced}
        />
      </div>

      <TrxFormTabs
        t={t}
        tab={tab}
        setTab={setTab}
        linesCount={lines.length}
        coaQuery={coaQuery}
        setCoaQuery={setCoaQuery}
        onOpenCoaLookup={() => openLookup('coa', 'coa-line')}
        onAddLine={addLine}
      />

      {tab === 'detail' && (
        <TrxLines
          cfg={cfg}
          t={t}
          lines={lines}
          setLines={setLines}
          editing={editing}
          setEditing={setEditing}
          total={total}
          totalKredit={totalKredit}
          head={{ uang: head.uang }}
          addLine={addLine}
        />
      )}
      {tab === 'info' && <TrxInfoPanel label={t(cfg.label)} />}
      {tab === 'audit' && <TrxAuditPanel />}

      <ContactPicker
        open={pickerOpen}
        onClose={() => setPickerOpen(false)}
        onSelect={(c) => {
          setContact(c);
          setHead((h) => ({ ...h, party: c.name }));
        }}
        t={t}
      />

      <LookupModal
        open={!!lookup}
        kind={lookup?.kind ?? null}
        onSelect={(val, row) => {
          const tgt = lookup?.target;
          if (tgt === 'akun') setHead((h) => ({ ...h, akun: val }));
          else if (tgt === 'lokasi') setHead((h) => ({ ...h, lokasi: val }));
          else if (tgt === 'coa-line') addLineFromCoa(row);
        }}
        onClose={() => setLookup(null)}
      />

      <TrxFooterShortcuts />
    </div>
  );
}

'use client';

/**
 * Journal transaction master-detail form — shared organism for General Journal
 * (GJ), Adjustment (AJ), Memorial (JM), Opening Balance (BB), Revaluation (RV).
 * Header rendered 100% from Form Builder config (field set/order/slot/labels/
 * visibility/placeholder/required/readonly via useFormFields). Structural fields
 * bind via JournalStructuralField; custom fields via CashBankCustomField. Detail =
 * config-driven Debit/Kredit grid. Status read-only; transitions via list actions.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { JournalLinesEditor, computeJournalTotals, type JournalLinesHandle } from '@/components/organisms/journal-lines';
import { listCurrencies, type ErpCurrency } from '@/lib/api/currencies';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { notify } from '@/lib/feedback';
import { TransactionAttachmentUpload } from '@/components/organisms/transaction-attachment-upload';
import { formatNumber } from '@/lib/format';
import { arrowFieldNav } from '@/lib/field-focus-nav';
import type { ErpDocumentStatus } from '@/lib/api/fin-journal-entries';
import type { ErpFormField, FormColumnSlot } from '@/lib/api/form-fields';
import { formDefaultsPatch, type JournalFormData } from './journal-form-model';
import { useFormFields, buildFormConfig } from '@/lib/use-form-fields';
import {
  JournalStructuralField,
  type JournalStructuralFieldCtx,
} from '@/components/molecules/journal-structural-field';
import { CashBankCustomField } from '@/components/molecules/cash-bank-custom-fields';

const EDITABLE: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'APPROVE_1', 'APPROVE_2', 'APPROVE_3', 'APPROVE_4', 'REJECTED'];
const SLOTS: FormColumnSlot[] = ['LEFT', 'CENTER', 'RIGHT'];

/** Fallback header layout until the FIN.* config loads (mirrors backend JOURNAL_DEFAULTS). */
const DEFAULT_JOURNAL_FIELDS: ErpFormField[] = [
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'notes', kind: 'STRUCTURAL', label: 'Catatan', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'entryDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT' },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'currencyId', kind: 'STRUCTURAL', label: 'Uang', fieldType: 'CURRENCY', isRequired: true, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
];

export function JournalTransactionForm({
  data,
  onChange,
  transactionCode,
  saving,
  allowedCreationStatuses,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: JournalFormData;
  onChange: (d: JournalFormData) => void;
  /** Kustomisasi Grid + Form Builder code, e.g. "FIN.GJ". */
  transactionCode?: string;
  saving?: boolean;
  /**
   * Statuses available in the creation-status dropdown (create mode only).
   * When undefined or single-element, the existing read-only badge is shown.
   */
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  const [tab, setTab] = React.useState<string>('detail');
  const [currencies, setCurrencies] = React.useState<ErpCurrency[]>([]);
  const linesRef = React.useRef<JournalLinesHandle>(null);
  const [lineRequiredMissing, setLineRequiredMissing] = React.useState<string[]>([]);
  const set = (p: Partial<JournalFormData>) => onChange({ ...data, ...p });

  const headerKeyNav = (e: React.KeyboardEvent<HTMLElement>) =>
    arrowFieldNav(e, {
      onForwardExit: () => {
        if (!linesRef.current) return false;
        setTab('detail');
        linesRef.current.focus();
        return true;
      },
    });

  const fallbackConfig = React.useMemo(() => buildFormConfig(DEFAULT_JOURNAL_FIELDS), []);
  const loaded = useFormFields(transactionCode);
  const formConfig = Object.keys(loaded.byKey).length ? loaded : fallbackConfig;

  const guardSave = (run: () => void) => () => {
    if (lineRequiredMissing.length) {
      notify(`Lengkapi kolom wajib di grid: ${lineRequiredMissing.join(', ')}`, 'warn');
      setTab('detail');
      return;
    }
    run();
  };

  const locked = !EDITABLE.includes(data.status);
  const showStatusPicker = !!allowedCreationStatuses && allowedCreationStatuses.length > 1 && !data.id;
  const ph = (key: string, fallback: string) => formConfig.byKey[key]?.placeholder || fallback;
  const ro = (key: string) => locked || formConfig.byKey[key]?.isReadonly === true;

  // Apply Form Builder defaults once on a new record (after config + currencies load).
  const defaultsApplied = React.useRef(false);
  React.useEffect(() => {
    if (data.id || defaultsApplied.current) return;
    if (Object.keys(loaded.byKey).length === 0) return;
    const patch = formDefaultsPatch(data, loaded);
    const needFallback = !data.currencyId && !patch.currencyId;
    if (needFallback && currencies.length === 0) return;
    defaultsApplied.current = true;
    if (needFallback) {
      const idr = currencies.find((c) => c.code === 'IDR') ?? currencies[0];
      if (idr) patch.currencyId = idr.id;
    }
    if (Object.keys(patch).length > 0) onChange({ ...data, ...patch });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loaded, currencies]);

  React.useEffect(() => {
    Promise.allSettled([
      listCurrencies({ page: 1, limit: 100, isActive: true }),
      listCurrencies({ search: 'IDR', limit: 5, isActive: true }),
    ]).then((res) => {
      const byId = new Map<string, ErpCurrency>();
      for (const r of res) {
        if (r.status === 'fulfilled') r.value.data.forEach((c) => byId.set(c.id, c));
      }
      if (byId.size > 0) setCurrencies([...byId.values()]);
    });
  }, []);

  const currencyLabel = React.useMemo(() => {
    const c = currencies.find((x) => x.id === data.currencyId);
    if (c) return `${c.code} - ${c.name}`;
    const cfg = formConfig.byKey['currencyId'];
    if (cfg?.defaultValue && cfg.defaultValue === data.currencyId) return cfg.defaultValueLabel ?? undefined;
    return undefined;
  }, [currencies, data.currencyId, formConfig]);

  const setCustomField = (key: string, value: string | number | null) =>
    set({ customFields: { ...data.customFields, [key]: value } });

  const ctx: JournalStructuralFieldCtx = { data, set, ph, ro, locked, currencyLabel };
  const totals = computeJournalTotals(data.lines);

  const renderSlot = (slot: FormColumnSlot) =>
    formConfig.slotFields[slot]
      .filter((f) => f.isVisible)
      .map((f) => (f.kind === 'STRUCTURAL'
        ? <JournalStructuralField key={f.fieldKey} field={f} ctx={ctx} />
        : (
          <CashBankCustomField
            key={f.fieldKey}
            field={f}
            value={data.customFields[f.fieldKey] ?? null}
            onChange={(v) => setCustomField(f.fieldKey, v)}
            disabled={locked}
          />
        )));

  return (
    <div className="jv-form flex flex-col gap-4">
      <div className="flex items-center gap-2 flex-wrap">
        <button type="button" className="btn primary" onClick={guardSave(onSave)} disabled={saving || locked}>
          <Icon name="save" size={13} /> Simpan
        </button>
        {!data.id && (
          <button type="button" className="btn" onClick={guardSave(onSaveNew)} disabled={saving || locked}>
            Simpan &amp; Baru
          </button>
        )}
        <button type="button" className="btn ghost" onClick={onReset} disabled={saving}>
          <Icon name="refresh" size={13} /> Reset
        </button>
        <div className="flex-1" />
        {showStatusPicker ? (
          <Select
            value={data.status}
            onValueChange={(v) => onChange({ ...data, status: v as typeof data.status })}
          >
            <SelectTrigger className="w-[160px] h-7 text-sm">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {allowedCreationStatuses!.map((s) => (
                <SelectItem key={s} value={s}>
                  {statusLabel(s)}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        ) : (
          <Badge variant={statusBadgeVariant(data.status)} dot>{statusLabel(data.status)}</Badge>
        )}
      </div>

      <div
        className="grid grid-cols-1 md:grid-cols-3 gap-x-6 gap-y-3 rounded-lg border border-border p-4"
        onKeyDown={headerKeyNav}
      >
        {SLOTS.map((slot) => (
          <div key={slot} className="flex flex-col gap-3">{renderSlot(slot)}</div>
        ))}
      </div>

      <div className="flex gap-1 border-b border-border">
        {[{ key: 'detail', label: 'Detail' }, { key: 'lampiran', label: 'Lampiran' }, { key: 'info', label: 'Info' }].map((t) => (
          <button
            key={t.key}
            type="button"
            className={`px-3 py-1.5 text-sm border-b-2 -mb-px ${
              tab === t.key ? 'border-primary text-primary font-medium' : 'border-transparent text-muted-foreground'
            }`}
            onClick={() => setTab(t.key)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'lampiran' && (
        <TransactionAttachmentUpload domain="fin" docType={transactionCode ?? 'FIN'} docId={data.id ?? null} />
      )}
      {tab === 'detail' && (
        <JournalLinesEditor
          ref={linesRef}
          lines={data.lines}
          onChange={(lines) => set({ lines })}
          readOnly={locked}
          transactionCode={transactionCode}
          onValidityChange={setLineRequiredMissing}
        />
      )}
      {tab === 'info' && (
        <dl className="grid grid-cols-2 gap-y-2 gap-x-6 text-sm max-w-xl">
          <dt className="text-muted-foreground">No Transaksi</dt>
          <dd className="mono">{data.docNumber || '—'}</dd>
          <dt className="text-muted-foreground">Status</dt>
          <dd>{statusLabel(data.status)}</dd>
          <dt className="text-muted-foreground">Tanggal Posting</dt>
          <dd>{data.postedAt ? data.postedAt.slice(0, 19).replace('T', ' ') : '—'}</dd>
        </dl>
      )}

      <div className="flex justify-end items-center gap-6 border-t border-border pt-3 text-sm">
        <span>Σ Debit <strong className="tabular-nums ml-2">{formatNumber(totals.debit, 2)}</strong></span>
        <span>Σ Kredit <strong className="tabular-nums ml-2">{formatNumber(totals.credit, 2)}</strong></span>
        <span className={totals.balanced ? 'text-emerald-600 font-medium' : 'text-rose-600 font-medium'}>
          {totals.balanced ? '● Balance' : '● Tidak balance'}
        </span>
      </div>
    </div>
  );
}

'use client';

/**
 * Giro transaction master-detail form — shared organism for Receipt Giro (RG),
 * Send Giro (SG), Receipt Giro Clearing (RGC), Send Giro Clearing (SGC). Header
 * rendered 100% from Form Builder config (field set/order/slot/labels/visibility/
 * placeholder/required/readonly via useFormFields). Structural fields bind via
 * GiroStructuralField; custom fields via CashBankCustomField. Detail = the register
 * instrument grid (kind=REGISTER) or the outstanding-giro clearing picker
 * (kind=CLEAR). Status read-only; transitions via list actions.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  GiroInstrumentsEditor,
  computeGiroTotal,
  type GiroInstrumentsHandle,
} from '@/components/organisms/giro-instruments';
import {
  GiroClearingLines,
  computeClearingTotal,
} from '@/components/organisms/giro-clearing-lines';
import { listCurrencies, type ErpCurrency } from '@/lib/api/currencies';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { notify } from '@/lib/feedback';
import { TransactionAttachmentUpload } from '@/components/organisms/transaction-attachment-upload';
import { formatNumber } from '@/lib/format';
import { arrowFieldNav } from '@/lib/field-focus-nav';
import type { ErpDocumentStatus } from '@/lib/api/fin-journal-entries';
import type { GiroKind } from '@/lib/api/fin-giro-entries';
import type { ErpFormField, FormColumnSlot } from '@/lib/api/form-fields';
import { formDefaultsPatch, type GiroFormData } from './giro-form-model';
import { useFormFields, buildFormConfig } from '@/lib/use-form-fields';
import {
  GiroStructuralField,
  type GiroStructuralFieldCtx,
} from '@/components/molecules/giro-structural-field';
import { CashBankCustomField } from '@/components/molecules/cash-bank-custom-fields';

const EDITABLE: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'APPROVE_1', 'APPROVE_2', 'APPROVE_3', 'APPROVE_4', 'REJECTED'];
const SLOTS: FormColumnSlot[] = ['LEFT', 'CENTER', 'RIGHT'];

/** Fallback header layout for REGISTER (RG/SG) until the FIN.* config loads. */
const DEFAULT_REGISTER_FIELDS: ErpFormField[] = [
  { fieldKey: 'partnerId', kind: 'STRUCTURAL', label: 'Partner', fieldType: 'PARTNER', isRequired: false, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'notes', kind: 'STRUCTURAL', label: 'Catatan', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'entryDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT' },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'currencyId', kind: 'STRUCTURAL', label: 'Uang', fieldType: 'CURRENCY', isRequired: true, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
];

/** Fallback header layout for CLEAR (RGC/SGC) — adds the settlement bank account. */
const DEFAULT_CLEAR_FIELDS: ErpFormField[] = [
  { fieldKey: 'partnerId', kind: 'STRUCTURAL', label: 'Partner', fieldType: 'PARTNER', isRequired: false, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'notes', kind: 'STRUCTURAL', label: 'Catatan', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'bankAccountId', kind: 'STRUCTURAL', label: 'Bank Pencairan', fieldType: 'ACCOUNT', isRequired: true, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'entryDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT' },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'currencyId', kind: 'STRUCTURAL', label: 'Uang', fieldType: 'CURRENCY', isRequired: true, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
];

const defaultFields = (kind: GiroKind) =>
  kind === 'CLEAR' ? DEFAULT_CLEAR_FIELDS : DEFAULT_REGISTER_FIELDS;

export function GiroTransactionForm({
  data,
  onChange,
  transactionCode,
  saving,
  allowedCreationStatuses,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: GiroFormData;
  onChange: (d: GiroFormData) => void;
  /** Kustomisasi Grid + Form Builder code, e.g. "FIN.RG". */
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
  const linesRef = React.useRef<GiroInstrumentsHandle>(null);
  const [detailMissing, setDetailMissing] = React.useState<string[]>([]);
  const set = (p: Partial<GiroFormData>) => onChange({ ...data, ...p });
  const isRegister = data.kind === 'REGISTER';

  const headerKeyNav = (e: React.KeyboardEvent<HTMLElement>) =>
    arrowFieldNav(e, {
      onForwardExit: () => {
        if (!isRegister || !linesRef.current) return false;
        setTab('detail');
        linesRef.current.focus();
        return true;
      },
    });

  const fallbackConfig = React.useMemo(() => buildFormConfig(defaultFields(data.kind)), [data.kind]);
  const loaded = useFormFields(transactionCode);
  const formConfig = Object.keys(loaded.byKey).length ? loaded : fallbackConfig;

  const guardSave = (run: () => void) => () => {
    if (detailMissing.length) {
      notify(
        isRegister
          ? `Lengkapi kolom wajib di grid: ${detailMissing.join(', ')}`
          : `Lengkapi Tgl Cair untuk giro: ${detailMissing.join(', ')}`,
        'warn',
      );
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

  const ctx: GiroStructuralFieldCtx = { data, set, ph, ro, locked, currencyLabel };
  const total = isRegister ? computeGiroTotal(data.instruments) : computeClearingTotal(data.clearings);

  const renderSlot = (slot: FormColumnSlot) =>
    formConfig.slotFields[slot]
      .filter((f) => f.isVisible)
      .map((f) => (f.kind === 'STRUCTURAL'
        ? <GiroStructuralField key={f.fieldKey} field={f} ctx={ctx} />
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
    <div className="giro-form flex flex-col gap-4">
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
      {tab === 'detail' && (isRegister ? (
        <GiroInstrumentsEditor
          ref={linesRef}
          lines={data.instruments}
          onChange={(instruments) => set({ instruments })}
          readOnly={locked}
          transactionCode={transactionCode}
          onValidityChange={setDetailMissing}
        />
      ) : (
        <GiroClearingLines
          type={data.type}
          rows={data.clearings}
          defaultClearedDate={data.entryDate}
          onChange={(clearings) => set({ clearings })}
          readOnly={locked}
          onValidityChange={setDetailMissing}
        />
      ))}
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
        <span>Σ Nominal <strong className="tabular-nums ml-2">{formatNumber(total, 2)}</strong></span>
      </div>
    </div>
  );
}

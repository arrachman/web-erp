'use client';

/**
 * Purchasing transaction master-detail form — shared organism for Purchase Order
 * (PO) and future item-based purchasing docs (PI/GR/…). The header is rendered
 * 100% from Form Builder config (field set/order/slot/labels/visibility/
 * placeholder/required/read-only via `useFormFields`); structural fields (bound
 * to DB columns) render via PurStructuralField, custom fields via
 * CashBankCustomField. Detail = config-driven item-line grid (Kustomisasi Grid).
 * Status is read-only (badge) — transitions happen via list workflow actions
 * (§2.7).
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
import { PurItemLinesEditor, computeLineTotal, type PurItemLinesHandle } from '@/components/organisms/pur-item-lines';
import { listCurrencies, type ErpCurrency } from '@/lib/api/currencies';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { notify } from '@/lib/feedback';
import { TransactionAttachmentUpload } from '@/components/organisms/transaction-attachment-upload';
import { formatNumber } from '@/lib/format';
import { arrowFieldNav } from '@/lib/field-focus-nav';
import type { ErpDocumentStatus } from '@/lib/api/pur-orders';
import { type FormColumnSlot } from '@/lib/api/form-fields';
import {
  DEFAULT_PUR_FORM_FIELDS,
  formDefaultsPatch,
  type PurOrderFormData,
} from './pur-order-form-model';
import { useFormFields, buildFormConfig } from '@/lib/use-form-fields';
import { PurStructuralField, type PurStructuralFieldCtx } from '@/components/molecules/pur-structural-field';
import { CashBankCustomField } from '@/components/molecules/cash-bank-custom-fields';

const EDITABLE: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'APPROVE_1', 'APPROVE_2', 'APPROVE_3', 'APPROVE_4', 'REJECTED'];
const SLOTS: FormColumnSlot[] = ['LEFT', 'CENTER', 'RIGHT'];

export function PurchaseTransactionForm({
  data,
  onChange,
  transactionCode,
  saving,
  allowedCreationStatuses,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: PurOrderFormData;
  onChange: (d: PurOrderFormData) => void;
  /** Kustomisasi Grid / Form Builder code, e.g. "PUR.PO". */
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
  const linesRef = React.useRef<PurItemLinesHandle>(null);
  const [lineRequiredMissing, setLineRequiredMissing] = React.useState<string[]>([]);
  const set = (p: Partial<PurOrderFormData>) => onChange({ ...data, ...p });

  const headerKeyNav = (e: React.KeyboardEvent<HTMLElement>) =>
    arrowFieldNav(e, {
      onForwardExit: () => {
        if (!linesRef.current) return false;
        setTab('detail');
        linesRef.current.focus();
        return true;
      },
    });

  const fallbackConfig = React.useMemo(() => buildFormConfig(DEFAULT_PUR_FORM_FIELDS), []);
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

  const subtotal = data.lines.reduce((s, l) => s + computeLineTotal(l), 0);

  const ctx: PurStructuralFieldCtx = { data, set, ph, ro, locked, currencyLabel };

  const renderSlot = (slot: FormColumnSlot) =>
    formConfig.slotFields[slot]
      .filter((f) => f.isVisible)
      .map((f) => (f.kind === 'STRUCTURAL'
        ? <PurStructuralField key={f.fieldKey} field={f} ctx={ctx} />
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
    <div className="po-form flex flex-col gap-4">
      {/* Toolbar */}
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
          <Badge variant={statusBadgeVariant(data.status)} dot>
            {statusLabel(data.status)}
          </Badge>
        )}
      </div>

      {/* Header — rendered from Form Builder config; LEFT/CENTER/RIGHT slots. */}
      <div
        className="grid grid-cols-1 md:grid-cols-3 gap-x-6 gap-y-3 rounded-lg border border-border p-4"
        onKeyDown={headerKeyNav}
      >
        {SLOTS.map((slot) => (
          <div key={slot} className="flex flex-col gap-3">
            {renderSlot(slot)}
          </div>
        ))}
      </div>

      {/* Tabs: Detail → Info */}
      <div className="flex gap-1 border-b border-border">
        {[
          { key: 'detail', label: 'Detail' },
          { key: 'lampiran', label: 'Lampiran' },
          { key: 'info', label: 'Info' },
        ].map((t) => (
          <button
            key={t.key}
            type="button"
            className={`px-3 py-1.5 text-sm border-b-2 -mb-px ${
              tab === t.key
                ? 'border-primary text-primary font-medium'
                : 'border-transparent text-muted-foreground'
            }`}
            onClick={() => setTab(t.key)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'lampiran' && (
        <TransactionAttachmentUpload domain="pur" docType={transactionCode ?? 'PUR'} docId={data.id ?? null} />
      )}
      {tab === 'detail' && (
        <PurItemLinesEditor
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
          <dt className="text-muted-foreground">Catatan</dt>
          <dd>
            <Input value={data.notes} disabled={locked} onChange={(e) => set({ notes: e.target.value })} />
          </dd>
        </dl>
      )}

      {/* Footer total */}
      <div className="flex justify-end items-center gap-3 border-t border-border pt-3">
        <span className="text-sm text-muted-foreground">Subtotal</span>
        <span className="text-lg font-semibold tabular-nums">{formatNumber(subtotal, 2)}</span>
      </div>
    </div>
  );
}

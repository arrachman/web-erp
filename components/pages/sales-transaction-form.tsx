'use client';

/**
 * Sales transaction master-detail form — shared organism for Sales Order (SO)
 * and future item-based sales docs (SI/DO/…). The header is rendered 100% from
 * Form Builder config (field set/order/slot/labels/visibility/placeholder/
 * required/read-only via `useFormFields`); structural fields (bound to DB
 * columns) render via SlsStructuralField, custom fields via CashBankCustomField.
 * Detail = config-driven item-line grid (Kustomisasi Grid). Status is read-only
 * (badge) — transitions happen via list workflow actions (§2.7).
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
import { SlsItemLinesEditor, computeLineTotal, type SlsItemLinesHandle } from '@/components/organisms/sls-item-lines';
import { listCurrencies, type ErpCurrency } from '@/lib/api/currencies';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { notify } from '@/lib/feedback';
import { TransactionAttachmentUpload } from '@/components/organisms/transaction-attachment-upload';
import { formatNumber } from '@/lib/format';
import { arrowFieldNav } from '@/lib/field-focus-nav';
import type { ErpDocumentStatus } from '@/lib/api/sls-orders';
import { type FormColumnSlot } from '@/lib/api/form-fields';
import {
  DEFAULT_SLS_FORM_FIELDS,
  formDefaultsPatch,
  type SlsOrderFormData,
} from './sls-order-form-model';
import { useFormFields, buildFormConfig } from '@/lib/use-form-fields';
import { SlsStructuralField, type SlsStructuralFieldCtx } from '@/components/molecules/sls-structural-field';
import { CashBankCustomField } from '@/components/molecules/cash-bank-custom-fields';

const EDITABLE: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'APPROVE_1', 'APPROVE_2', 'APPROVE_3', 'APPROVE_4', 'REJECTED'];
const SLOTS: FormColumnSlot[] = ['LEFT', 'CENTER', 'RIGHT'];

export function SalesTransactionForm<T extends SlsOrderFormData>({
  data,
  onChange,
  transactionCode,
  saving,
  allowedCreationStatuses,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: T;
  onChange: (d: T) => void;
  /** Kustomisasi Grid / Form Builder code, e.g. "SLS.SO". */
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
  const linesRef = React.useRef<SlsItemLinesHandle>(null);
  const [lineRequiredMissing, setLineRequiredMissing] = React.useState<string[]>([]);
  const set = (p: Partial<SlsOrderFormData>) => onChange({ ...data, ...p } as T);

  const headerKeyNav = (e: React.KeyboardEvent<HTMLElement>) =>
    arrowFieldNav(e, {
      onForwardExit: () => {
        if (!linesRef.current) return false;
        setTab('detail');
        linesRef.current.focus();
        return true;
      },
    });

  const fallbackConfig = React.useMemo(() => buildFormConfig(DEFAULT_SLS_FORM_FIELDS), []);
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
    if (Object.keys(patch).length > 0) onChange({ ...data, ...patch } as T);
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
  const discAmt = Number(data.discountAmount) || (data.discountPercent ? subtotal * Number(data.discountPercent) / 100 : 0);
  const lineTaxTotal = 0; // computed server-side; UI shows header adjustments only
  const headerTax = (Number(data.tax1Amount) || 0) + (Number(data.tax2Amount) || 0);
  const otherCost = Number(data.otherCostAmount) || 0;
  const grandTotal = subtotal - discAmt + headerTax + otherCost + lineTaxTotal;
  const hasAdj = !!(data.discountPercent || data.discountAmount || data.tax1Amount || data.tax2Amount || data.otherCostAmount);
  const [adjOpen, setAdjOpen] = React.useState(false);

  const ctx: SlsStructuralFieldCtx = { data, set, ph, ro, locked, currencyLabel };

  const renderSlot = (slot: FormColumnSlot) =>
    formConfig.slotFields[slot]
      .filter((f) => f.isVisible)
      .map((f) => (f.kind === 'STRUCTURAL'
        ? <SlsStructuralField key={f.fieldKey} field={f} ctx={ctx} />
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
    <div className="so-form flex flex-col gap-4">
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
            onValueChange={(v) => onChange({ ...data, status: v as typeof data.status } as T)}
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
        <TransactionAttachmentUpload domain="sls" docType={transactionCode ?? 'SLS'} docId={data.id ?? null} />
      )}
      {tab === 'detail' && (
        <SlsItemLinesEditor
          ref={linesRef}
          lines={data.lines}
          onChange={(lines) => set({ lines })}
          readOnly={locked}
          transactionCode={transactionCode}
          onValidityChange={setLineRequiredMissing}
          salesTier={data.salesTier}
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

      {/* Footer — subtotal + adjustments + grand total */}
      <div className="flex flex-col items-end gap-1 border-t border-border pt-3">
        <div className="flex items-center gap-6 text-sm">
          <span className="text-muted-foreground">Subtotal</span>
          <span className="tabular-nums font-medium w-36 text-right">{formatNumber(subtotal, 2)}</span>
        </div>

        {/* Adjustment toggle */}
        <button
          type="button"
          className="btn ghost xs text-xs"
          onClick={() => setAdjOpen((o) => !o)}
          disabled={locked}
        >
          {adjOpen || hasAdj ? 'Tutup penyesuaian' : '+ Diskon / Pajak / Biaya'}
        </button>

        {(adjOpen || hasAdj) && (
          <div className="grid grid-cols-[auto_1fr] items-center gap-x-4 gap-y-1.5 text-sm mt-1 w-80">
            <span className="text-muted-foreground text-right">Diskon %</span>
            <Input
              className="h-7 text-sm text-right"
              value={data.discountPercent}
              disabled={locked}
              placeholder="0"
              onChange={(e) => set({ discountPercent: e.target.value, discountAmount: '' })}
            />
            <span className="text-muted-foreground text-right">Diskon Rp</span>
            <Input
              className="h-7 text-sm text-right"
              value={data.discountAmount}
              disabled={locked}
              placeholder="0"
              onChange={(e) => set({ discountAmount: e.target.value, discountPercent: '' })}
            />
            {discAmt > 0 && (
              <>
                <span className="text-muted-foreground text-right text-xs">  – Diskon</span>
                <span className="tabular-nums text-right text-destructive">–{formatNumber(discAmt, 2)}</span>
              </>
            )}
            <span className="text-muted-foreground text-right">Pajak 1 Rp</span>
            <Input
              className="h-7 text-sm text-right"
              value={data.tax1Amount}
              disabled={locked}
              placeholder="0"
              onChange={(e) => set({ tax1Amount: e.target.value })}
            />
            <span className="text-muted-foreground text-right">Pajak 2 Rp</span>
            <Input
              className="h-7 text-sm text-right"
              value={data.tax2Amount}
              disabled={locked}
              placeholder="0"
              onChange={(e) => set({ tax2Amount: e.target.value })}
            />
            <span className="text-muted-foreground text-right">Biaya Lain Rp</span>
            <Input
              className="h-7 text-sm text-right"
              value={data.otherCostAmount}
              disabled={locked}
              placeholder="0"
              onChange={(e) => set({ otherCostAmount: e.target.value })}
            />
          </div>
        )}

        <div className="flex items-center gap-6 text-base font-semibold mt-1 border-t border-border pt-2 min-w-64">
          <span>Grand Total</span>
          <span className="tabular-nums w-36 text-right">{formatNumber(grandTotal, 2)}</span>
        </div>
      </div>
    </div>
  );
}

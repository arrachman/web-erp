'use client';

/**
 * Cash/bank transaction master-detail form — shared organism for Kas Masuk (CR),
 * Kas Keluar (CD), Bank Keluar (BD)… The header is rendered 100% from Form Builder
 * config: field set, order, slot, labels, visibility, placeholder, required and
 * read-only all come from `useFormFields`. Structural fields (bound to DB columns)
 * render via CashBankStructuralField; custom fields via CashBankCustomField.
 * Status is read-only (badge) — transitions happen via list workflow actions (§2.7).
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
import { CashBankLinesEditor, type CashBankLinesHandle } from '@/components/organisms/cash-bank-lines';
import { loadAccountOptionsCoded } from './items-form-lookups';
import { buildLookupLoader } from '@/lib/lookup-source-registry';
import { listCurrencies, type ErpCurrency } from '@/lib/api/currencies';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { notify } from '@/lib/feedback';
import { TransactionAttachmentUpload } from '@/components/organisms/transaction-attachment-upload';
import { formatNumber } from '@/lib/format';
import { arrowFieldNav } from '@/lib/field-focus-nav';
import type { ErpDocumentStatus } from '@/lib/api/fin-cash-receipts';
import { DEFAULT_FORM_FIELDS, type FormColumnSlot } from '@/lib/api/form-fields';
import { formDefaultsPatch, type CashBankFormData } from './cash-bank-form-model';
import { useFormFields, buildFormConfig } from '@/lib/use-form-fields';
import {
  CashBankStructuralField, type StructuralFieldCtx,
} from '@/components/molecules/cash-bank-structural-field';
import { CashBankCustomField } from '@/components/molecules/cash-bank-custom-fields';

/** Direction-specific header labels (only thing that differs CR vs CD). */
export interface CashBankFormLabels {
  /** Party picker, e.g. "Terima Dari" (CR) / "Bayar Ke" (CD). */
  partner: string;
  /** Cash/bank account picker, e.g. "Akun Kas [D]" (CR) / "Akun Kas [K]" (CD). */
  account: string;
}

/** Extra tab a caller injects between Detail and Info (e.g. Giro for Bank Masuk). */
export interface CashBankExtraTab {
  key: string;
  label: string;
  content: React.ReactNode;
}

/** Imperative handle exposed via ref so the parent page can focus a header field by key. */
export interface CashBankTransactionFormHandle {
  /** Scroll to and focus the first focusable input inside the header field with the given key. */
  focusField: (key: string) => void;
}

const EDITABLE: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'APPROVE_1', 'APPROVE_2', 'APPROVE_3', 'APPROVE_4', 'REJECTED'];
const SLOTS: FormColumnSlot[] = ['LEFT', 'CENTER', 'RIGHT'];

export const CashBankTransactionForm = React.forwardRef<
  CashBankTransactionFormHandle,
  {
  data: CashBankFormData;
  onChange: (d: CashBankFormData) => void;
  labels: CashBankFormLabels;
  /** Kustomisasi Grid code for the detail grid, e.g. "FIN.CR". */
  transactionCode?: string;
  /** Extra field rendered atop the left header column (e.g. Cara Bayar). */
  headerExtra?: React.ReactNode;
  /** Extra tabs injected between Detail and Info (e.g. Giro). */
  extraTabs?: CashBankExtraTab[];
  saving?: boolean;
  /**
   * Statuses available in the creation-status dropdown (create mode only).
   * When undefined or single-element, the existing read-only badge is shown.
   * The page should call useAllowedCreationStatuses() and pass the result here.
   */
  allowedCreationStatuses?: string[];
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}
>(function CashBankTransactionForm({
  data,
  onChange,
  labels,
  transactionCode,
  headerExtra,
  extraTabs = [],
  saving,
  allowedCreationStatuses,
  onSave,
  onSaveNew,
  onReset,
}, ref) {
  const [tab, setTab] = React.useState<string>('detail');
  const [currencies, setCurrencies] = React.useState<ErpCurrency[]>([]);
  const linesRef = React.useRef<CashBankLinesHandle>(null);
  const headerRef = React.useRef<HTMLDivElement>(null);

  // Expose imperative focusField so the parent page can scroll-to and focus
  // the first missing required header field after a failed save attempt.
  React.useImperativeHandle(ref, () => ({
    focusField: (key: string) => {
      const container = headerRef.current;
      if (!container) return;
      const row = container.querySelector<HTMLElement>(`[data-field-key="${key}"]`);
      if (!row) return;
      row.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
      const input = row.querySelector<HTMLElement>('input:not([disabled]):not([type="checkbox"]), button:not([disabled])');
      input?.focus();
    },
  }));
  // ArrowDown past the last header field hands focus to the detail grid (cell 0,0).
  const headerKeyNav = (e: React.KeyboardEvent<HTMLElement>) =>
    arrowFieldNav(e, {
      onForwardExit: () => {
        if (!linesRef.current) return false;
        setTab('detail');
        linesRef.current.focus();
        return true;
      },
    });
  // Required ("Wajib") grid columns still empty — reported by the lines editor.
  const [lineRequiredMissing, setLineRequiredMissing] = React.useState<string[]>([]);
  const set = (p: Partial<CashBankFormData>) => onChange({ ...data, ...p });

  // Fallback layout (direction labels injected) until the real config loads.
  const fallbackConfig = React.useMemo(
    () => buildFormConfig(DEFAULT_FORM_FIELDS.map((f) =>
      f.fieldKey === 'partnerId' ? { ...f, label: labels.partner }
        : f.fieldKey === 'bankAccountId' ? { ...f, label: labels.account }
          : f)),
    [labels.partner, labels.account],
  );
  const loaded = useFormFields(transactionCode);
  const formConfig = Object.keys(loaded.byKey).length ? loaded : fallbackConfig;

  // Block saving while a required grid column is empty; surface which ones.
  const guardSave = (run: () => void) => () => {
    if (lineRequiredMissing.length) {
      notify(`Lengkapi kolom wajib di grid: ${lineRequiredMissing.join(', ')}`, 'warn');
      setTab('detail');
      return;
    }
    run();
  };

  const locked = !EDITABLE.includes(data.status);

  // Show status picker in create mode when the role allows more than one starting status.
  const showStatusPicker =
    !!allowedCreationStatuses && allowedCreationStatuses.length > 1 && !data.id;

  const bankAccountLoader = React.useMemo(() => {
    const f = formConfig.byKey['bankAccountId'];
    const configured = (f?.lookupDefaultFilter || f?.lookupDefaultSort)
      ? buildLookupLoader('accounts', f.lookupDefaultFilter ?? null, f.lookupDefaultSort ?? null)
      : null;
    return configured ?? loadAccountOptionsCoded;
  }, [formConfig]);

  // Per-field placeholder / read-only from form builder config.
  const ph = (key: string, fallback: string) => formConfig.byKey[key]?.placeholder || fallback;
  const ro = (key: string) => locked || formConfig.byKey[key]?.isReadonly === true;

  // Apply Form Builder default values once, on a new (unsaved) record — after BOTH
  // the field config and the currency list have loaded (currencies feed the
  // base-currency fallback below + label resolution). Form Builder config is the
  // source of truth for the default currency; only fall back to base when unset.
  const defaultsApplied = React.useRef(false);
  React.useEffect(() => {
    if (data.id || defaultsApplied.current) return;
    if (Object.keys(loaded.byKey).length === 0) return; // real config not loaded yet
    const patch = formDefaultsPatch(data, loaded);
    // Config default is authoritative & needs no currency list. Only the
    // base-currency fallback (no configured default) waits for currencies.
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

  const setCustomField = (key: string, value: string | number | null) =>
    set({ customFields: { ...data.customFields, [key]: value } });
  const total = data.lines.reduce((s, l) => s + Number(l.amount || 0), 0);

  React.useEffect(() => {
    // Load the currency list for the picker + label resolution. The default
    // currency is decided by the Form Builder defaults effect above — NOT here
    // (the old hardcoded IDR fallback used to override the configured default).
    // Explicitly fetch IDR too: with 172 currencies the base currency sits past
    // the first page, so the base-currency fallback + its label would miss it.
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // SearchSelect trigger label for the selected currency ("IDR - Rupiah").
  // The currency may sit outside the fetched page (172 currencies) — fall back to
  // the Form Builder config's resolved default label so the picker isn't blank.
  const currencyLabel = React.useMemo(() => {
    const c = currencies.find((x) => x.id === data.currencyId);
    if (c) return `${c.code} - ${c.name}`;
    const cfg = formConfig.byKey['currencyId'];
    if (cfg?.defaultValue && cfg.defaultValue === data.currencyId) {
      return cfg.defaultValueLabel ?? undefined;
    }
    return undefined;
  }, [currencies, data.currencyId, formConfig]);

  const ctx: StructuralFieldCtx = { data, set, ph, ro, locked, currencyLabel, bankAccountLoader };

  const renderSlot = (slot: FormColumnSlot) =>
    formConfig.slotFields[slot]
      .filter((f) => f.isVisible)
      .map((f) => (f.kind === 'STRUCTURAL'
        ? <CashBankStructuralField key={f.fieldKey} field={f} ctx={ctx} />
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
    <div className="cr-form flex flex-col gap-4">
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

      {/* Header — rendered from Form Builder config; LEFT/CENTER/RIGHT slots, ordered by sortOrder.
          ArrowDown/ArrowUp move focus between fields like Tab/Shift+Tab (§ arrow field nav). */}
      <div
        ref={headerRef}
        className="grid grid-cols-1 md:grid-cols-3 gap-x-6 gap-y-3 rounded-lg border border-border p-4"
        onKeyDown={headerKeyNav}
      >
        {SLOTS.map((slot) => (
          <div key={slot} className="flex flex-col gap-3">
            {slot === 'LEFT' && headerExtra}
            {renderSlot(slot)}
          </div>
        ))}
      </div>

      {/* Tabs: Detail → [extra tabs] → Info */}
      <div className="flex gap-1 border-b border-border">
        {[
          { key: 'detail', label: 'Detail' },
          ...extraTabs.map((t) => ({ key: t.key, label: t.label })),
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

      {tab === 'detail' && (
        <CashBankLinesEditor
          ref={linesRef}
          lines={data.lines}
          onChange={(lines) => set({ lines })}
          readOnly={locked}
          transactionCode={transactionCode}
          onValidityChange={setLineRequiredMissing}
        />
      )}
      {extraTabs.map((t) => (tab === t.key ? <div key={t.key}>{t.content}</div> : null))}
      {tab === 'lampiran' && (
        <TransactionAttachmentUpload domain="fin" docType={transactionCode ?? 'FIN'} docId={data.id ?? null} />
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
            <Input
              value={data.notes}
              disabled={locked}
              onChange={(e) => set({ notes: e.target.value })}
            />
          </dd>
        </dl>
      )}

      {/* Footer total */}
      <div className="flex justify-end items-center gap-3 border-t border-border pt-3">
        <span className="text-sm text-muted-foreground">Total</span>
        <span className="text-lg font-semibold tabular-nums">{formatNumber(total, 2)}</span>
      </div>
    </div>
  );
});

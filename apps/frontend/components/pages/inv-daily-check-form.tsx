'use client';

/**
 * Inventory daily-check master-detail form. Config-driven header (Form Builder
 * via useFormFields('INV.DC')) + InvDailyCheckLinesEditor grid. Status read-only
 * (badge) — transitions happen via list workflow actions. No money totals —
 * daily checks track quantity. Accepts TrxFormPageProps for registry
 * compatibility. Atomic tier: Page.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  InvDailyCheckLinesEditor,
  type InvDailyCheckLinesHandle,
} from '@/components/organisms/inv-daily-check-lines';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { notify } from '@/lib/feedback';
import { TransactionAttachmentUpload } from '@/components/organisms/transaction-attachment-upload';
import { arrowFieldNav } from '@/lib/field-focus-nav';
import type { ErpDocumentStatus } from '@/lib/api/inv-daily-checks';
import type { InvDailyCheckTransition } from '@/lib/api/inv-daily-checks';
import { type FormColumnSlot } from '@/lib/api/form-fields';
import {
  DEFAULT_INV_DC_FORM_FIELDS,
  formDefaultsPatch,
  type InvDailyCheckFormData,
} from './inv-daily-check-form-model';
import { useFormFields, buildFormConfig } from '@/lib/use-form-fields';
import {
  InvDailyCheckStructuralField,
  type InvDailyCheckStructuralFieldCtx,
} from '@/components/molecules/inv-daily-check-structural-field';
import { CashBankCustomField } from '@/components/molecules/cash-bank-custom-fields';
import { invDailyCheckWorkflowActions } from '@/lib/inv-daily-check-workflow';
import type { TrxFormPageProps } from '@/lib/trx-route';

const EDITABLE: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'REJECTED'];
const SLOTS: FormColumnSlot[] = ['LEFT', 'CENTER', 'RIGHT'];

export interface InvDailyCheckFormOwnProps {
  data: InvDailyCheckFormData;
  onChange: (d: InvDailyCheckFormData) => void;
  transactionCode?: string;
  saving?: boolean;
  onSave: () => void;
  onSaveNew?: () => void;
  onReset: () => void;
  onTransition?: (action: InvDailyCheckTransition) => void;
}

export type InvDailyCheckFormProps = InvDailyCheckFormOwnProps & TrxFormPageProps;

export function InvDailyCheckForm({
  data,
  onChange,
  transactionCode,
  saving,
  onSave,
  onSaveNew,
  onReset,
  onTransition,
}: InvDailyCheckFormOwnProps) {
  const [tab, setTab] = React.useState<string>('detail');
  const linesRef = React.useRef<InvDailyCheckLinesHandle>(null);
  const [lineRequiredMissing, setLineRequiredMissing] = React.useState<string[]>([]);
  const set = (p: Partial<InvDailyCheckFormData>) => onChange({ ...data, ...p });
  const locked = !EDITABLE.includes(data.status);

  const headerKeyNav = (e: React.KeyboardEvent<HTMLElement>) =>
    arrowFieldNav(e, {
      onForwardExit: () => {
        if (!linesRef.current) return false;
        setTab('detail');
        linesRef.current.focus();
        return true;
      },
    });

  const fallbackConfig = React.useMemo(
    () => buildFormConfig(DEFAULT_INV_DC_FORM_FIELDS),
    [],
  );
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

  const ph = (key: string, fallback: string) => formConfig.byKey[key]?.placeholder || fallback;
  const ro = (key: string) => locked || formConfig.byKey[key]?.isReadonly === true;

  // Apply Form Builder defaults once on a new record (after config loads).
  const defaultsApplied = React.useRef(false);
  React.useEffect(() => {
    if (data.id || defaultsApplied.current) return;
    if (Object.keys(loaded.byKey).length === 0) return;
    defaultsApplied.current = true;
    const patch = formDefaultsPatch(data, loaded);
    if (Object.keys(patch).length > 0) onChange({ ...data, ...patch });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loaded]);

  const setCustomField = (key: string, value: string | number | null) =>
    set({ customFields: { ...data.customFields, [key]: value } });

  const ctx: InvDailyCheckStructuralFieldCtx = { data, set, ph, ro, locked };

  const renderSlot = (slot: FormColumnSlot) =>
    formConfig.slotFields[slot]
      .filter((f) => f.isVisible)
      .map((f) =>
        f.kind === 'STRUCTURAL' ? (
          <InvDailyCheckStructuralField key={f.fieldKey} field={f} ctx={ctx} />
        ) : (
          <CashBankCustomField
            key={f.fieldKey}
            field={f}
            value={data.customFields[f.fieldKey] ?? null}
            onChange={(v) => setCustomField(f.fieldKey, v)}
            disabled={locked}
          />
        ),
      );

  const workflowItems = onTransition
    ? invDailyCheckWorkflowActions(data.status, onTransition)
    : [];

  return (
    <div className="inv-daily-check-form flex flex-col gap-4">
      {/* Toolbar */}
      <div className="flex items-center gap-2 flex-wrap">
        <button
          type="button"
          className="btn primary"
          onClick={guardSave(onSave)}
          disabled={saving || locked}
        >
          <Icon name="save" size={13} /> Simpan
        </button>
        {!data.id && (
          <button
            type="button"
            className="btn"
            onClick={guardSave(onSaveNew ?? (() => {}))}
            disabled={saving || locked}
          >
            Simpan &amp; Baru
          </button>
        )}
        <button
          type="button"
          className="btn ghost"
          onClick={onReset}
          disabled={saving}
        >
          <Icon name="refresh" size={13} /> Reset
        </button>
        {workflowItems.map((item) => (
          <button
            key={item.label}
            type="button"
            className={`btn ${(item as {danger?: boolean}).danger ? 'danger' : 'secondary'}`}
            onClick={item.onSelect}
            disabled={saving}
          >
            {item.label}
          </button>
        ))}
        <div className="flex-1" />
        <Badge variant={statusBadgeVariant(data.status)} dot>
          {statusLabel(data.status)}
        </Badge>
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
        <TransactionAttachmentUpload domain="inv" docType={transactionCode ?? 'INV'} docId={data.id ?? null} />
      )}
      {tab === 'detail' && (
        <InvDailyCheckLinesEditor
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
            <Input
              value={data.notes}
              disabled={locked}
              onChange={(e) => set({ notes: e.target.value })}
            />
          </dd>
        </dl>
      )}
    </div>
  );
}

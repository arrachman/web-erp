'use client';

/**
 * Inventory weighbridge-ticket form (Receipt Weigher). Header-only — no item
 * line grid. Config-driven header via Form Builder (useFormFields('INV.RW'));
 * structural fields rendered by InvWeighbridgeStructuralField. Displays
 * computed netWeight as a read-only badge always below the header grid.
 * Status badge + workflow transitions via onTransition callback.
 * Accepts TrxFormPageProps for registry compatibility. Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Icon } from '@/components/ui/icons';
import { notify } from '@/lib/feedback';
import { arrowFieldNav } from '@/lib/field-focus-nav';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { type FormColumnSlot } from '@/lib/api/form-fields';
import { useFormFields, buildFormConfig } from '@/lib/use-form-fields';
import type { ErpDocumentStatus } from '@/lib/api/inv-weighbridge-tickets';
import type { InvWeighbridgeTicketTransition } from '@/lib/api/inv-weighbridge-tickets';
import {
  DEFAULT_INV_RW_FORM_FIELDS,
  computeNetWeight,
  type InvWeighbridgeTicketFormData,
} from './inv-weighbridge-ticket-form-model';
import {
  InvWeighbridgeStructuralField,
  type InvWeighbridgeStructuralFieldCtx,
} from '@/components/molecules/inv-weighbridge-structural-field';
import { CashBankCustomField } from '@/components/molecules/cash-bank-custom-fields';
import { invWeighbridgeTicketWorkflowActions } from '@/lib/inv-weighbridge-ticket-workflow';
import type { TrxFormPageProps } from '@/lib/trx-route';

const EDITABLE: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'REJECTED'];
const SLOTS: FormColumnSlot[] = ['LEFT', 'CENTER', 'RIGHT'];

export interface InvWeighbridgeTicketFormOwnProps {
  data: InvWeighbridgeTicketFormData;
  onChange: (d: InvWeighbridgeTicketFormData) => void;
  transactionCode?: string;
  saving?: boolean;
  onSave: () => void;
  onSaveNew?: () => void;
  onReset: () => void;
  onTransition?: (action: InvWeighbridgeTicketTransition) => void;
}

export type InvWeighbridgeTicketFormProps = InvWeighbridgeTicketFormOwnProps &
  TrxFormPageProps;

export function InvWeighbridgeTicketForm({
  data,
  onChange,
  transactionCode,
  saving,
  onSave,
  onSaveNew,
  onReset,
  onTransition,
}: InvWeighbridgeTicketFormOwnProps) {
  const set = (p: Partial<InvWeighbridgeTicketFormData>) => onChange({ ...data, ...p });
  const locked = !EDITABLE.includes(data.status);

  const headerKeyNav = (e: React.KeyboardEvent<HTMLElement>) => arrowFieldNav(e, {});

  const fallbackConfig = React.useMemo(
    () => buildFormConfig(DEFAULT_INV_RW_FORM_FIELDS),
    [],
  );
  const loaded = useFormFields(transactionCode);
  const formConfig = Object.keys(loaded.byKey).length ? loaded : fallbackConfig;

  const ph = (key: string, fallback: string) => formConfig.byKey[key]?.placeholder || fallback;
  const ro = (key: string) => locked || formConfig.byKey[key]?.isReadonly === true;

  const ctx: InvWeighbridgeStructuralFieldCtx = { data, set, ph, ro, locked };

  const setCustomField = (key: string, value: string | number | null) =>
    set({
      customFields: {
        ...((data as unknown as Record<string, unknown>).customFields as Record<string, string | number | null> | undefined ?? {}),
        [key]: value,
      },
    } as unknown as Partial<InvWeighbridgeTicketFormData>);

  const renderSlot = (slot: FormColumnSlot) =>
    formConfig.slotFields[slot]
      .filter((f) => f.isVisible)
      .map((f) =>
        f.kind === 'STRUCTURAL' ? (
          <InvWeighbridgeStructuralField key={f.fieldKey} field={f} ctx={ctx} />
        ) : (
          <CashBankCustomField
            key={f.fieldKey}
            field={f}
            value={null}
            onChange={(v) => setCustomField(f.fieldKey, v)}
            disabled={locked}
          />
        ),
      );

  const netWeight = computeNetWeight(data);
  const netWeightNum = parseFloat(netWeight);

  const workflowItems = onTransition
    ? invWeighbridgeTicketWorkflowActions(data.status, onTransition)
    : [];

  const guardSave = (run: () => void) => () => {
    if (!data.branchId || !data.ticketDate) {
      notify('Cabang dan Tanggal wajib diisi.', 'warn');
      return;
    }
    run();
  };

  return (
    <div className="inv-weighbridge-ticket-form flex flex-col gap-4">
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

      {/* Header — rendered from Form Builder config */}
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

      {/* Computed Net Weight — always visible read-only field */}
      <div className="flex items-center gap-3 px-4 py-3 rounded-lg border border-border bg-muted/30">
        <span className="text-sm text-muted-foreground">Berat Bersih (Net Weight)</span>
        <span className="text-sm font-semibold tabular-nums ml-auto">
          {isNaN(netWeightNum) ? '—' : netWeightNum.toLocaleString('id-ID', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
        </span>
        <Badge variant="info" dot={false}>
          Otomatis
        </Badge>
      </div>

      {/* Info tab */}
      <dl className="grid grid-cols-2 gap-y-2 gap-x-6 text-sm max-w-xl">
        <dt className="text-muted-foreground">No Transaksi</dt>
        <dd className="mono">{data.docNumber || '—'}</dd>
        <dt className="text-muted-foreground">Status</dt>
        <dd>{statusLabel(data.status)}</dd>
        {data.postedAt && (
          <>
            <dt className="text-muted-foreground">Tanggal Posting</dt>
            <dd>{data.postedAt.slice(0, 19).replace('T', ' ')}</dd>
          </>
        )}
        {data.notes && (
          <>
            <dt className="text-muted-foreground">Catatan</dt>
            <dd>{data.notes}</dd>
          </>
        )}
      </dl>
    </div>
  );
}

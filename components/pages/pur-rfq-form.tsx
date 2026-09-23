'use client';

/**
 * Request for Quotation (RFQ) form — header driven by Form Builder config
 * (PUR.RFQ fields = PURCHASE_REQUEST_FORM_FIELDS, supplier optional), plus a
 * custom supplier-invitation table (pur_rfq_suppliers).
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { Badge } from '@/components/ui/badge';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import { SearchSelect } from '@/components/molecules/search-select';
import { notify } from '@/lib/feedback';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { arrowFieldNav } from '@/lib/field-focus-nav';
import { useFormFields, buildFormConfig } from '@/lib/use-form-fields';
import { PurStructuralField, type PurStructuralFieldCtx } from '@/components/molecules/pur-structural-field';
import { loadSupplierOptions } from '@/components/pages/pur-form-lookups';
import { listCurrencies, type ErpCurrency } from '@/lib/api/currencies';
import { type ErpFormField, type FormColumnSlot } from '@/lib/api/form-fields';
import {
  createPurRfq,
  updatePurRfq,
  getPurRfq,
  transitionPurRfq,
  type ErpPurRfq,
  type PurRfqTransition,
  type CreatePurRfqPayload,
} from '@/lib/api/pur-rfqs';
import type { ErpDocumentStatus } from '@/lib/api/pur-orders';
import { type PurOrderFormData, defaultPurOrderForm, formDefaultsPatch } from './pur-order-form-model';

export interface RfqSupplierRow {
  key: string;
  supplierId: string;
  supplierLabel?: string;
  notes: string;
  lineNo: number;
}

export interface PurRfqFormData extends PurOrderFormData {
  validFrom: string;
  validTo: string;
  requisitionId: string;
  suppliers: RfqSupplierRow[];
}

export function defaultPurRfqForm(): PurRfqFormData {
  return { ...defaultPurOrderForm(), validFrom: '', validTo: '', requisitionId: '', suppliers: [newRfqSupplierRow()] };
}

export function newRfqSupplierRow(): RfqSupplierRow {
  return { key: `rs-${Date.now()}-${Math.random()}`, supplierId: '', notes: '', lineNo: 1 };
}

export function fromPurRfq(r: ErpPurRfq): PurRfqFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    docDate: r.docDate.slice(0, 10),
    dueDate: '',
    supplierId: '',
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    locationId: r.locationId ?? '',
    locationLabel: r.location?.name,
    warehouseId: '',
    payableAccountId: '',
    paymentTermId: '',
    currencyId: r.suppliers?.[0]?.rfqId ? '' : '',
    exchangeRate: '1',
    priceMode: 'TAX_EXCLUSIVE',
    description: r.description ?? '',
    referenceNo: r.referenceNo ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
    customFields: {},
    lines: [],
    validFrom: r.validFrom ? r.validFrom.slice(0, 10) : '',
    validTo: r.validTo ? r.validTo.slice(0, 10) : '',
    requisitionId: r.requisitionId ?? '',
    suppliers: r.suppliers.length
      ? r.suppliers.map((s, i) => ({
          key: `rs-${s.id ?? i}`,
          supplierId: s.supplierId,
          supplierLabel: s.supplier?.name,
          notes: s.notes ?? '',
          lineNo: s.lineNo,
        }))
      : [newRfqSupplierRow()],
  };
}

export function toPurRfqPayload(d: PurRfqFormData): CreatePurRfqPayload {
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    docDate: d.docDate,
    branchId: d.branchId,
    locationId: d.locationId || undefined,
    requisitionId: d.requisitionId || undefined,
    validFrom: d.validFrom || undefined,
    validTo: d.validTo || undefined,
    description: d.description || undefined,
    notes: d.notes || undefined,
    referenceNo: d.referenceNo || undefined,
    suppliers: d.suppliers
      .filter((s) => s.supplierId)
      .map((s, i) => ({ supplierId: s.supplierId, notes: s.notes || undefined, lineNo: i + 1 })),
  };
}

const EDITABLE: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'REJECTED'];
const SLOTS: FormColumnSlot[] = ['LEFT', 'CENTER', 'RIGHT'];

const DEFAULT_RFQ_FIELDS: ErpFormField[] = [
    { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
    { fieldKey: 'referenceNo', kind: 'STRUCTURAL', label: 'No Referensi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT' },
    { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
    { fieldKey: 'locationId', kind: 'STRUCTURAL', label: 'Lokasi', fieldType: 'LOCATION', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
    { fieldKey: 'docDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT' },
    { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  ];

export function PurRfqForm({
  data,
  onChange,
  saving,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: PurRfqFormData;
  onChange: (d: PurRfqFormData) => void;
  saving?: boolean;
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  const [currencies, setCurrencies] = React.useState<ErpCurrency[]>([]);
  const set = (p: Partial<PurRfqFormData>) => onChange({ ...data, ...p });

  const fallbackConfig = React.useMemo(() => buildFormConfig(DEFAULT_RFQ_FIELDS), []);
  const loaded = useFormFields('PUR.RFQ');
  const formConfig = Object.keys(loaded.byKey).length ? loaded : fallbackConfig;

  const defaultsApplied = React.useRef(false);
  React.useEffect(() => {
    if (data.id || defaultsApplied.current) return;
    if (Object.keys(loaded.byKey).length === 0) return;
    const patch = formDefaultsPatch(data as PurOrderFormData, loaded);
    if (Object.keys(patch).length > 0) { defaultsApplied.current = true; onChange({ ...data, ...patch }); }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loaded]);

  React.useEffect(() => {
    listCurrencies({ page: 1, limit: 100, isActive: true }).then((r) => setCurrencies(r.data));
  }, []);

  const locked = !EDITABLE.includes(data.status);
  const ph = (key: string, fallback: string) => formConfig.byKey[key]?.placeholder || fallback;
  const ro = (key: string) => locked || formConfig.byKey[key]?.isReadonly === true;
  const ctx: PurStructuralFieldCtx = {
    data: data as PurOrderFormData,
    set: (p) => set(p as Partial<PurRfqFormData>),
    ph, ro, locked,
    currencyLabel: currencies.find((c) => c.id === data.currencyId)?.code,
  };

  const renderSlot = (slot: FormColumnSlot) =>
    formConfig.slotFields[slot]
      .filter((f) => f.isVisible)
      .map((f) => f.kind === 'STRUCTURAL' ? <PurStructuralField key={f.fieldKey} field={f} ctx={ctx} /> : null);

  const addSupplier = () => onChange({
    ...data,
    suppliers: [...data.suppliers, { ...newRfqSupplierRow(), lineNo: data.suppliers.length + 1 }],
  });
  const removeSupplier = (key: string) => onChange({ ...data, suppliers: data.suppliers.filter((s) => s.key !== key) });
  const setSupplier = (key: string, patch: Partial<RfqSupplierRow>) => onChange({
    ...data,
    suppliers: data.suppliers.map((s) => s.key === key ? { ...s, ...patch } : s),
  });

  return (
    <div className="po-form flex flex-col gap-4">
      <div className="flex items-center gap-2 flex-wrap">
        <button type="button" className="btn primary" onClick={onSave} disabled={saving || locked}>
          <Icon name="save" size={13} /> Simpan
        </button>
        {!data.id && (
          <button type="button" className="btn" onClick={onSaveNew} disabled={saving || locked}>
            Simpan &amp; Baru
          </button>
        )}
        <button type="button" className="btn ghost" onClick={onReset} disabled={saving}>
          <Icon name="refresh" size={13} /> Reset
        </button>
        <div className="flex-1" />
        <Badge variant={statusBadgeVariant(data.status)} dot>{statusLabel(data.status)}</Badge>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-x-6 gap-y-3 rounded-lg border border-border p-4"
        onKeyDown={(e) => arrowFieldNav(e, {})}>
        {SLOTS.map((slot) => (
          <div key={slot} className="flex flex-col gap-3">{renderSlot(slot)}</div>
        ))}
      </div>

      {/* Validity dates */}
      <div className="flex gap-6 rounded-lg border border-border p-4">
        <div className="flex-1">
          <FormFieldRow label="Berlaku Dari">
            <DateInput value={data.validFrom} disabled={locked} onChange={(v) => set({ validFrom: v })} />
          </FormFieldRow>
        </div>
        <div className="flex-1">
          <FormFieldRow label="Berlaku Hingga">
            <DateInput value={data.validTo} disabled={locked} onChange={(v) => set({ validTo: v })} />
          </FormFieldRow>
        </div>
      </div>

      {/* Supplier invitations */}
      <div className="rounded-lg border border-border">
        <div className="flex items-center justify-between px-3 py-2 border-b border-border bg-muted/30">
          <span className="text-sm font-medium">Undangan Supplier</span>
          {!locked && (
            <button type="button" className="btn ghost sm" onClick={addSupplier}>
              <Icon name="plus" size={12} /> Tambah
            </button>
          )}
        </div>
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-border">
              <th className="px-3 py-2 text-left font-medium text-muted-foreground" style={{ width: 40 }}>#</th>
              <th className="px-3 py-2 text-left font-medium text-muted-foreground">Supplier</th>
              <th className="px-3 py-2 text-left font-medium text-muted-foreground">Catatan</th>
              {!locked && <th style={{ width: 44 }} />}
            </tr>
          </thead>
          <tbody>
            {data.suppliers.map((s, idx) => (
              <tr key={s.key} className="border-b border-border last:border-0">
                <td className="px-3 py-1.5 text-muted-foreground tabular-nums">{idx + 1}</td>
                <td className="px-3 py-1.5" style={{ minWidth: 220 }}>
                  <SearchSelect placeholder="Pilih supplier…" value={s.supplierId}
                    initialLabel={s.supplierLabel} disabled={locked}
                    loadOptions={loadSupplierOptions}
                    onValueChange={(v) => setSupplier(s.key, { supplierId: v })}
                    onPick={(o) => setSupplier(s.key, { supplierId: o.value, supplierLabel: o.label })} />
                </td>
                <td className="px-3 py-1.5">
                  <Input value={s.notes} placeholder="Catatan…" disabled={locked}
                    onChange={(e) => setSupplier(s.key, { notes: e.target.value })} />
                </td>
                {!locked && (
                  <td className="px-2 py-1.5 text-center">
                    {data.suppliers.length > 1 && (
                      <button type="button" className="iconbtn text-muted-foreground hover:text-danger"
                        onClick={() => removeSupplier(s.key)} title="Hapus baris">
                        <Icon name="trash" size={13} />
                      </button>
                    )}
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

// Re-export helpers consumed by pur-rfqs-page
export { createPurRfq, updatePurRfq, getPurRfq, transitionPurRfq };
export type { ErpPurRfq, PurRfqTransition };

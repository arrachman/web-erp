'use client';

/**
 * Bid Selection / Comparison (BS) form — header driven by Form Builder config
 * (PUR.BS fields = PURCHASE_REQUEST_FORM_FIELDS), plus a bid-evaluation lines
 * table (pur_bid_selection_lines: quotationLineId / priceRank / selected / notes).
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { notify } from '@/lib/feedback';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { arrowFieldNav } from '@/lib/field-focus-nav';
import { useFormFields, buildFormConfig } from '@/lib/use-form-fields';
import { PurStructuralField, type PurStructuralFieldCtx } from '@/components/molecules/pur-structural-field';
import { listCurrencies, type ErpCurrency } from '@/lib/api/currencies';
import { type ErpFormField, type FormColumnSlot } from '@/lib/api/form-fields';
import {
  createPurBidSelection,
  updatePurBidSelection,
  getPurBidSelection,
  transitionPurBidSelection,
  type ErpPurBidSelection,
  type PurBidSelectionTransition,
  type CreatePurBidSelectionPayload,
} from '@/lib/api/pur-bid-selections';
import type { ErpDocumentStatus } from '@/lib/api/pur-orders';
import { type PurOrderFormData, defaultPurOrderForm, formDefaultsPatch } from './pur-order-form-model';

export interface BsLineRow {
  key: string;
  quotationLineId: string;
  priceRank: number;
  selected: boolean;
  notes: string;
  lineNo: number;
}

export interface PurBsFormData extends PurOrderFormData {
  bidLines: BsLineRow[];
}

export function defaultPurBsForm(): PurBsFormData {
  return { ...defaultPurOrderForm(), bidLines: [newBsLineRow()] };
}

export function newBsLineRow(): BsLineRow {
  return { key: `bl-${Date.now()}-${Math.random()}`, quotationLineId: '', priceRank: 1, selected: false, notes: '', lineNo: 1 };
}

export function fromPurBidSelection(r: ErpPurBidSelection): PurBsFormData {
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
    currencyId: '',
    exchangeRate: '1',
    priceMode: 'TAX_EXCLUSIVE',
    description: r.description ?? '',
    referenceNo: r.referenceNo ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
    customFields: {},
    lines: [],
    bidLines: r.lines.length
      ? r.lines.map((l, i) => ({
          key: `bl-${l.id ?? i}`,
          quotationLineId: l.quotationLineId,
          priceRank: l.priceRank,
          selected: l.selected,
          notes: l.notes ?? '',
          lineNo: l.lineNo,
        }))
      : [newBsLineRow()],
  };
}

export function toPurBsPayload(d: PurBsFormData): CreatePurBidSelectionPayload {
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    docDate: d.docDate,
    branchId: d.branchId,
    locationId: d.locationId || undefined,
    description: d.description || undefined,
    notes: d.notes || undefined,
    referenceNo: d.referenceNo || undefined,
    lines: d.bidLines
      .filter((l) => l.quotationLineId)
      .map((l, i) => ({
        quotationLineId: l.quotationLineId,
        priceRank: l.priceRank,
        selected: l.selected,
        notes: l.notes || undefined,
        lineNo: i + 1,
      })),
  };
}

const EDITABLE: ErpDocumentStatus[] = ['DRAFT', 'NEED_APPROVE', 'REJECTED'];
const SLOTS: FormColumnSlot[] = ['LEFT', 'CENTER', 'RIGHT'];

const DEFAULT_BS_FIELDS: ErpFormField[] = [
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'referenceNo', kind: 'STRUCTURAL', label: 'No Referensi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'locationId', kind: 'STRUCTURAL', label: 'Lokasi', fieldType: 'LOCATION', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'docDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT' },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
];

export function PurBsForm({
  data,
  onChange,
  saving,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: PurBsFormData;
  onChange: (d: PurBsFormData) => void;
  saving?: boolean;
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  const [currencies, setCurrencies] = React.useState<ErpCurrency[]>([]);
  const set = (p: Partial<PurBsFormData>) => onChange({ ...data, ...p });

  const fallbackConfig = React.useMemo(() => buildFormConfig(DEFAULT_BS_FIELDS), []);
  const loaded = useFormFields('PUR.BS');
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
    listCurrencies({ page: 1, limit: 5, isActive: true }).then((r) => setCurrencies(r.data));
  }, []);

  const locked = !EDITABLE.includes(data.status);
  const ph = (key: string, fallback: string) => formConfig.byKey[key]?.placeholder || fallback;
  const ro = (key: string) => locked || formConfig.byKey[key]?.isReadonly === true;
  const ctx: PurStructuralFieldCtx = {
    data: data as PurOrderFormData,
    set: (p) => set(p as Partial<PurBsFormData>),
    ph, ro, locked,
    currencyLabel: currencies.find((c) => c.id === data.currencyId)?.code,
  };

  const renderSlot = (slot: FormColumnSlot) =>
    formConfig.slotFields[slot]
      .filter((f) => f.isVisible)
      .map((f) => f.kind === 'STRUCTURAL' ? <PurStructuralField key={f.fieldKey} field={f} ctx={ctx} /> : null);

  const addLine = () => onChange({
    ...data,
    bidLines: [...data.bidLines, { ...newBsLineRow(), lineNo: data.bidLines.length + 1 }],
  });
  const removeLine = (key: string) => onChange({ ...data, bidLines: data.bidLines.filter((l) => l.key !== key) });
  const setLine = (key: string, patch: Partial<BsLineRow>) => onChange({
    ...data,
    bidLines: data.bidLines.map((l) => l.key === key ? { ...l, ...patch } : l),
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

      {/* Bid evaluation lines */}
      <div className="rounded-lg border border-border">
        <div className="flex items-center justify-between px-3 py-2 border-b border-border bg-muted/30">
          <span className="text-sm font-medium">Baris Evaluasi Penawaran</span>
          {!locked && (
            <button type="button" className="btn ghost sm" onClick={addLine}>
              <Icon name="plus" size={12} /> Tambah
            </button>
          )}
        </div>
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-border">
              <th className="px-3 py-2 text-left font-medium text-muted-foreground" style={{ width: 40 }}>#</th>
              <th className="px-3 py-2 text-left font-medium text-muted-foreground">ID Baris Quotasi</th>
              <th className="px-3 py-2 text-left font-medium text-muted-foreground" style={{ width: 90 }}>Rank Harga</th>
              <th className="px-3 py-2 text-center font-medium text-muted-foreground" style={{ width: 80 }}>Dipilih</th>
              <th className="px-3 py-2 text-left font-medium text-muted-foreground">Catatan</th>
              {!locked && <th style={{ width: 44 }} />}
            </tr>
          </thead>
          <tbody>
            {data.bidLines.map((l, idx) => (
              <tr key={l.key} className="border-b border-border last:border-0">
                <td className="px-3 py-1.5 text-muted-foreground tabular-nums">{idx + 1}</td>
                <td className="px-3 py-1.5" style={{ minWidth: 180 }}>
                  <Input value={l.quotationLineId} placeholder="ID baris penawaran…" disabled={locked}
                    onChange={(e) => setLine(l.key, { quotationLineId: e.target.value })} />
                </td>
                <td className="px-3 py-1.5">
                  <Input type="number" min={1} value={l.priceRank} disabled={locked}
                    className="tabular-nums text-right"
                    onChange={(e) => setLine(l.key, { priceRank: Number(e.target.value) || 1 })} />
                </td>
                <td className="px-3 py-1.5 text-center">
                  <input type="checkbox" checked={l.selected} disabled={locked}
                    onChange={(e) => setLine(l.key, { selected: e.target.checked })} />
                </td>
                <td className="px-3 py-1.5">
                  <Input value={l.notes} placeholder="Catatan…" disabled={locked}
                    onChange={(e) => setLine(l.key, { notes: e.target.value })} />
                </td>
                {!locked && (
                  <td className="px-2 py-1.5 text-center">
                    {data.bidLines.length > 1 && (
                      <button type="button" className="iconbtn text-muted-foreground hover:text-danger"
                        onClick={() => removeLine(l.key)} title="Hapus baris">
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

export { createPurBidSelection, updatePurBidSelection, getPurBidSelection, transitionPurBidSelection };
export type { ErpPurBidSelection, PurBidSelectionTransition };

'use client';

/**
 * Bill of Materials (BOM) form — header + two-tab line grid (inputs / outputs).
 * Tab "Material Input" = komponen yang dikonsumsi.
 * Tab "Output Produksi" = produk yang dihasilkan.
 * Line grid extracted to mfg-bom-line-grid.tsx (§3 400-line limit).
 * Atomic tier: Page (form sub-view).
 */

import * as React from 'react';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { Input } from '@/components/ui/input';
import { BomLineGrid } from './mfg-bom-line-grid';
import type {
  CreateMfgBomPayload,
  ErpMfgBom,
  ErpMfgBomLine,
  MfgBomLinePayload,
} from '@/lib/api/mfg-boms';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface BomLine {
  tempId: string;
  itemId: string;
  quantity: string;
  unitId: string;
  unitPrice: string;
  unitCost: string;
  notes: string;
  lineNo: number;
}

export interface MfgBomFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  docDate: string;
  branchId: string;
  locationId: string;
  sourceWarehouseId: string;
  productionWarehouseId: string;
  destinationWarehouseId: string;
  currencyId: string;
  exchangeRate: string;
  neededDate: string;
  workEstimate: string;
  description: string;
  notes: string;
  referenceNo: string;
  inputs: BomLine[];
  outputs: BomLine[];
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

let _seq = 0;
const nextTempId = () => `tmp-${++_seq}`;

export const emptyBomLine = (lineNo: number): BomLine => ({
  tempId: nextTempId(),
  itemId: '',
  quantity: '',
  unitId: '',
  unitPrice: '',
  unitCost: '',
  notes: '',
  lineNo,
});

export function defaultMfgBomForm(): MfgBomFormData {
  const today = new Date().toISOString().slice(0, 10);
  return {
    docNumber: '',
    auto: true,
    docDate: today,
    branchId: '',
    locationId: '',
    sourceWarehouseId: '',
    productionWarehouseId: '',
    destinationWarehouseId: '',
    currencyId: '',
    exchangeRate: '1',
    neededDate: '',
    workEstimate: '',
    description: '',
    notes: '',
    referenceNo: '',
    inputs: [emptyBomLine(1)],
    outputs: [emptyBomLine(1)],
  };
}

function serverLineToForm(l: ErpMfgBomLine, idx: number): BomLine {
  return {
    tempId: l.id ?? nextTempId(),
    itemId: l.itemId,
    quantity: l.quantity,
    unitId: l.unitId,
    unitPrice: l.unitPrice,
    unitCost: l.unitCost,
    notes: l.notes ?? '',
    lineNo: l.lineNo ?? idx + 1,
  };
}

export function fromMfgBom(bom: ErpMfgBom): MfgBomFormData {
  return {
    id: bom.id,
    docNumber: bom.docNumber,
    auto: false,
    docDate: bom.docDate.slice(0, 10),
    branchId: bom.branchId,
    locationId: bom.locationId ?? '',
    sourceWarehouseId: bom.sourceWarehouseId ?? '',
    productionWarehouseId: bom.productionWarehouseId ?? '',
    destinationWarehouseId: bom.destinationWarehouseId ?? '',
    currencyId: bom.currencyId,
    exchangeRate: bom.exchangeRate,
    neededDate: bom.neededDate?.slice(0, 10) ?? '',
    workEstimate: bom.workEstimate ?? '',
    description: bom.description ?? '',
    notes: bom.notes ?? '',
    referenceNo: bom.referenceNo ?? '',
    inputs: bom.inputs.length ? bom.inputs.map(serverLineToForm) : [emptyBomLine(1)],
    outputs: bom.outputs.length ? bom.outputs.map(serverLineToForm) : [emptyBomLine(1)],
  };
}

function lineToPayload(l: BomLine, idx: number): MfgBomLinePayload {
  return {
    itemId: l.itemId,
    quantity: l.quantity || '0',
    unitId: l.unitId,
    unitPrice: l.unitPrice || undefined,
    unitCost: l.unitCost || undefined,
    notes: l.notes || undefined,
    lineNo: idx + 1,
  };
}

export function toMfgBomPayload(form: MfgBomFormData): CreateMfgBomPayload {
  return {
    docNumber: form.auto ? undefined : form.docNumber || undefined,
    auto: form.auto,
    docDate: form.docDate,
    branchId: form.branchId,
    locationId: form.locationId || undefined,
    sourceWarehouseId: form.sourceWarehouseId || undefined,
    productionWarehouseId: form.productionWarehouseId || undefined,
    destinationWarehouseId: form.destinationWarehouseId || undefined,
    currencyId: form.currencyId,
    exchangeRate: form.exchangeRate || '1',
    neededDate: form.neededDate || undefined,
    workEstimate: form.workEstimate || undefined,
    description: form.description || undefined,
    notes: form.notes || undefined,
    referenceNo: form.referenceNo || undefined,
    inputs: form.inputs.filter((l) => l.itemId).map(lineToPayload),
    outputs: form.outputs.filter((l) => l.itemId).map(lineToPayload),
  };
}

// ─── Main form ────────────────────────────────────────────────────────────────

export function MfgBomForm({
  data,
  onChange,
  saving,
  onSave,
  onSaveNew,
  onReset,
}: {
  data: MfgBomFormData;
  onChange: (d: MfgBomFormData) => void;
  saving?: boolean;
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}) {
  const set = (patch: Partial<MfgBomFormData>) => onChange({ ...data, ...patch });

  return (
    <div className="flex flex-col gap-4">
      {/* ── Header 2-col grid ─────────────────────────────────────────── */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Left column */}
        <div className="flex flex-col gap-3">
          <div className="flex items-end gap-2">
            <div className="flex-1">
              <label className="form-label">No BOM</label>
              <Input
                value={data.docNumber}
                onChange={(e) => set({ docNumber: e.target.value })}
                placeholder="Otomatis"
                disabled={data.auto}
              />
            </div>
            <label className="flex items-center gap-1.5 pb-1.5 whitespace-nowrap cursor-pointer text-sm">
              <input
                type="checkbox"
                checked={data.auto}
                onChange={(e) =>
                  set({
                    auto: e.target.checked,
                    docNumber: e.target.checked ? '' : data.docNumber,
                  })
                }
              />
              Auto
            </label>
          </div>

          <div>
            <label className="form-label">Tanggal *</label>
            <Input type="date" value={data.docDate} onChange={(e) => set({ docDate: e.target.value })} />
          </div>
          <div>
            <label className="form-label">Cabang *</label>
            <Input value={data.branchId} onChange={(e) => set({ branchId: e.target.value })} placeholder="ID cabang" />
          </div>
          <div>
            <label className="form-label">Lokasi</label>
            <Input value={data.locationId} onChange={(e) => set({ locationId: e.target.value })} placeholder="ID lokasi" />
          </div>
          <div>
            <label className="form-label">Tanggal Dibutuhkan</label>
            <Input type="date" value={data.neededDate} onChange={(e) => set({ neededDate: e.target.value })} />
          </div>
          <div>
            <label className="form-label">Estimasi Kerja (jam)</label>
            <Input value={data.workEstimate} onChange={(e) => set({ workEstimate: e.target.value })} placeholder="0" />
          </div>
        </div>

        {/* Right column */}
        <div className="flex flex-col gap-3">
          <div>
            <label className="form-label">Mata Uang *</label>
            <Input value={data.currencyId} onChange={(e) => set({ currencyId: e.target.value })} placeholder="ID mata uang" />
          </div>
          <div>
            <label className="form-label">Kurs</label>
            <Input value={data.exchangeRate} onChange={(e) => set({ exchangeRate: e.target.value })} placeholder="1" />
          </div>
          <div>
            <label className="form-label">Gudang Sumber</label>
            <Input value={data.sourceWarehouseId} onChange={(e) => set({ sourceWarehouseId: e.target.value })} placeholder="ID gudang sumber" />
          </div>
          <div>
            <label className="form-label">Gudang Produksi</label>
            <Input value={data.productionWarehouseId} onChange={(e) => set({ productionWarehouseId: e.target.value })} placeholder="ID gudang produksi" />
          </div>
          <div>
            <label className="form-label">Gudang Tujuan</label>
            <Input value={data.destinationWarehouseId} onChange={(e) => set({ destinationWarehouseId: e.target.value })} placeholder="ID gudang tujuan" />
          </div>
          <div>
            <label className="form-label">No Referensi</label>
            <Input value={data.referenceNo} onChange={(e) => set({ referenceNo: e.target.value })} placeholder="—" />
          </div>
        </div>
      </div>

      {/* Description + notes */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="form-label">Keterangan</label>
          <Input value={data.description} onChange={(e) => set({ description: e.target.value })} placeholder="Uraian BOM" />
        </div>
        <div>
          <label className="form-label">Catatan</label>
          <Input value={data.notes} onChange={(e) => set({ notes: e.target.value })} placeholder="—" />
        </div>
      </div>

      {/* ── Line tabs ──────────────────────────────────────────────────── */}
      <Tabs defaultValue="inputs">
        <TabsList>
          <TabsTrigger value="inputs">Material Input</TabsTrigger>
          <TabsTrigger value="outputs">Output Produksi</TabsTrigger>
        </TabsList>
        <TabsContent value="inputs" className="pt-3">
          <BomLineGrid
            lines={data.inputs}
            onChange={(inputs) => set({ inputs })}
            addLabel="Tambah Baris Input"
          />
        </TabsContent>
        <TabsContent value="outputs" className="pt-3">
          <BomLineGrid
            lines={data.outputs}
            onChange={(outputs) => set({ outputs })}
            addLabel="Tambah Baris Output"
          />
        </TabsContent>
      </Tabs>

      {/* ── Footer buttons ─────────────────────────────────────────────── */}
      <div className="flex items-center gap-2 pt-2 border-t border-border">
        <button type="button" className="btn primary" onClick={onSave} disabled={saving}>
          {saving ? 'Menyimpan…' : 'Simpan'}
        </button>
        <button type="button" className="btn secondary" onClick={onSaveNew} disabled={saving}>
          Simpan &amp; Baru
        </button>
        <button type="button" className="btn ghost" onClick={onReset} disabled={saving}>
          Reset
        </button>
      </div>
    </div>
  );
}

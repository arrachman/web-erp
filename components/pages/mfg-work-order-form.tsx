'use client';

/**
 * Manufacturing Work Order (WO) form — header + input/output lines.
 * Atomic tier: Page (rendered inside mfg-work-orders-page in form mode).
 * Model + factories live in mfg-work-order-form-model.ts (§3 reuse).
 */

import * as React from 'react';
import type { MfgWorkOrderFormData, MfgWorkOrderLineFormData } from './mfg-work-order-form-model';

export type { MfgWorkOrderFormData, MfgWorkOrderLineFormData } from './mfg-work-order-form-model';
export {
  defaultMfgWorkOrderForm,
  fromMfgWorkOrder,
  toMfgWorkOrderPayload,
} from './mfg-work-order-form-model';

// ─── Line row sub-component ───────────────────────────────────────────────────

interface LineRowProps {
  line: MfgWorkOrderLineFormData;
  idx: number;
  onChange: (idx: number, patch: Partial<MfgWorkOrderLineFormData>) => void;
  onRemove: (idx: number) => void;
}

function LineRow({ line, idx, onChange, onRemove }: LineRowProps) {
  return (
    <tr>
      <td style={{ verticalAlign: 'middle' }}>
        <select
          className="input input-sm"
          value={line.lineType}
          onChange={(e) => onChange(idx, { lineType: e.target.value as 'INPUT' | 'OUTPUT' })}
        >
          <option value="INPUT">Input</option>
          <option value="OUTPUT">Output</option>
        </select>
      </td>
      <td>
        <input
          className="input input-sm w-full"
          placeholder="ID Item"
          value={line.itemId}
          onChange={(e) => onChange(idx, { itemId: e.target.value })}
        />
      </td>
      <td>
        <input
          className="input input-sm w-full tabular-nums"
          placeholder="0"
          value={line.quantity}
          onChange={(e) => onChange(idx, { quantity: e.target.value })}
          style={{ textAlign: 'right' }}
        />
      </td>
      <td>
        <input
          className="input input-sm w-full"
          placeholder="ID Satuan"
          value={line.unitId}
          onChange={(e) => onChange(idx, { unitId: e.target.value })}
        />
      </td>
      <td>
        <input
          className="input input-sm w-full"
          placeholder="Keterangan"
          value={line.notes ?? ''}
          onChange={(e) => onChange(idx, { notes: e.target.value })}
        />
      </td>
      <td style={{ textAlign: 'center' }}>
        <button type="button" className="btn ghost sm danger" onClick={() => onRemove(idx)} title="Hapus baris">
          ×
        </button>
      </td>
    </tr>
  );
}

// ─── Main form component ──────────────────────────────────────────────────────

export interface MfgWorkOrderFormProps {
  data: MfgWorkOrderFormData;
  onChange: (d: MfgWorkOrderFormData) => void;
  saving?: boolean;
  onSave: () => void;
  onSaveNew: () => void;
  onReset: () => void;
}

export function MfgWorkOrderForm({
  data,
  onChange,
  saving,
  onSave,
  onSaveNew,
  onReset,
}: MfgWorkOrderFormProps) {
  const set = <K extends keyof MfgWorkOrderFormData>(
    key: K,
    value: MfgWorkOrderFormData[K],
  ) => onChange({ ...data, [key]: value });

  const addLine = (lineType: 'INPUT' | 'OUTPUT') => {
    const newLine: MfgWorkOrderLineFormData = {
      lineType,
      itemId: '',
      quantity: '1',
      unitId: '',
      notes: '',
      lineNo: data.lines.length + 1,
    };
    onChange({ ...data, lines: [...data.lines, newLine] });
  };

  const updateLine = (idx: number, patch: Partial<MfgWorkOrderLineFormData>) => {
    onChange({ ...data, lines: data.lines.map((l, i) => (i === idx ? { ...l, ...patch } : l)) });
  };

  const removeLine = (idx: number) => {
    onChange({ ...data, lines: data.lines.filter((_, i) => i !== idx) });
  };

  return (
    <div className="form-container space-y-6">
      {/* Header */}
      <div className="form-section">
        <h2 className="form-section-title">Header Work Order</h2>
        <div className="grid grid-cols-2 gap-4">
          {/* Left column */}
          <div className="space-y-3">
            <div className="form-field">
              <label className="form-label">Cabang *</label>
              <input className="input w-full" placeholder="ID Cabang" value={data.branchId} onChange={(e) => set('branchId', e.target.value)} />
            </div>
            <div className="form-field">
              <label className="form-label">Gudang Produksi</label>
              <input className="input w-full" placeholder="ID Gudang" value={data.warehouseId ?? ''} onChange={(e) => set('warehouseId', e.target.value)} />
            </div>
            <div className="form-field">
              <label className="form-label">BOM Referensi</label>
              <input className="input w-full" placeholder="No BOM / BOM Reference" value={data.bomId ?? ''} onChange={(e) => set('bomId', e.target.value)} />
            </div>
            <div className="form-field">
              <label className="form-label">Uraian</label>
              <input className="input w-full" placeholder="Keterangan dokumen" value={data.description ?? ''} onChange={(e) => set('description', e.target.value)} />
            </div>
            <div className="form-field">
              <label className="form-label">Catatan</label>
              <textarea className="input w-full" rows={2} placeholder="Catatan internal" value={data.notes ?? ''} onChange={(e) => set('notes', e.target.value)} />
            </div>
          </div>

          {/* Right column */}
          <div className="space-y-3">
            <div className="form-field">
              <label className="form-label">Tanggal *</label>
              <input className="input w-full" type="date" value={data.docDate} onChange={(e) => set('docDate', e.target.value)} />
            </div>
            <div className="form-field">
              <label className="form-label">No Work Order</label>
              <div className="flex gap-2 items-center">
                <input
                  className="input flex-1"
                  placeholder="Otomatis"
                  value={data.docNumber ?? ''}
                  disabled={data.auto}
                  onChange={(e) => set('docNumber', e.target.value)}
                />
                <label className="flex items-center gap-1 text-sm cursor-pointer">
                  <input type="checkbox" checked={data.auto} onChange={(e) => set('auto', e.target.checked)} />
                  Auto
                </label>
              </div>
            </div>
            <div className="form-field">
              <label className="form-label">No Referensi</label>
              <input className="input w-full" placeholder="No dokumen referensi" value={data.referenceNo ?? ''} onChange={(e) => set('referenceNo', e.target.value)} />
            </div>
          </div>
        </div>
      </div>

      {/* Lines */}
      <div className="form-section">
        <div className="flex items-center justify-between mb-2">
          <h2 className="form-section-title">Baris Komponen</h2>
          <div className="flex gap-2">
            <button type="button" className="btn secondary sm" onClick={() => addLine('INPUT')}>+ Input</button>
            <button type="button" className="btn secondary sm" onClick={() => addLine('OUTPUT')}>+ Output</button>
          </div>
        </div>
        <div className="overflow-x-auto">
          <table className="table w-full text-sm">
            <thead>
              <tr>
                <th style={{ width: 100 }}>Tipe</th>
                <th>Item</th>
                <th style={{ width: 100, textAlign: 'right' }}>Qty</th>
                <th style={{ width: 120 }}>Satuan</th>
                <th>Keterangan</th>
                <th style={{ width: 44 }} />
              </tr>
            </thead>
            <tbody>
              {data.lines.length === 0 ? (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '1rem' }}>
                    Belum ada baris. Klik + Input atau + Output untuk menambah.
                  </td>
                </tr>
              ) : (
                data.lines.map((line, idx) => (
                  <LineRow key={idx} line={line} idx={idx} onChange={updateLine} onRemove={removeLine} />
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Actions */}
      <div className="flex gap-3 justify-end pt-2">
        <button type="button" className="btn ghost" onClick={onReset} disabled={saving}>Reset</button>
        <button type="button" className="btn secondary" onClick={onSaveNew} disabled={saving}>Simpan &amp; Baru</button>
        <button type="button" className="btn primary" onClick={onSave} disabled={saving}>
          {saving ? 'Menyimpan…' : 'Simpan'}
        </button>
      </div>
    </div>
  );
}

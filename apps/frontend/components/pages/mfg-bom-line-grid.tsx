'use client';

/**
 * BOM line editor grid — shared for inputs and outputs tabs.
 * Atomic tier: Organism (inline editable table of BOM lines).
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { notify } from '@/lib/feedback';
import type { BomLine } from './mfg-bom-form';

export function BomLineGrid({
  lines,
  onChange,
  addLabel,
}: {
  lines: BomLine[];
  onChange: (lines: BomLine[]) => void;
  addLabel: string;
}) {
  let _seq = 0;
  const nextTempId = () => `tmp-${++_seq}-${Date.now()}`;

  const setLine = (idx: number, patch: Partial<BomLine>) =>
    onChange(lines.map((l, i) => (i === idx ? { ...l, ...patch } : l)));

  const addLine = () =>
    onChange([
      ...lines,
      {
        tempId: nextTempId(),
        itemId: '',
        quantity: '',
        unitId: '',
        unitPrice: '',
        unitCost: '',
        notes: '',
        lineNo: lines.length + 1,
      },
    ]);

  const removeLine = (idx: number) => {
    if (lines.length === 1) {
      notify('Minimal satu baris harus ada.', 'warn');
      return;
    }
    onChange(lines.filter((_, i) => i !== idx));
  };

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm border-collapse">
        <thead>
          <tr className="border-b border-border bg-muted/40">
            <th className="px-2 py-1.5 text-left font-medium text-muted-foreground w-8">#</th>
            <th className="px-2 py-1.5 text-left font-medium text-muted-foreground min-w-[160px]">
              Item
            </th>
            <th className="px-2 py-1.5 text-right font-medium text-muted-foreground w-24">Qty</th>
            <th className="px-2 py-1.5 text-left font-medium text-muted-foreground w-28">
              Satuan
            </th>
            <th className="px-2 py-1.5 text-right font-medium text-muted-foreground w-28">
              Harga
            </th>
            <th className="px-2 py-1.5 text-right font-medium text-muted-foreground w-28">
              Biaya
            </th>
            <th className="px-2 py-1.5 text-left font-medium text-muted-foreground min-w-[120px]">
              Catatan
            </th>
            <th className="px-2 py-1.5 w-8" />
          </tr>
        </thead>
        <tbody>
          {lines.map((line, idx) => (
            <tr key={line.tempId} className="border-b border-border hover:bg-muted/20">
              <td className="px-2 py-1 text-muted-foreground text-xs tabular-nums">
                {idx + 1}
              </td>
              <td className="px-2 py-1">
                <input
                  className="w-full h-7 rounded border border-input bg-background px-2 text-sm focus:outline-none focus:ring-1 focus:ring-ring"
                  placeholder="Kode atau nama item"
                  value={line.itemId}
                  onChange={(e) => setLine(idx, { itemId: e.target.value })}
                />
              </td>
              <td className="px-2 py-1">
                <input
                  className="w-full h-7 rounded border border-input bg-background px-2 text-sm text-right tabular-nums focus:outline-none focus:ring-1 focus:ring-ring"
                  placeholder="0"
                  value={line.quantity}
                  onChange={(e) => setLine(idx, { quantity: e.target.value })}
                />
              </td>
              <td className="px-2 py-1">
                <input
                  className="w-full h-7 rounded border border-input bg-background px-2 text-sm focus:outline-none focus:ring-1 focus:ring-ring"
                  placeholder="Satuan"
                  value={line.unitId}
                  onChange={(e) => setLine(idx, { unitId: e.target.value })}
                />
              </td>
              <td className="px-2 py-1">
                <input
                  className="w-full h-7 rounded border border-input bg-background px-2 text-sm text-right tabular-nums focus:outline-none focus:ring-1 focus:ring-ring"
                  placeholder="0"
                  value={line.unitPrice}
                  onChange={(e) => setLine(idx, { unitPrice: e.target.value })}
                />
              </td>
              <td className="px-2 py-1">
                <input
                  className="w-full h-7 rounded border border-input bg-background px-2 text-sm text-right tabular-nums focus:outline-none focus:ring-1 focus:ring-ring"
                  placeholder="0"
                  value={line.unitCost}
                  onChange={(e) => setLine(idx, { unitCost: e.target.value })}
                />
              </td>
              <td className="px-2 py-1">
                <input
                  className="w-full h-7 rounded border border-input bg-background px-2 text-sm focus:outline-none focus:ring-1 focus:ring-ring"
                  placeholder="—"
                  value={line.notes}
                  onChange={(e) => setLine(idx, { notes: e.target.value })}
                />
              </td>
              <td className="px-2 py-1 text-center">
                <button
                  type="button"
                  className="iconbtn text-muted-foreground hover:text-danger cursor-pointer"
                  onClick={() => removeLine(idx)}
                  title="Hapus baris"
                >
                  <Icon name="x" size={12} />
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <button type="button" className="btn ghost sm mt-2" onClick={addLine}>
        <Icon name="plus" size={12} /> {addLabel}
      </button>
    </div>
  );
}

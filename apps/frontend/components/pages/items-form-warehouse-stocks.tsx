'use client';

/**
 * Per-warehouse stock override editor — multi-row (Gudang + Stok Min + Stok
 * Maks + Min Order). Nilai global di section Inventory tetap jadi default;
 * baris di sini meng-override untuk satu gudang. Atomic tier: Organism.
 */

import * as React from 'react';
import { SearchSelect } from '@/components/molecules/search-select';
import { NumInput } from '@/components/molecules/num-input';
import { loadWarehouseOptions } from './items-form-lookups';
import type { ItemWarehouseStockFormRow } from './items-form';

export function ItemWarehouseStocksEditor({
  rows, onChange,
}: { rows: ItemWarehouseStockFormRow[]; onChange: (rows: ItemWarehouseStockFormRow[]) => void }) {
  const nextKey = React.useRef(0);

  const addRow = () => onChange([
    ...rows,
    { key: `new-${nextKey.current++}`, warehouseId: '', minStock: '', maxStock: '', minOrderQty: '' },
  ]);
  const removeRow = (key: string) => onChange(rows.filter((r) => r.key !== key));
  const setRow = (key: string, patch: Partial<ItemWarehouseStockFormRow>) =>
    onChange(rows.map((r) => (r.key === key ? { ...r, ...patch } : r)));

  return (
    <div className="col-span-2 py-1">
      <div className="overflow-hidden rounded-[var(--radius)] border border-border">
        <table className="w-full text-xs">
          <thead>
            <tr className="border-b border-border bg-[var(--panel-2)] text-left text-[var(--fg-muted)]">
              <th className="w-10 px-2 py-1.5 font-medium">No</th>
              <th className="px-2 py-1.5 font-medium">Gudang</th>
              <th className="w-28 px-2 py-1.5 font-medium">Stok Min</th>
              <th className="w-28 px-2 py-1.5 font-medium">Stok Maks</th>
              <th className="w-28 px-2 py-1.5 font-medium">Min Order</th>
              <th className="w-9 px-2 py-1.5" aria-label="Aksi" />
            </tr>
          </thead>
          <tbody>
            {rows.length === 0 && (
              <tr>
                <td colSpan={6} className="px-2 py-3 text-center text-[var(--fg-subtle)]">
                  Belum ada pengaturan per gudang. Semua gudang memakai nilai global di atas.
                </td>
              </tr>
            )}
            {rows.map((row, i) => (
              <tr key={row.key} className="border-b border-border last:border-b-0">
                <td className="px-2 py-1 text-center tabular-nums text-[var(--fg-muted)]">{i + 1}</td>
                <td className="px-2 py-1">
                  <SearchSelect
                    id={`iws-wh-${row.key}`}
                    value={row.warehouseId}
                    onValueChange={(v) => setRow(row.key, { warehouseId: v })}
                    placeholder="Pilih gudang…"
                    loadOptions={loadWarehouseOptions}
                    initialLabel={row.warehouseLabel}
                    title="Gudang"
                  />
                </td>
                <td className="px-2 py-1">
                  <NumInput id={`iws-min-${row.key}`} value={row.minStock} onChange={(v) => setRow(row.key, { minStock: v })} placeholder="Global" />
                </td>
                <td className="px-2 py-1">
                  <NumInput id={`iws-max-${row.key}`} value={row.maxStock} onChange={(v) => setRow(row.key, { maxStock: v })} placeholder="Global" />
                </td>
                <td className="px-2 py-1">
                  <NumInput id={`iws-mo-${row.key}`} value={row.minOrderQty} onChange={(v) => setRow(row.key, { minOrderQty: v })} placeholder="Global" />
                </td>
                <td className="px-2 py-1 text-center">
                  <button
                    type="button"
                    onClick={() => removeRow(row.key)}
                    className="rounded-[var(--radius)] px-1.5 py-0.5 text-[var(--fg-muted)] hover:bg-[var(--panel-hover)] hover:text-danger"
                    title="Hapus baris"
                    aria-label={`Hapus baris ${i + 1}`}
                  >
                    ✕
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <button
        type="button"
        onClick={addRow}
        className="mt-2 inline-flex items-center gap-1 rounded-[var(--radius)] border border-border bg-[var(--panel-2)] px-2.5 py-1 text-[11px] font-medium hover:bg-[var(--panel-hover)]"
      >
        + Tambah
      </button>
    </div>
  );
}

'use client';

import * as React from 'react';
import { Checkbox } from '@/components/ui/checkbox';
import { Icon } from '@/components/ui/icons';
import type { ErpGridColumn } from '@/lib/api/transaction-grids';

export type BooleanColumnFlag = 'isVisible' | 'isRequired' | 'isEditable' | 'isSkippable';

interface GridBulkToolbarProps {
  total: number;
  totalSelectableCols?: number;
  selectedCells: Set<string>;
  onSelectAll: (checked: boolean) => void;
  onBulkToggle: (flag: BooleanColumnFlag, value: boolean) => void;
  onBulkReset: () => void;
  columns: ErpGridColumn[];
}

const FLAG_LABELS: { flag: BooleanColumnFlag; label: string }[] = [
  { flag: 'isVisible', label: 'Tampil' },
  { flag: 'isRequired', label: 'Wajib' },
  { flag: 'isEditable', label: 'Edit' },
  { flag: 'isSkippable', label: 'Skip' },
];

export function GridBulkToolbar({
  total,
  totalSelectableCols,
  selectedCells,
  onSelectAll,
  onBulkToggle,
  onBulkReset,
  columns,
}: GridBulkToolbarProps) {
  const count = selectedCells.size;
  const totalCells = total * (totalSelectableCols ?? FLAG_LABELS.length);
  const allChecked = count === totalCells && totalCells > 0;
  const someChecked = count > 0 && count < totalCells;

  const allFlagOn = (flag: BooleanColumnFlag) => {
    const inFlag = [...selectedCells].filter((k) => k.endsWith(`:${flag}`));
    if (inFlag.length === 0) return false;
    return inFlag.every((k) => {
      const rowIdx = parseInt(k.split(':')[0], 10);
      return columns[rowIdx]?.[flag] === true;
    });
  };

  const countForFlag = (flag: BooleanColumnFlag) =>
    [...selectedCells].filter((k) => k.endsWith(`:${flag}`)).length;

  return (
    <div className="flex items-center gap-3 border-b border-border bg-secondary/60 px-3 py-1.5">
      <Checkbox
        checked={allChecked ? true : someChecked ? 'indeterminate' : false}
        onCheckedChange={(v) => onSelectAll(v === true)}
        title="Pilih semua sel"
      />

      {count === 0 ? (
        <span className="text-xs text-muted-foreground">
          Klik sel Tampil/Wajib/Edit/Skip untuk pilih, atau klik header kolom
        </span>
      ) : (
        <>
          <span className="text-xs font-medium text-foreground">{count} sel dipilih</span>
          <div className="h-4 w-px bg-border" />
          {FLAG_LABELS.map(({ flag, label }) => {
            const n = countForFlag(flag);
            if (n === 0) return null;
            const on = allFlagOn(flag);
            return (
              <button
                key={flag}
                type="button"
                className="btn text-xs"
                title={`${on ? 'Matikan' : 'Aktifkan'} ${label} pada ${n} kolom`}
                onClick={() => onBulkToggle(flag, !on)}
              >
                <Icon name="check" size={12} />
                {on ? `Off ${label}` : label}
                {n > 1 && <span className="ml-0.5 opacity-60">×{n}</span>}
              </button>
            );
          })}
          <div className="h-4 w-px bg-border" />
          <button
            type="button"
            className="btn text-xs text-warning"
            title="Reset baris terpilih ke nilai default"
            onClick={onBulkReset}
          >
            <Icon name="refresh" size={12} /> Reset
          </button>
          <button
            type="button"
            className="iconbtn ml-auto"
            title="Batal pilihan"
            onClick={() => onSelectAll(false)}
          >
            <Icon name="x" size={13} />
          </button>
        </>
      )}
    </div>
  );
}

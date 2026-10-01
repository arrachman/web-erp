'use client';

/**
 * Clearing detail for giro clearing transactions (RGC/SGC — kind=CLEAR). Rows are
 * PICKED from outstanding giros (not free-typed): No · No Giro · Bank · Jatuh
 * Tempo · Nominal (read-only, from the picked giro) · Tgl Cair (editable, defaults
 * to header entryDate) · remove. "+ Tambah Giro" opens the outstanding-giro
 * picker. Footer Σ Nominal. Atomic tier: Organism.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { DateInput } from '@/components/ui/date-input';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
} from '@/components/organisms/table';
import { formatNumber } from '@/lib/format';
import { GiroPickerDialog } from './giro-picker-dialog';
import type { GiroType, OutstandingGiro } from '@/lib/api/fin-giro-entries';

export interface GiroClearingRow {
  giroId: string;
  giroNumber: string;
  bankName?: string | null;
  dueDate: string;
  amount: string;
  clearedDate: string;
}

/** Σ amount across all clearing rows. */
export function computeClearingTotal(rows: GiroClearingRow[]): number {
  return rows.reduce((s, r) => s + Number(r.amount || 0), 0);
}

export function GiroClearingLines({
  type,
  rows,
  defaultClearedDate,
  onChange,
  readOnly = false,
  onValidityChange,
}: {
  type: GiroType;
  rows: GiroClearingRow[];
  /** Header entryDate — used as default Tgl Cair for newly picked giros. */
  defaultClearedDate: string;
  onChange: (rows: GiroClearingRow[]) => void;
  readOnly?: boolean;
  /** Reports clearing rows still missing a Tgl Cair (by giroNumber). */
  onValidityChange?: (missing: string[]) => void;
}) {
  const [pickerOpen, setPickerOpen] = React.useState(false);

  const excludeIds = React.useMemo(() => rows.map((r) => r.giroId), [rows]);

  const missing = React.useMemo(
    () => rows.filter((r) => !r.clearedDate).map((r) => r.giroNumber),
    [rows],
  );
  React.useEffect(() => { onValidityChange?.(missing); }, [missing, onValidityChange]);

  const addGiros = (giros: OutstandingGiro[]) => {
    const next: GiroClearingRow[] = giros.map((g) => ({
      giroId: g.id,
      giroNumber: g.giroNumber,
      bankName: g.bankName ?? null,
      dueDate: g.dueDate,
      amount: g.amount,
      clearedDate: defaultClearedDate || '',
    }));
    onChange([...rows, ...next]);
  };

  const setClearedDate = (giroId: string, clearedDate: string) =>
    onChange(rows.map((r) => (r.giroId === giroId ? { ...r, clearedDate } : r)));

  const removeRow = (giroId: string) =>
    onChange(rows.filter((r) => r.giroId !== giroId));

  const total = computeClearingTotal(rows);

  return (
    <div className="giro-clearing-lines">
      {!readOnly && (
        <div className="mb-2 flex justify-end">
          <button type="button" className="btn sm" onClick={() => setPickerOpen(true)}>
            <Icon name="plus" size={12} /> Tambah Giro
          </button>
        </div>
      )}

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 48 }}>No</TableHead>
            <TableHead>No Giro</TableHead>
            <TableHead>Bank</TableHead>
            <TableHead>Jatuh Tempo</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Nominal</TableHead>
            <TableHead style={{ width: 180 }}>Tgl Cair</TableHead>
            {!readOnly && <TableHead style={{ width: 44 }} />}
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableEmpty colSpan={readOnly ? 6 : 7} />
          ) : (
            rows.map((r, i) => (
              <TableRow key={r.giroId}>
                <TableCell style={{ textAlign: 'center' }}>{i + 1}</TableCell>
                <TableCell className="mono">{r.giroNumber}</TableCell>
                <TableCell>{r.bankName ?? '—'}</TableCell>
                <TableCell>{r.dueDate.slice(0, 10)}</TableCell>
                <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                  {formatNumber(Number(r.amount || 0), 2)}
                </TableCell>
                <TableCell>
                  <DateInput
                    value={r.clearedDate}
                    disabled={readOnly}
                    onChange={(v) => setClearedDate(r.giroId, v)}
                  />
                </TableCell>
                {!readOnly && (
                  <TableCell style={{ textAlign: 'center' }}>
                    <button
                      type="button"
                      className="iconbtn"
                      title="Hapus"
                      onClick={() => removeRow(r.giroId)}
                    >
                      <Icon name="trash" size={13} />
                    </button>
                  </TableCell>
                )}
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>

      <div className="flex flex-wrap justify-end gap-6 pt-3 pr-2 text-sm">
        <span>Σ Nominal <strong className="tabular-nums ml-2">{formatNumber(total, 2)}</strong></span>
      </div>

      <GiroPickerDialog
        open={pickerOpen}
        onOpenChange={setPickerOpen}
        type={type}
        excludeIds={excludeIds}
        onConfirm={addGiros}
      />
    </div>
  );
}

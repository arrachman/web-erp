'use client';

/**
 * Giro instrument editor for bank transactions (the "Giro" tab of Bank Keluar /
 * Bank Masuk). Atomic tier: Organism. Each row = one giro/cek persisted to
 * fin_giros via the shared cash-bank backend (`giros` payload). Cash (Kas)
 * transactions never render this — only the bank variant mounts the tab.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { NumInput } from '@/components/molecules/num-input';
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

export interface GiroRow {
  key: string;
  id?: string;
  giroNumber: string;
  bankName: string;
  bankAccountNo: string;
  amount: string;
  dueDate: string;
  notes: string;
}

let seq = 0;
export const newGiro = (): GiroRow => ({
  key: `ng-${(seq += 1)}`,
  giroNumber: '',
  bankName: '',
  bankAccountNo: '',
  amount: '',
  dueDate: '',
  notes: '',
});

export function CashBankGirosEditor({
  giros,
  onChange,
  readOnly,
}: {
  giros: GiroRow[];
  onChange: (rows: GiroRow[]) => void;
  readOnly?: boolean;
}) {
  const set = (i: number, patch: Partial<GiroRow>) =>
    onChange(giros.map((g, idx) => (idx === i ? { ...g, ...patch } : g)));
  const remove = (i: number) => onChange(giros.filter((_, idx) => idx !== i));
  const add = () => onChange([...giros, newGiro()]);

  const total = giros.reduce((s, g) => s + Number(g.amount || 0), 0);

  return (
    <div className="flex flex-col gap-3">
      {!readOnly && (
        <div className="flex justify-end">
          <button type="button" className="btn sm" onClick={add}>
            <Icon name="plus" size={12} /> Tambah Giro
          </button>
        </div>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36, textAlign: 'right' }}>No</TableHead>
            <TableHead>No Giro</TableHead>
            <TableHead>Bank</TableHead>
            <TableHead>No Rekening</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Jumlah</TableHead>
            <TableHead>Jatuh Tempo</TableHead>
            <TableHead>Catatan</TableHead>
            {!readOnly && <TableHead style={{ width: 40 }} />}
          </TableRow>
        </TableHeader>
        <TableBody>
          {giros.length === 0 ? (
            <TableEmpty colSpan={readOnly ? 7 : 8} title="Belum ada giro" />
          ) : (
            giros.map((g, i) => (
              <TableRow key={g.key}>
                <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                  {i + 1}
                </TableCell>
                <TableCell>
                  <Input
                    value={g.giroNumber}
                    disabled={readOnly}
                    onChange={(e) => set(i, { giroNumber: e.target.value })}
                  />
                </TableCell>
                <TableCell>
                  <Input
                    value={g.bankName}
                    disabled={readOnly}
                    onChange={(e) => set(i, { bankName: e.target.value })}
                  />
                </TableCell>
                <TableCell>
                  <Input
                    value={g.bankAccountNo}
                    disabled={readOnly}
                    onChange={(e) => set(i, { bankAccountNo: e.target.value })}
                  />
                </TableCell>
                <TableCell>
                  <NumInput
                    value={g.amount}
                    decimals={2}
                    disabled={readOnly}
                    style={{ textAlign: 'right' }}
                    onChange={(raw) => set(i, { amount: raw })}
                  />
                </TableCell>
                <TableCell>
                  <DateInput
                    value={g.dueDate}
                    disabled={readOnly}
                    onChange={(v) => set(i, { dueDate: v })}
                  />
                </TableCell>
                <TableCell>
                  <Input
                    value={g.notes}
                    disabled={readOnly}
                    onChange={(e) => set(i, { notes: e.target.value })}
                  />
                </TableCell>
                {!readOnly && (
                  <TableCell style={{ textAlign: 'center' }}>
                    <button
                      type="button"
                      className="iconbtn danger"
                      title="Hapus giro"
                      onClick={() => remove(i)}
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
      <div className="flex justify-end items-center gap-3 border-t border-border pt-2">
        <span className="text-sm text-muted-foreground">Total Giro</span>
        <span className="text-sm font-semibold tabular-nums">{formatNumber(total, 2)}</span>
      </div>
    </div>
  );
}

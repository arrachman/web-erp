'use client';

/**
 * Shared journal-lines editor for M2 Finance transaction forms.
 * Atomic tier: Organism sub-part.
 *
 * All header+lines transactions in M2 (cash receipts, disbursements,
 * giros, adjustment journals, …) share the same line shape
 * (`ErpJournalLine`). This component owns the +/− line editor + balance
 * footer so each form file stays tiny and consistent.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import type { ErpJournalLine } from '@/lib/api/fin-journal-entries';

export const emptyJournalLine = (lineNo: number): ErpJournalLine => ({
  accountId: '',
  currencyId: '1',
  exchangeRate: '1.000000',
  debit: '0.0000',
  credit: '0.0000',
  lineNo,
});

export function JournalLinesEditor({
  lines,
  onChange,
}: {
  lines: ErpJournalLine[];
  onChange: (lines: ErpJournalLine[]) => void;
}) {
  const updateLine = (idx: number, patch: Partial<ErpJournalLine>) => {
    onChange(lines.map((l, i) => (i === idx ? { ...l, ...patch } : l)));
  };
  const addLine = () => onChange([...lines, emptyJournalLine(lines.length + 1)]);
  const removeLine = (idx: number) =>
    onChange(lines.filter((_, i) => i !== idx));

  const totalDebit = lines.reduce((s, l) => s + Number(l.debit || 0), 0);
  const totalCredit = lines.reduce((s, l) => s + Number(l.credit || 0), 0);

  return (
    <div style={{ marginTop: 16 }}>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 8,
        }}
      >
        <strong>Baris Jurnal</strong>
        <button className="btn sm" type="button" onClick={addLine}>
          + Tambah Baris
        </button>
      </div>
      <table style={{ width: '100%', fontSize: '0.85em' }}>
        <thead>
          <tr>
            <th style={{ textAlign: 'left' }}>No</th>
            <th style={{ textAlign: 'left' }}>Account ID</th>
            <th style={{ textAlign: 'left' }}>Debit</th>
            <th style={{ textAlign: 'left' }}>Credit</th>
            <th style={{ textAlign: 'left' }}>Notes</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {lines.length === 0 ? (
            <tr>
              <td colSpan={6} style={{ color: '#888', padding: 8 }}>
                Belum ada baris. Tambah baris untuk mengisi jurnal.
              </td>
            </tr>
          ) : (
            lines.map((l, i) => (
              <tr key={i}>
                <td>{i + 1}</td>
                <td>
                  <Input
                    value={l.accountId}
                    onChange={(e) =>
                      updateLine(i, { accountId: e.target.value })
                    }
                  />
                </td>
                <td>
                  <Input
                    value={l.debit}
                    onChange={(e) => updateLine(i, { debit: e.target.value })}
                  />
                </td>
                <td>
                  <Input
                    value={l.credit}
                    onChange={(e) => updateLine(i, { credit: e.target.value })}
                  />
                </td>
                <td>
                  <Input
                    value={l.notes ?? ''}
                    onChange={(e) =>
                      updateLine(i, { notes: e.target.value })
                    }
                  />
                </td>
                <td>
                  <button
                    className="btn sm danger"
                    type="button"
                    onClick={() => removeLine(i)}
                  >
                    ×
                  </button>
                </td>
              </tr>
            ))
          )}
        </tbody>
        <tfoot>
          <tr>
            <td colSpan={2} style={{ textAlign: 'right', fontWeight: 600 }}>
              Total
            </td>
            <td className="mono">{totalDebit.toFixed(4)}</td>
            <td className="mono">{totalCredit.toFixed(4)}</td>
            <td colSpan={2} className="muted">
              {totalDebit === totalCredit
                ? 'Balanced'
                : `Selisih ${(totalDebit - totalCredit).toFixed(4)}`}
            </td>
          </tr>
        </tfoot>
      </table>
    </div>
  );
}

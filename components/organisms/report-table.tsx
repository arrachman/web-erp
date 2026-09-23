'use client';

/**
 * Generic report table + summary bar. Renders an arbitrary `ReportDataset`:
 * columns drive the header, each cell is formatted by its column `type`
 * (money→formatRupiah, qty→formatQty, number→formatNumber, percent→`n%`,
 * date→formatDate, status→status Badge, text→raw). Numeric columns are
 * right-aligned. The summary list renders as a footer bar.
 *
 * Atomic tier: Organism.
 */

import * as React from 'react';
import {
  Table,
  TableBody,
  TableHead,
  TableHeader,
  TableRow,
  TableCell,
  TableEmpty,
} from '@/components/organisms/table';
import { StatusBadge } from '@/components/ui/badge';
import { formatNumber, formatQty, formatRupiah } from '@/lib/format';
import { formatDate } from '@/lib/date-format';
import { statusLabel } from '@/lib/status';
import { tGlobal } from '@/lib/mock';
import type {
  ReportCellType,
  ReportColumn,
  ReportDataset,
  ReportSummaryItem,
} from '@/lib/api/inv-reports';

const NUMERIC_TYPES: ReportCellType[] = ['number', 'money', 'qty', 'percent'];

function isNumericType(type: ReportCellType): boolean {
  return NUMERIC_TYPES.includes(type);
}

function toNumber(value: unknown): number | null {
  if (value === null || value === undefined || value === '') return null;
  const n = typeof value === 'number' ? value : Number(value);
  return Number.isFinite(n) ? n : null;
}

/** Format a raw cell value for display per column type. Returns a node. */
function renderCell(value: unknown, type: ReportCellType): React.ReactNode {
  if (value === null || value === undefined || value === '') {
    return type === 'status' ? null : '—';
  }
  switch (type) {
    case 'money':
      return formatRupiah(toNumber(value));
    case 'qty':
      return formatQty(toNumber(value));
    case 'number':
      return formatNumber(toNumber(value));
    case 'percent': {
      const n = toNumber(value);
      return n === null ? '—' : `${formatNumber(n)}%`;
    }
    case 'date':
      return formatDate(String(value));
    case 'status': {
      const s = String(value);
      return <StatusBadge status={s} label={statusLabel(s)} />;
    }
    default:
      return String(value);
  }
}

/** Format a summary item value per its (optional) type. */
function renderSummaryValue(item: ReportSummaryItem): React.ReactNode {
  return renderCell(item.value, item.type ?? 'text');
}

interface ReportTableProps {
  dataset: ReportDataset;
  searchTerm?: string;
}

function SummaryBar({ summary }: { summary: ReportSummaryItem[] }) {
  if (!summary || summary.length === 0) return null;
  return (
    <div className="flex flex-wrap items-center gap-x-6 gap-y-1.5 border-t border-border bg-secondary px-3 py-2 text-xs">
      {summary.map((item, i) => (
        <span key={`${item.label}-${i}`} className="flex items-center gap-1.5">
          <span className="text-muted-foreground">{tGlobal(item.label)}:</span>
          <strong className="font-mono tabular-nums text-foreground">
            {renderSummaryValue(item)}
          </strong>
        </span>
      ))}
    </div>
  );
}

export function ReportTable({ dataset, searchTerm }: ReportTableProps) {
  const { columns, rows } = dataset;
  const colCount = columns.length || 1;

  return (
    <div className="flex min-h-0 flex-1 flex-col">
      <div className="min-h-0 flex-1 overflow-auto">
        <Table>
          <TableHeader>
            <TableRow>
              {columns.map((col: ReportColumn) => (
                <TableHead
                  key={col.key}
                  numeric={col.align === 'right' || (!col.align && isNumericType(col.type))}
                >
                  {tGlobal(col.header)}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {rows.length === 0 ? (
              <TableEmpty
                colSpan={colCount}
                variant={searchTerm ? 'filtered' : 'empty'}
                searchTerm={searchTerm}
              />
            ) : (
              rows.map((row, ri) => (
                <TableRow key={ri}>
                  {columns.map((col) => {
                    const numeric =
                      col.align === 'right' || (!col.align && isNumericType(col.type));
                    return (
                      <TableCell
                        key={col.key}
                        numeric={numeric}
                        className={col.align === 'center' ? 'text-center' : undefined}
                      >
                        {renderCell(row[col.key], col.type)}
                      </TableCell>
                    );
                  })}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
      <SummaryBar summary={dataset.summary} />
    </div>
  );
}

'use client';

/**
 * Generic financial-report viewer. Pure presentational — renders any
 * {@link ReportDocument} (Trial Balance, Income Statement, Balance Sheet,
 * General Ledger) returned by `lib/api/fin-reports`.
 *
 * Atomic tier: Organism.
 */

import * as React from 'react';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
} from '@/components/organisms/table';
import { formatNumber } from '@/lib/format';
import type {
  ReportColumn,
  ReportDocument,
  ReportRow,
} from '@/lib/api/fin-reports';

const INDENT_STEP = 16; // px per indent level

function alignClass(col: ReportColumn): string {
  if (col.align === 'right') return 'text-right';
  if (col.align === 'center') return 'text-center';
  if (col.align === 'left') return 'text-left';
  // Fall back to type-based default.
  if (col.type === 'number') return 'text-right';
  return 'text-left';
}

/** Render a single cell value per its column type. */
function renderCell(
  value: string | number | null | undefined,
  col: ReportColumn,
): React.ReactNode {
  if (value === null || value === undefined || value === '') return '';
  if (col.type === 'number') {
    const num = typeof value === 'string' ? Number(value) : value;
    if (Number.isFinite(num)) return formatNumber(num, 2);
  }
  return String(value);
}

interface DataRowProps {
  row: ReportRow;
  columns: ReportColumn[];
  variant?: 'normal' | 'subtotal' | 'grand';
}

function DataRow({ row, columns, variant = 'normal' }: DataRowProps) {
  const bold = row.bold || variant !== 'normal';
  const indentPx = (row.indent ?? 0) * INDENT_STEP;
  return (
    <TableRow>
      {columns.map((col, idx) => {
        const numeric = col.type === 'number' || col.align === 'right';
        const isFirst = idx === 0;
        return (
          <TableCell
            key={col.key}
            numeric={numeric}
            truncate={false}
            className={[
              alignClass(col),
              bold ? 'font-semibold text-foreground' : '',
              variant === 'subtotal' ? 'border-t border-border' : '',
              variant === 'grand'
                ? 'border-t-2 border-[var(--border-strong,var(--border))]'
                : '',
            ]
              .filter(Boolean)
              .join(' ')}
            style={
              isFirst && indentPx ? { paddingLeft: indentPx + 10 } : undefined
            }
          >
            {renderCell(row.cells[col.key], col)}
          </TableCell>
        );
      })}
    </TableRow>
  );
}

export function ReportView({ doc }: { doc: ReportDocument }) {
  const colCount = doc.columns.length;
  const hasRows = doc.sections.some((s) => s.rows.length > 0);

  return (
    <div className="flex flex-col gap-3 p-4">
      {/* Title + subtitle + meta */}
      <header className="space-y-1">
        <h2 className="text-sm font-semibold text-foreground">{doc.title}</h2>
        {doc.subtitle && (
          <p className="text-xs text-muted-foreground">{doc.subtitle}</p>
        )}
        {doc.meta.length > 0 && (
          <dl className="flex flex-wrap gap-x-4 gap-y-0.5 text-[11.5px] text-muted-foreground">
            {doc.meta.map((m) => (
              <div key={m.label} className="flex gap-1">
                <dt>{m.label}:</dt>
                <dd className="text-foreground">{m.value}</dd>
              </div>
            ))}
          </dl>
        )}
      </header>

      <div className="lines">
        <Table>
          <TableHeader>
            <TableRow>
              {doc.columns.map((col) => (
                <TableHead
                  key={col.key}
                  numeric={col.type === 'number' || col.align === 'right'}
                  truncate={false}
                  className={alignClass(col)}
                  style={col.width ? { width: col.width } : undefined}
                >
                  {col.label}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {!hasRows ? (
              <TableRow>
                <TableCell
                  colSpan={colCount}
                  truncate={false}
                  className="py-10 text-center text-xs text-muted-foreground"
                >
                  Tidak ada data untuk periode ini.
                </TableCell>
              </TableRow>
            ) : (
              doc.sections.map((section, si) => (
                <React.Fragment key={section.heading ?? `sec-${si}`}>
                  {section.heading && (
                    <TableRow>
                      <TableCell
                        colSpan={colCount}
                        truncate={false}
                        className="bg-secondary font-semibold text-foreground"
                      >
                        {section.heading}
                      </TableCell>
                    </TableRow>
                  )}
                  {section.rows.map((row, ri) => (
                    <DataRow
                      key={`r-${si}-${ri}`}
                      row={row}
                      columns={doc.columns}
                    />
                  ))}
                  {section.subtotal && (
                    <DataRow
                      row={section.subtotal}
                      columns={doc.columns}
                      variant="subtotal"
                    />
                  )}
                </React.Fragment>
              ))
            )}
            {doc.grandTotal && hasRows && (
              <DataRow
                row={doc.grandTotal}
                columns={doc.columns}
                variant="grand"
              />
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}

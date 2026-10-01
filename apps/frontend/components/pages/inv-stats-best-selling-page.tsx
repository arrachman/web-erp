'use client';

/**
 * Warehouse Statistics — Best Selling Products (ranked table by qty).
 * Source: `GET /inv/stats/best-selling`.
 *
 * Atomic tier: Page.
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
import { StatPageShell, useStatData } from '@/components/organisms/stat-page-shell';
import { getBestSelling, type BestSellingRow } from '@/lib/api/inv-stats';
import { formatNumber } from '@/lib/format';

export function InvStatsBestSellingPage() {
  const { data, loading, error } = useStatData<BestSellingRow[]>(() =>
    getBestSelling(),
  );
  const rows = data ?? [];

  return (
    <StatPageShell
      title="Best Selling Products"
      code="inv/stats/best-selling"
      loading={loading}
      error={error}
      empty={rows.length === 0}
    >
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-10" numeric>
              #
            </TableHead>
            <TableHead>Kode</TableHead>
            <TableHead>Nama Item</TableHead>
            <TableHead numeric>Qty Terjual</TableHead>
            <TableHead>Satuan</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((row, i) => (
            <TableRow key={row.itemId}>
              <TableCell numeric>{formatNumber(i + 1, 0)}</TableCell>
              <TableCell className="mono tabular-nums">{row.itemCode}</TableCell>
              <TableCell>{row.itemName}</TableCell>
              <TableCell numeric>{formatNumber(row.qty, 0)}</TableCell>
              <TableCell>{row.unitName ?? '—'}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </StatPageShell>
  );
}

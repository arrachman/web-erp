'use client';

/**
 * Warehouse Statistics — Top Revenue Products (ranked table).
 * Source: `GET /inv/stats/top-revenue`.
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
import { getTopRevenue, type TopRevenueRow } from '@/lib/api/inv-stats';
import { formatNumber, formatRupiah } from '@/lib/format';

export function InvStatsTopRevenuePage() {
  const { data, loading, error } = useStatData<TopRevenueRow[]>(() =>
    getTopRevenue(),
  );
  const rows = data ?? [];

  return (
    <StatPageShell
      title="Top Revenue Products"
      code="inv/stats/top-revenue"
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
            <TableHead numeric>Pendapatan</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((row, i) => (
            <TableRow key={row.itemId}>
              <TableCell numeric>{formatNumber(i + 1, 0)}</TableCell>
              <TableCell className="mono tabular-nums">{row.itemCode}</TableCell>
              <TableCell>{row.itemName}</TableCell>
              <TableCell numeric>{formatRupiah(row.revenue)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </StatPageShell>
  );
}

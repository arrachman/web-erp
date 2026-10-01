'use client';

/**
 * Warehouse Statistics — Most Profitable Products (ranked table).
 * Source: `GET /inv/stats/most-profitable` (data[] + top-level `note`).
 * marginPct rendered as "12,5%"; the note is a muted caption above the table.
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
import { getMostProfitable, type MostProfitableRow } from '@/lib/api/inv-stats';
import { formatNumber, formatRupiah } from '@/lib/format';

interface ProfitData {
  rows: MostProfitableRow[];
  note?: string;
}

export function InvStatsMostProfitablePage() {
  const { data, loading, error } = useStatData<ProfitData>(() =>
    getMostProfitable(),
  );
  const rows = data?.rows ?? [];

  return (
    <StatPageShell
      title="Most Profitable Products"
      code="inv/stats/most-profitable"
      loading={loading}
      error={error}
      empty={rows.length === 0}
    >
      {data?.note && (
        <p className="mb-2 text-xs text-muted-foreground">{data.note}</p>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-10" numeric>
              #
            </TableHead>
            <TableHead>Kode</TableHead>
            <TableHead>Nama Item</TableHead>
            <TableHead numeric>Pendapatan</TableHead>
            <TableHead numeric>HPP</TableHead>
            <TableHead numeric>Laba</TableHead>
            <TableHead numeric>Margin</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((row, i) => (
            <TableRow key={row.itemId}>
              <TableCell numeric>{formatNumber(i + 1, 0)}</TableCell>
              <TableCell className="mono tabular-nums">{row.itemCode}</TableCell>
              <TableCell>{row.itemName}</TableCell>
              <TableCell numeric>{formatRupiah(row.revenue)}</TableCell>
              <TableCell numeric>{formatRupiah(row.cogs)}</TableCell>
              <TableCell numeric>{formatRupiah(row.profit)}</TableCell>
              <TableCell numeric>{formatNumber(row.marginPct, 1)}%</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </StatPageShell>
  );
}

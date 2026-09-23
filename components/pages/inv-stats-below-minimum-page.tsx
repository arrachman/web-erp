'use client';

/**
 * Warehouse Statistics — Below Minimum Stock (ranked table).
 * Source: `GET /inv/stats/below-minimum`. Shortage emphasized via danger Badge.
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
import { Badge } from '@/components/ui/badge';
import { StatPageShell, useStatData } from '@/components/organisms/stat-page-shell';
import { getBelowMinimum, type BelowMinimumRow } from '@/lib/api/inv-stats';
import { formatNumber } from '@/lib/format';

export function InvStatsBelowMinimumPage() {
  const { data, loading, error } = useStatData<BelowMinimumRow[]>(() =>
    getBelowMinimum(),
  );
  const rows = data ?? [];

  return (
    <StatPageShell
      title="Below Minimum Stock"
      code="inv/stats/below-minimum"
      loading={loading}
      error={error}
      empty={rows.length === 0}
    >
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Kode</TableHead>
            <TableHead>Nama Item</TableHead>
            <TableHead>Gudang</TableHead>
            <TableHead numeric>Stok</TableHead>
            <TableHead numeric>Minimum</TableHead>
            <TableHead numeric>Kekurangan</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={`${row.itemId}-${row.warehouseName ?? ''}`}>
              <TableCell className="mono tabular-nums">{row.itemCode}</TableCell>
              <TableCell>{row.itemName}</TableCell>
              <TableCell>{row.warehouseName ?? '—'}</TableCell>
              <TableCell numeric>{formatNumber(row.onHand, 0)}</TableCell>
              <TableCell numeric>{formatNumber(row.minQty, 0)}</TableCell>
              <TableCell numeric>
                <Badge variant="danger">{formatNumber(row.shortage, 0)}</Badge>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </StatPageShell>
  );
}

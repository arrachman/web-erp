'use client';

/**
 * Warehouse Statistics — Need Approval (table of doc types + total row).
 * Source: `GET /inv/stats/approvals` (data[] + top-level `total`).
 *
 * Atomic tier: Page.
 */

import * as React from 'react';
import {
  Table,
  TableHeader,
  TableBody,
  TableFooter,
  TableRow,
  TableHead,
  TableCell,
} from '@/components/organisms/table';
import { StatPageShell, useStatData } from '@/components/organisms/stat-page-shell';
import { getApprovals, type ApprovalRow } from '@/lib/api/inv-stats';
import { formatNumber } from '@/lib/format';

interface ApprovalData {
  rows: ApprovalRow[];
  total: number;
}

export function InvStatsApprovalsPage() {
  const { data, loading, error } = useStatData<ApprovalData>(() =>
    getApprovals(),
  );
  const rows = data?.rows ?? [];

  return (
    <StatPageShell
      title="Need Approval (Warehouse)"
      code="inv/stats/approvals"
      loading={loading}
      error={error}
      empty={rows.length === 0}
    >
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Jenis Dokumen</TableHead>
            <TableHead numeric>Jumlah</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={row.docType}>
              <TableCell>{row.label}</TableCell>
              <TableCell numeric>{formatNumber(row.count, 0)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
        <TableFooter>
          <TableRow>
            <TableCell>Total</TableCell>
            <TableCell numeric>{formatNumber(data?.total ?? 0, 0)}</TableCell>
          </TableRow>
        </TableFooter>
      </Table>
    </StatPageShell>
  );
}

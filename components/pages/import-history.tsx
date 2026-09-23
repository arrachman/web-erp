'use client';

/**
 * Admin — Import history table (sub-component of ErpImportPage).
 * Renders recent import jobs from GET /import/jobs.
 * Atomic tier: Organism.
 */

import * as React from 'react';
import type { ImportJob } from '@/lib/api/import';

const STATUS_COLOR: Record<string, string> = {
  COMPLETED: 'var(--success, #16a34a)',
  PARTIAL: 'var(--warning, #d97706)',
  FAILED: 'var(--danger, #dc2626)',
  PENDING: 'var(--fg-subtle)',
};

function fmtDate(iso: string): string {
  try {
    return new Date(iso).toLocaleString('id-ID', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  } catch {
    return iso;
  }
}

export function ImportHistory({ jobs }: { jobs: ImportJob[] }) {
  if (jobs.length === 0) {
    return (
      <p style={{ fontSize: 'calc(12px * var(--font-scale, 1))', color: 'var(--fg-subtle)' }}>
        Belum ada riwayat impor.
      </p>
    );
  }

  return (
    <div style={{ overflowX: 'auto' }}>
      <table
        style={{
          width: '100%',
          borderCollapse: 'collapse',
          fontSize: 'calc(12px * var(--font-scale, 1))',
        }}
      >
        <thead>
          <tr style={{ textAlign: 'left', color: 'var(--fg-subtle)' }}>
            <th style={{ padding: '6px 8px' }}>Waktu</th>
            <th style={{ padding: '6px 8px' }}>Entitas</th>
            <th style={{ padding: '6px 8px' }}>File</th>
            <th style={{ padding: '6px 8px' }}>Status</th>
            <th style={{ padding: '6px 8px', textAlign: 'right' }}>Total</th>
            <th style={{ padding: '6px 8px', textAlign: 'right' }}>OK</th>
            <th style={{ padding: '6px 8px', textAlign: 'right' }}>Gagal</th>
          </tr>
        </thead>
        <tbody>
          {jobs.map((j) => (
            <tr key={j.id} style={{ borderTop: '1px solid var(--border)' }}>
              <td style={{ padding: '6px 8px' }}>{fmtDate(j.createdAt)}</td>
              <td style={{ padding: '6px 8px' }}>{j.entity}</td>
              <td style={{ padding: '6px 8px' }}>{j.fileName}</td>
              <td style={{ padding: '6px 8px', color: STATUS_COLOR[j.status] ?? 'var(--fg)' }}>
                {j.status}
              </td>
              <td style={{ padding: '6px 8px', textAlign: 'right' }}>{j.rowsTotal}</td>
              <td style={{ padding: '6px 8px', textAlign: 'right' }}>{j.rowsOk}</td>
              <td style={{ padding: '6px 8px', textAlign: 'right' }}>{j.rowsFailed}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

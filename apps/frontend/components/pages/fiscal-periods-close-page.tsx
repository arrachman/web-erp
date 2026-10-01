'use client';

/**
 * Close Fiscal Period wizard page.
 * Lists fiscal periods that are OPEN or SOFT_CLOSED and lets admin close them.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
} from '@/components/organisms/table';
import { confirmAction, notify } from '@/lib/feedback';
import { useErpList } from '@/lib/use-erp-list';
import {
  listFiscalPeriods,
  closeFiscalPeriod,
} from '@/lib/api/fiscal-periods';
import type {
  ErpFiscalPeriod,
  ErpFiscalPeriodStatus,
} from '@/lib/api/fiscal-periods';

// ─── Helpers ──────────────────────────────────────────────────────────────────

const STATUS_VARIANT: Record<
  ErpFiscalPeriodStatus,
  'success' | 'default' | 'warn' | 'danger' | 'info'
> = {
  OPEN: 'success',
  SOFT_CLOSED: 'warn',
  CLOSED: 'default',
  REOPENED: 'info',
};

const STATUS_LABEL: Record<ErpFiscalPeriodStatus, string> = {
  OPEN: 'Buka',
  SOFT_CLOSED: 'Soft Closed',
  CLOSED: 'Ditutup',
  REOPENED: 'Dibuka Kembali',
};

function canClose(status: ErpFiscalPeriodStatus): boolean {
  return status === 'OPEN' || status === 'SOFT_CLOSED';
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpFiscalPeriodsClosePage() {
  const { rows, loading, error, reload } = useErpList(() =>
    listFiscalPeriods(),
  );

  const handleClose = (p: ErpFiscalPeriod) => {
    confirmAction({
      title: 'Tutup periode?',
      message: `${p.name} akan ditutup. Transaksi pada periode ini akan terkunci.`,
      variant: 'warn',
      confirmLabel: 'Tutup',
      confirmIcon: 'shield',
      onConfirm: async () => {
        try {
          await closeFiscalPeriod(p.id);
          notify('Periode ditutup', 'success');
          reload();
        } catch (e: unknown) {
          notify(e instanceof Error ? e.message : 'Gagal menutup periode', 'danger');
        }
      },
    });
  };

  return (
    <div className="p-6 max-w-4xl">
      {/* Header */}
      <div className="mb-4">
        <h2 className="text-lg font-semibold">Tutup Periode Fiskal</h2>
        <p className="text-sm text-muted mt-1">
          Menutup periode fiskal akan mengunci semua transaksi pada periode tersebut.
          Periode yang sudah ditutup tidak dapat menerima transaksi baru kecuali
          dibuka kembali oleh administrator.
        </p>
      </div>

      {/* Info box */}
      <div className="card p-4 mb-6" style={{ borderLeft: '4px solid var(--warn)' }}>
        <p className="text-sm text-warn font-medium">Perhatian</p>
        <p className="text-sm text-muted mt-1">
          Tindakan penutupan periode bersifat final. Pastikan seluruh transaksi
          pada periode tersebut sudah diverifikasi sebelum menutupnya.
        </p>
      </div>

      {/* Error state */}
      {error && (
        <div className="text-sm text-danger mb-4">
          Gagal memuat data: {error}
        </div>
      )}

      {/* Table */}
      <div className="lines">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nama Periode</TableHead>
              <TableHead>Tahun</TableHead>
              <TableHead>No</TableHead>
              <TableHead>Mulai</TableHead>
              <TableHead>Akhir</TableHead>
              <TableHead>Status</TableHead>
              <TableHead />
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableEmpty colSpan={7} title="Memuat..." />
            ) : rows.length === 0 ? (
              <TableEmpty colSpan={7} />
            ) : (
              rows.map((p) => {
                const closeable = canClose(p.status);
                return (
                  <TableRow
                    key={p.id}
                    style={{ opacity: p.status === 'CLOSED' ? 0.5 : 1 }}
                  >
                    <TableCell>{p.name}</TableCell>
                    <TableCell className="mono">{p.year}</TableCell>
                    <TableCell className="mono">{p.periodNo}</TableCell>
                    <TableCell>{p.startDate.slice(0, 10)}</TableCell>
                    <TableCell>{p.endDate.slice(0, 10)}</TableCell>
                    <TableCell>
                      <Badge variant={STATUS_VARIANT[p.status]} dot>
                        {STATUS_LABEL[p.status]}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      {closeable && (
                        <button
                          className="btn sm warn"
                          onClick={() => handleClose(p)}
                        >
                          Tutup
                        </button>
                      )}
                    </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}

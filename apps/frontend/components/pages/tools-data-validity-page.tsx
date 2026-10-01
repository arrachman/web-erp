'use client';

/**
 * Admin tool page — Data Validity Check.
 * Scans the database for common data integrity issues and displays results.
 * Auto-runs on mount. Atomic tier: Page.
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
import { notify } from '@/lib/feedback';
import {
  runDataValidity,
  type DataValidityResult,
  type ValidityCheck,
} from '@/lib/api/tools';

// ─── Helpers ──────────────────────────────────────────────────────────────────

const CHECK_VARIANT: Record<
  ValidityCheck['status'],
  'success' | 'warn' | 'danger'
> = {
  OK: 'success',
  WARN: 'warn',
  ERROR: 'danger',
};

const CHECK_LABEL: Record<ValidityCheck['status'], string> = {
  OK: 'OK',
  WARN: 'Peringatan',
  ERROR: 'Error',
};

// ─── Summary bar ──────────────────────────────────────────────────────────────

function SummaryBar({
  summary,
}: {
  summary: DataValidityResult['summary'];
}) {
  return (
    <div style={{ display: 'flex', gap: 24 }} className="mt-4">
      <span className="text-sm text-success">
        <strong>{summary.ok}</strong> OK
      </span>
      <span className="text-sm text-warn">
        <strong>{summary.warn}</strong> peringatan
      </span>
      <span className="text-sm text-danger">
        <strong>{summary.error}</strong> error
      </span>
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpDataValidityPage() {
  const [running, setRunning] = React.useState(false);
  const [result, setResult] = React.useState<DataValidityResult | null>(null);
  const [lastRun, setLastRun] = React.useState<Date | null>(null);

  const doRun = React.useCallback(async () => {
    setRunning(true);
    try {
      const res = await runDataValidity();
      setResult(res.data);
      setLastRun(new Date());
    } catch (e: unknown) {
      notify(
        e instanceof Error ? e.message : 'Gagal menjalankan pemeriksaan',
        'danger',
      );
    } finally {
      setRunning(false);
    }
  }, []);

  // Auto-run on mount
  React.useEffect(() => {
    doRun();
  }, [doRun]);

  return (
    <div className="p-6 max-w-3xl">
      <h2 className="text-lg font-semibold mb-2">Data Validity Check</h2>
      <p className="text-sm text-muted mb-6">
        Memindai database untuk menemukan masalah integritas data yang umum,
        seperti referensi yang hilang, nilai yang tidak konsisten, atau
        inkonsistensi antar tabel.
      </p>

      {/* Action row */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 16 }} className="mb-4">
        <button
          className="btn primary"
          onClick={doRun}
          disabled={running}
        >
          {running ? 'Memeriksa...' : 'Jalankan Pemeriksaan'}
        </button>
        {lastRun && (
          <span className="text-xs text-muted">
            Terakhir dijalankan: {lastRun.toLocaleTimeString('id-ID')}
          </span>
        )}
      </div>

      {/* Results table */}
      {result ? (
        <>
          <div className="lines">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nama Pemeriksaan</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead style={{ textAlign: 'right' }}>Jumlah</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {result.checks.length === 0 ? (
                  <TableEmpty colSpan={3} title="Tidak ada hasil pemeriksaan" />
                ) : (
                  result.checks.map((check, idx) => (
                    <TableRow key={idx}>
                      <TableCell>{check.name}</TableCell>
                      <TableCell>
                        <Badge variant={CHECK_VARIANT[check.status]} dot>
                          {CHECK_LABEL[check.status]}
                        </Badge>
                      </TableCell>
                      <TableCell style={{ textAlign: 'right' }} className="mono">
                        {check.count}
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
          <SummaryBar summary={result.summary} />
        </>
      ) : running ? (
        <p className="text-sm text-muted">Memeriksa data...</p>
      ) : null}
    </div>
  );
}

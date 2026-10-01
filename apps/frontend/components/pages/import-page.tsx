'use client';

/**
 * Admin — Import Data page.
 * Real importer: pick a master entity, upload XLSX/CSV, backend parses,
 * validates, inserts valid rows, and returns a summary. Also downloads a
 * per-entity XLSX template and shows recent import history.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { notify } from '@/lib/feedback';
import {
  downloadTemplate,
  getImportEntities,
  importFile,
  listImportJobs,
  type ImportEntity,
  type ImportJob,
  type ImportResult,
} from '@/lib/api/import';
import { ImportHistory } from './import-history';

const LABEL_STYLE: React.CSSProperties = {
  display: 'block',
  marginBottom: 6,
  fontSize: 'calc(12.5px * var(--font-scale, 1))',
  color: 'var(--fg)',
};

const INPUT_FONT = 'calc(12.5px * var(--font-scale, 1))';

export function ErpImportPage() {
  const [entities, setEntities] = React.useState<ImportEntity[]>([]);
  const [entity, setEntity] = React.useState('');
  const [file, setFile] = React.useState<File | null>(null);
  const [busy, setBusy] = React.useState(false);
  const [result, setResult] = React.useState<ImportResult | null>(null);
  const [jobs, setJobs] = React.useState<ImportJob[]>([]);
  const fileInputRef = React.useRef<HTMLInputElement>(null);

  const selected = entities.find((e) => e.value === entity);

  const loadJobs = React.useCallback(async () => {
    try {
      const res = await listImportJobs();
      setJobs(res.data);
    } catch (err) {
      // history is non-critical; surface but don't block
      notify(err instanceof Error ? err.message : 'Gagal memuat riwayat', 'warn');
    }
  }, []);

  React.useEffect(() => {
    getImportEntities()
      .then((res) => setEntities(res.data))
      .catch((err) =>
        notify(err instanceof Error ? err.message : 'Gagal memuat daftar entitas', 'danger'),
      );
    loadJobs();
  }, [loadJobs]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFile(e.target.files?.[0] ?? null);
    setResult(null);
  };

  const handleTemplate = async () => {
    if (!entity) {
      notify('Pilih entitas terlebih dahulu', 'warn');
      return;
    }
    try {
      await downloadTemplate(entity);
    } catch (err) {
      notify(err instanceof Error ? err.message : 'Gagal mengunduh template', 'danger');
    }
  };

  const handleImport = async () => {
    if (!entity) {
      notify('Pilih entitas terlebih dahulu', 'warn');
      return;
    }
    if (!file) {
      notify('Pilih file untuk diimpor', 'warn');
      return;
    }
    setBusy(true);
    setResult(null);
    try {
      const res = await importFile(entity, file);
      setResult(res.data);
      const { ok, failed, total, async: isAsync, jobId } = res.data;

      if (isAsync || res.data.status === 'PENDING' || res.data.status === 'RUNNING') {
        notify(
          `Impor dijadwalkan (${total} baris). Job #${jobId} — pantau riwayat di bawah.`,
          'success',
        );
        // Poll job list until terminal status (max ~2 min)
        const terminal = new Set(['COMPLETED', 'FAILED', 'PARTIAL']);
        for (let i = 0; i < 60; i += 1) {
          await new Promise((r) => setTimeout(r, 2000));
          const jobs = await listImportJobs();
          const job = (jobs.data ?? []).find((j) => j.id === jobId);
          if (job && terminal.has(job.status)) {
            setResult({
              jobId,
              total: job.rowsTotal,
              ok: job.rowsOk,
              failed: job.rowsFailed,
              errors: job.errors ?? res.data.errors,
              status: job.status,
            });
            if (job.status === 'COMPLETED') {
              notify(`${job.rowsOk} dari ${job.rowsTotal} baris berhasil diimpor`, 'success');
            } else if (job.status === 'FAILED') {
              notify(`Impor gagal: ${job.rowsFailed} baris bermasalah`, 'danger');
            } else {
              notify(`${job.rowsOk} baris berhasil, ${job.rowsFailed} gagal`, 'warn');
            }
            break;
          }
        }
        await loadJobs();
      } else {
        if (failed === 0) notify(`${ok} dari ${total} baris berhasil diimpor`, 'success');
        else if (ok === 0) notify(`Impor gagal: ${failed} baris bermasalah`, 'danger');
        else notify(`${ok} baris berhasil, ${failed} gagal`, 'warn');
        await loadJobs();
      }
    } catch (err) {
      notify(err instanceof Error ? err.message : 'Impor gagal', 'danger');
    } finally {
      setBusy(false);
    }
  };

  const handleReset = () => {
    setEntity('');
    setFile(null);
    setResult(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  return (
    <div style={{ padding: '24px', maxWidth: 760 }}>
      <h2 style={{ marginBottom: 20, fontSize: 'calc(15px * var(--font-scale, 1))' }}>
        Import Data
      </h2>

      <div className="card" style={{ padding: 16, marginBottom: 24 }}>
        {/* Entity selector */}
        <div style={{ marginBottom: 16 }}>
          <label htmlFor="imp-entity" style={LABEL_STYLE}>
            Entitas
          </label>
          <select
            id="imp-entity"
            value={entity}
            onChange={(e) => {
              setEntity(e.target.value);
              setResult(null);
            }}
            style={{
              height: 28,
              width: '100%',
              maxWidth: 360,
              fontSize: INPUT_FONT,
              borderRadius: 6,
              border: '1px solid var(--border)',
              background: 'var(--card)',
              color: 'var(--fg)',
              padding: '0 8px',
              cursor: 'pointer',
            }}
          >
            <option value="">— Pilih entitas —</option>
            {entities.map((e) => (
              <option key={e.value} value={e.value}>
                {e.label}
              </option>
            ))}
          </select>
        </div>

        {/* Header hints */}
        {selected && (
          <div
            style={{
              marginBottom: 16,
              fontSize: 'calc(11.5px * var(--font-scale, 1))',
              color: 'var(--fg-subtle)',
            }}
          >
            <div>
              Kolom wajib: <strong>{selected.requiredHeaders.join(', ')}</strong>
            </div>
            {selected.optionalHeaders.length > 0 && (
              <div>Kolom opsional: {selected.optionalHeaders.join(', ')}</div>
            )}
          </div>
        )}

        {/* File upload */}
        <div style={{ marginBottom: 16 }}>
          <label htmlFor="imp-file" style={LABEL_STYLE}>
            File (.xlsx, .csv)
          </label>
          <input
            id="imp-file"
            ref={fileInputRef}
            type="file"
            accept=".xlsx,.csv"
            onChange={handleFileChange}
            style={{ fontSize: INPUT_FONT, color: 'var(--fg)', cursor: 'pointer' }}
          />
        </div>

        {/* Actions */}
        <div style={{ display: 'flex', gap: 8, marginTop: 8, flexWrap: 'wrap' }}>
          <button className="btn primary" onClick={handleImport} disabled={busy}>
            {busy ? 'Mengimpor…' : 'Impor'}
          </button>
          <button className="btn ghost" onClick={handleTemplate} disabled={!entity}>
            Unduh Template
          </button>
          <button className="btn ghost" onClick={handleReset} disabled={busy}>
            Reset
          </button>
        </div>

        {/* Result summary */}
        {result && (
          <div
            style={{
              marginTop: 20,
              padding: '10px 14px',
              background: 'var(--bg-hover)',
              borderRadius: 6,
              border: '1px solid var(--border)',
              fontSize: 'calc(12px * var(--font-scale, 1))',
            }}
          >
            <div style={{ marginBottom: result.errors.length > 0 ? 10 : 0 }}>
              Total <strong>{result.total}</strong> · Berhasil{' '}
              <strong style={{ color: 'var(--success, #16a34a)' }}>{result.ok}</strong> · Gagal{' '}
              <strong style={{ color: 'var(--danger, #dc2626)' }}>{result.failed}</strong>
            </div>
            {result.errors.length > 0 && (
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ textAlign: 'left', color: 'var(--fg-subtle)' }}>
                    <th style={{ padding: '4px 8px', width: 70 }}>Baris</th>
                    <th style={{ padding: '4px 8px' }}>Pesan</th>
                  </tr>
                </thead>
                <tbody>
                  {result.errors.map((e) => (
                    <tr key={e.row} style={{ borderTop: '1px solid var(--border)' }}>
                      <td style={{ padding: '4px 8px' }}>{e.row}</td>
                      <td style={{ padding: '4px 8px' }}>{e.message}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </div>

      <h3 style={{ marginBottom: 12, fontSize: 'calc(13.5px * var(--font-scale, 1))' }}>
        Riwayat Impor
      </h3>
      <div className="card" style={{ padding: 16 }}>
        <ImportHistory jobs={jobs} />
      </div>
    </div>
  );
}

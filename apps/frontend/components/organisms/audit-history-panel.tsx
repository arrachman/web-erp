'use client';

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Icon } from '@/components/ui/icons';
import { listAuditLogs } from '@/lib/api/audit-logs';
import type { ErpAuditLog, AuditAction, FieldDiff } from '@/lib/api/audit-logs';

// ─── Helpers ──────────────────────────────────────────────────────────────────

const ACTION_LABEL: Record<AuditAction, string> = {
  CREATE: 'Dibuat',
  UPDATE: 'Diperbarui',
  DELETE: 'Dihapus',
  RESTORE: 'Dipulihkan',
  LOGIN: 'Login',
  LOGOUT: 'Logout',
  APPROVE: 'Disetujui',
  REJECT: 'Ditolak',
  POST: 'Diposting',
  REOPEN: 'Dibuka Ulang',
};

const ACTION_VARIANT: Record<AuditAction, 'success' | 'danger' | 'info' | 'warn' | 'default'> = {
  CREATE: 'success',
  UPDATE: 'info',
  DELETE: 'danger',
  RESTORE: 'warn',
  LOGIN: 'info',
  LOGOUT: 'default',
  APPROVE: 'success',
  REJECT: 'danger',
  POST: 'info',
  REOPEN: 'warn',
};

function formatDate(iso: string) {
  return new Date(iso).toLocaleString('id-ID', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
    hour12: false,
  });
}

function formatValue(v: unknown): string {
  if (v === null || v === undefined) return '—';
  if (typeof v === 'boolean') return v ? 'Ya' : 'Tidak';
  return String(v);
}

// ─── Sub-components ───────────────────────────────────────────────────────────

function DiffTable({ changes }: { changes: Record<string, FieldDiff> }) {
  const entries = Object.entries(changes);
  if (entries.length === 0) return null;
  return (
    <table className="audit-diff-table">
      <thead>
        <tr>
          <th>Field</th>
          <th>Sebelum</th>
          <th>Sesudah</th>
        </tr>
      </thead>
      <tbody>
        {entries.map(([field, { from, to }]) => (
          <tr key={field}>
            <td className="field-name">{field}</td>
            <td className="from">{formatValue(from)}</td>
            <td className="to">{formatValue(to)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function AuditEntry({ log }: { log: ErpAuditLog }) {
  const [expanded, setExpanded] = React.useState(false);
  const hasChanges = log.changes && Object.keys(log.changes).length > 0;

  return (
    <div className="audit-entry">
      <div className="audit-entry-header" onClick={() => hasChanges && setExpanded((v) => !v)}
        style={{ cursor: hasChanges ? 'pointer' : 'default' }}>
        <div className="audit-entry-icon">
          <Icon name="history" />
        </div>
        <div className="audit-entry-meta">
          <div className="audit-entry-row">
            <Badge variant={ACTION_VARIANT[log.action]}>
              {ACTION_LABEL[log.action]}
            </Badge>
            {log.summary && <span className="audit-summary">{log.summary}</span>}
          </div>
          <div className="audit-entry-sub">
            <Icon name="user" />
            <span>{log.actor ? `${log.actor.name} (${log.actor.code})` : 'Sistem'}</span>
            <span className="audit-dot">·</span>
            <span>{formatDate(log.occurredAt)}</span>
            {log.actorIp && (
              <>
                <span className="audit-dot">·</span>
                <span className="audit-ip">{log.actorIp}</span>
              </>
            )}
          </div>
        </div>
        {hasChanges && (
          <Icon name={expanded ? 'chevup' : 'chevdown'} className="audit-expand-icon" />
        )}
      </div>
      {expanded && hasChanges && log.changes && (
        <div className="audit-entry-diff">
          <DiffTable changes={log.changes} />
        </div>
      )}
    </div>
  );
}

// ─── Main component ───────────────────────────────────────────────────────────

interface Props {
  entityName: string;
  entityId: string;
}

export function AuditHistoryPanel({ entityName, entityId }: Props) {
  const [logs, setLogs] = React.useState<ErpAuditLog[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [page, setPage] = React.useState(1);
  const [totalPages, setTotalPages] = React.useState(1);

  React.useEffect(() => {
    setLoading(true);
    setError(null);
    listAuditLogs({ entityName, entityId, page, limit: 15 })
      .then((res) => {
        setLogs(res.data);
        setTotalPages(res.meta.totalPages);
      })
      .catch((e: unknown) => setError(e instanceof Error ? e.message : 'Gagal memuat riwayat'))
      .finally(() => setLoading(false));
  }, [entityName, entityId, page]);

  if (loading) {
    return (
      <div className="audit-panel-state">
        <Icon name="refresh" className="spin" />
        <span>Memuat riwayat…</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="audit-panel-state danger">
        <Icon name="info" />
        <span>{error}</span>
      </div>
    );
  }

  if (logs.length === 0) {
    return (
      <div className="audit-panel-state muted">
        <Icon name="history" />
        <span>Belum ada riwayat perubahan</span>
      </div>
    );
  }

  return (
    <div className="audit-panel">
      <div className="audit-entries">
        {logs.map((log) => (
          <AuditEntry key={log.id} log={log} />
        ))}
      </div>
      {totalPages > 1 && (
        <div className="audit-pagination">
          <button className="btn sm ghost" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
            <Icon name="chevleft" /> Sebelumnya
          </button>
          <span className="audit-page-info">{page} / {totalPages}</span>
          <button className="btn sm ghost" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
            Berikutnya <Icon name="chevright" />
          </button>
        </div>
      )}
    </div>
  );
}

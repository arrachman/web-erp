'use client';

/**
 * Administrator — Online Users page.
 * Read-only view of active sessions from GET /erp/users/online.
 * Auto-refreshes every 60 seconds. Has a window selector (15/30/60/120 min).
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
} from '@/components/organisms/table';
import { Icon } from '@/components/ui/icons';
import { notify } from '@/lib/feedback';
import { listOnlineUsers, type ErpOnlineUser } from '@/lib/api/online-users';

// ─── Helpers ──────────────────────────────────────────────────────────────────

const WINDOW_OPTIONS = [
  { label: '15 menit', value: '15' },
  { label: '30 menit', value: '30' },
  { label: '60 menit', value: '60' },
  { label: '120 menit', value: '120' },
];

function formatRelative(iso: string): string {
  try {
    const diff = Math.floor((Date.now() - new Date(iso).getTime()) / 1000);
    if (diff < 60) return `${diff}d lalu`;
    if (diff < 3600) return `${Math.floor(diff / 60)}m lalu`;
    return `${Math.floor(diff / 3600)}j lalu`;
  } catch {
    return iso;
  }
}

function formatDateTime(iso: string): string {
  try {
    const d = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
  } catch {
    return iso;
  }
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpOnlineUsersPage() {
  const [sessions, setSessions] = React.useState<ErpOnlineUser[]>([]);
  const [meta, setMeta] = React.useState<{ count: number; windowMinutes: number } | null>(null);
  const [loading, setLoading] = React.useState(false);
  const [windowMin, setWindowMin] = React.useState(30);

  const load = React.useCallback(async () => {
    setLoading(true);
    try {
      const res = await listOnlineUsers(windowMin);
      setSessions(res.data);
      setMeta(res.meta);
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal memuat sesi aktif', 'danger');
    } finally {
      setLoading(false);
    }
  }, [windowMin]);

  React.useEffect(() => {
    load();
  }, [load]);

  React.useEffect(() => {
    const t = setInterval(load, 60_000);
    return () => clearInterval(t);
  }, [load]);

  const count = meta?.count ?? sessions.length;

  return (
    <div className="page">
      {/* Action bar */}
      <div className="page-header">
        <h1 className="page-title">
          Online Users
          <span className="code-tag">ONLINE</span>
        </h1>
        <div className="page-actions">
          <Select
            value={String(windowMin)}
            onValueChange={(v) => setWindowMin(Number(v))}
          >
            <SelectTrigger style={{ width: 'auto', minWidth: '9rem' }}>
              <span style={{ color: 'var(--fg-faint)', fontSize: 'calc(11px * var(--font-scale, 1))', marginRight: 2 }}>
                Window:
              </span>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {WINDOW_OPTIONS.map((o) => (
                <SelectItem key={o.value} value={o.value}>
                  {o.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <button className="btn" onClick={load} disabled={loading} title="Muat ulang">
            <Icon name="refresh" size={12} />
            {loading ? 'Memuat...' : 'Refresh'}
          </button>

          <Badge variant={count > 0 ? 'success' : 'default'}>
            {count} aktif dalam {meta?.windowMinutes ?? windowMin} menit terakhir
          </Badge>
        </div>
      </div>

      {/* Table */}
      <div className="page-body">
        <div className="flex-1 min-h-0">
          {loading && sessions.length === 0 ? (
            <div className="text-center text-xs text-muted-foreground py-8">Memuat...</div>
          ) : (
            <div className="lines">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead style={{ width: 100 }}>Kode</TableHead>
                    <TableHead>Nama</TableHead>
                    <TableHead>Email</TableHead>
                    <TableHead style={{ width: 130 }}>IP Address</TableHead>
                    <TableHead>Device</TableHead>
                    <TableHead style={{ width: 120 }}>Terakhir Aktif</TableHead>
                    <TableHead style={{ width: 140 }}>Kadaluarsa</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {sessions.length === 0 ? (
                    <TableEmpty colSpan={7} icon="users" entityLabel="sesi aktif" />
                  ) : (
                    sessions.map((s) => (
                      <TableRow key={s.id}>
                        <TableCell className="mono">
                          {s.user?.code ?? '—'}
                        </TableCell>
                        <TableCell>{s.user?.name ?? '—'}</TableCell>
                        <TableCell className="muted">{s.user?.email ?? '—'}</TableCell>
                        <TableCell className="mono">{s.ipAddress ?? '—'}</TableCell>
                        <TableCell className="muted" title={s.userAgent ?? undefined}>
                          {s.deviceName ?? (s.userAgent ? s.userAgent.slice(0, 40) + '…' : '—')}
                        </TableCell>
                        <TableCell className="mono" title={s.lastActiveAt}>
                          {formatRelative(s.lastActiveAt)}
                        </TableCell>
                        <TableCell className="mono">
                          {formatDateTime(s.expiresAt)}
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

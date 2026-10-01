'use client';

/**
 * Admin page — Role Document Creation Policies (matrix editor).
 * Baris = jenis dokumen (grouped, collapsible). Kolom = status yang diizinkan saat input.
 * Pilih role → centang/hapus centang → Simpan (bulk upsert).
 */

import * as React from 'react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { notify } from '@/lib/feedback';
import { listRoles, type ErpRole } from '@/lib/api/roles';
import {
  getAllPoliciesForRole,
  bulkUpsertPolicies,
} from '@/lib/api/role-doc-policies';
import { RoleDocPoliciesMatrix, DOC_GROUPS } from './role-doc-policies-matrix';

type Matrix = Record<string, Set<string>>;

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpRoleDocPoliciesPage() {
  const [roles, setRoles] = React.useState<ErpRole[]>([]);
  const [roleId, setRoleId] = React.useState('');
  const [matrix, setMatrix] = React.useState<Matrix>({});
  const [loading, setLoading] = React.useState(false);
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    listRoles({ limit: 200 })
      .then((res) => setRoles(res.data))
      .catch(() => {});
  }, []);

  const loadPolicies = React.useCallback(async (rid: string) => {
    setLoading(true);
    setMatrix({});
    try {
      const policies = await getAllPoliciesForRole(rid);
      const m: Matrix = {};
      for (const p of policies) {
        m[p.documentType] = new Set(p.allowedStatuses);
      }
      setMatrix(m);
    } catch {
      notify('Gagal memuat policy', 'danger');
    } finally {
      setLoading(false);
    }
  }, []);

  const handleRoleChange = (rid: string) => {
    setRoleId(rid);
    if (rid) loadPolicies(rid);
    else setMatrix({});
  };

  const toggle = (docType: string, status: string) => {
    setMatrix((prev) => {
      const set = new Set(prev[docType] ?? []);
      if (set.has(status)) set.delete(status);
      else set.add(status);
      return { ...prev, [docType]: set };
    });
  };

  const toggleGroup = (docTypes: string[], status: string, checked: boolean) => {
    setMatrix((prev) => {
      const next = { ...prev };
      for (const dt of docTypes) {
        const set = new Set(prev[dt] ?? []);
        if (checked) set.add(status);
        else set.delete(status);
        next[dt] = set;
      }
      return next;
    });
  };

  const handleSave = async () => {
    if (!roleId) {
      notify('Pilih role terlebih dahulu', 'danger');
      return;
    }
    setSaving(true);
    try {
      const allDocTypes = DOC_GROUPS.flatMap((g) => g.items);
      const entries = allDocTypes.map((dt) => ({
        documentType: dt,
        allowedStatuses: [...(matrix[dt] ?? [])],
      }));
      await bulkUpsertPolicies({ roleId, entries });
      notify('Policy disimpan', 'success');
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const selectedRole = roles.find((r) => r.id === roleId);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      {/* ── Toolbar ─────────────────────────────────────────────── */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 12,
          padding: '8px 16px',
          borderBottom: '1px solid var(--border)',
          background: 'var(--card)',
          flexShrink: 0,
          flexWrap: 'wrap',
        }}
      >
        <span style={{ fontWeight: 700, fontSize: 'calc(13px * var(--font-scale, 1))', marginRight: 4 }}>
          Kebijakan Status Dokumen
        </span>

        <div style={{ width: 220 }}>
          <Select value={roleId} onValueChange={handleRoleChange}>
            <SelectTrigger>
              <SelectValue placeholder="Pilih role…" />
            </SelectTrigger>
            <SelectContent>
              {roles.map((r) => (
                <SelectItem key={r.id} value={r.id}>{r.name}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <button
          className="btn primary sm"
          onClick={handleSave}
          disabled={saving || !roleId}
        >
          {saving ? 'Menyimpan…' : 'Simpan'}
        </button>

        {loading && (
          <span style={{ color: 'var(--fg-muted)', fontSize: 'calc(12px * var(--font-scale, 1))' }}>
            Memuat…
          </span>
        )}
        {selectedRole && !loading && (
          <span style={{ color: 'var(--fg-muted)', fontSize: 'calc(12px * var(--font-scale, 1))' }}>
            {selectedRole.name} — centang status yang diizinkan saat input dokumen
          </span>
        )}
      </div>

      {/* ── Content ─────────────────────────────────────────────── */}
      {!roleId ? (
        <div
          style={{
            flex: 1,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'var(--fg-muted)',
            fontSize: 'calc(13px * var(--font-scale, 1))',
          }}
        >
          Pilih role untuk mengelola kebijakan status dokumen
        </div>
      ) : (
        <div style={{ flex: 1, overflowY: 'auto', paddingBottom: 24 }}>
          <RoleDocPoliciesMatrix
            matrix={matrix}
            onToggle={toggle}
            onToggleGroup={toggleGroup}
          />
        </div>
      )}
    </div>
  );
}

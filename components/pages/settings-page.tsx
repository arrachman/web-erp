'use client';

/**
 * F2 Admin — System Settings page.
 * Displays list of key-value settings; allows inline edit per row.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import { ErpListLayout } from '@/components/organisms/erp-list-layout';
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
import { useErpList } from '@/lib/use-erp-list';
import { getSettings, updateSetting } from '@/lib/api/settings';
import type { ErpSetting } from '@/lib/api/settings';

// ─── Inline edit row ──────────────────────────────────────────────────────────

function SettingRow({
  setting,
  onSaved,
}: {
  setting: ErpSetting;
  onSaved: () => void;
}) {
  const [editing, setEditing] = React.useState(false);
  const [value, setValue] = React.useState(setting.value ?? '');
  const [saving, setSaving] = React.useState(false);

  const handleSave = async () => {
    setSaving(true);
    try {
      await updateSetting(setting.key, value);
      notify(`${setting.key} diperbarui`, 'success');
      setEditing(false);
      onSaved();
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    setValue(setting.value ?? '');
    setEditing(false);
  };

  return (
    <TableRow>
      <TableCell className="mono text-[11px] text-muted-foreground">
        {setting.group ?? '—'}
      </TableCell>
      <TableCell className="mono">{setting.key}</TableCell>
      <TableCell className="muted text-[11px]">
        {setting.description ?? '—'}
      </TableCell>
      <TableCell>
        {editing ? (
          <Input
            value={value}
            onChange={(e) => setValue(e.target.value)}
            className="w-full"
            autoFocus
          />
        ) : (
          <span className="mono text-xs">{setting.value ?? '—'}</span>
        )}
      </TableCell>
      <TableCell>
        {editing ? (
          <div style={{ display: 'flex', gap: 4 }}>
            <button
              className="btn sm primary"
              onClick={handleSave}
              disabled={saving}
            >
              {saving ? '...' : <Icon name="check" size={12} />}
            </button>
            <button className="btn sm ghost" onClick={handleCancel}>
              <Icon name="x" size={12} />
            </button>
          </div>
        ) : (
          <button className="btn sm" onClick={() => setEditing(true)}>
            Edit
          </button>
        )}
      </TableCell>
    </TableRow>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpSettingsPage() {
  const { rows, loading, error, reload } = useErpList(() => getSettings());
  const [search, setSearch] = React.useState('');

  const filtered = React.useMemo(() => {
    const q = search.toLowerCase();
    return q
      ? rows.filter(
          (r) =>
            r.key.toLowerCase().includes(q) ||
            (r.group ?? '').toLowerCase().includes(q) ||
            (r.description ?? '').toLowerCase().includes(q),
        )
      : rows;
  }, [rows, search]);

  return (
    <ErpListLayout
      title="System Settings"
      code="SET"
      loading={loading}
      error={error}
      search={search}
      onSearch={setSearch}
      onAdd={() => notify('Setting baru ditambahkan melalui backend', 'info')}
      onRefresh={reload}
      addLabel="Info"
    >
      <div className="lines">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Grup</TableHead>
              <TableHead>Key</TableHead>
              <TableHead>Deskripsi</TableHead>
              <TableHead>Value</TableHead>
              <TableHead />
            </TableRow>
          </TableHeader>
          <TableBody>
            {filtered.length === 0 ? (
              <TableEmpty colSpan={5} />
            ) : (
              filtered.map((s) => (
                <SettingRow key={s.key} setting={s} onSaved={reload} />
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </ErpListLayout>
  );
}

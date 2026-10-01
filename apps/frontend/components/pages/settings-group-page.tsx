'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { notify } from '@/lib/feedback';
import { getSettings, updateSetting } from '@/lib/api/settings';
import type { ErpSetting } from '@/lib/api/settings';
import { SettingRow } from './setting-row';
import { EmptyGroup } from './empty-group-state';

interface Props {
  group: string;
  title: string;
}

export function SettingsGroupPage({ group, title }: Props) {
  const [settings, setSettings] = React.useState<ErpSetting[]>([]);
  const [values, setValues] = React.useState<Record<string, string>>({});
  const [original, setOriginal] = React.useState<Record<string, string>>({});
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    getSettings({ group })
      .then((res) => {
        if (cancelled) return;
        const rows = res.data ?? [];
        setSettings(rows);
        const map: Record<string, string> = {};
        for (const s of rows) map[s.key] = s.value ?? '';
        setValues(map);
        setOriginal(map);
      })
      .catch((e: unknown) => {
        if (cancelled) return;
        setError(e instanceof Error ? e.message : 'Gagal memuat pengaturan');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => { cancelled = true; };
  }, [group]);

  const dirtyKeys = React.useMemo(
    () => settings.filter((s) => values[s.key] !== original[s.key]).map((s) => s.key),
    [settings, values, original],
  );

  const handleChange = (key: string, val: string) =>
    setValues((prev) => ({ ...prev, [key]: val }));

  const handleReset = () => {
    setValues(original);
  };

  const handleSave = async () => {
    if (dirtyKeys.length === 0) {
      notify('Tidak ada perubahan', 'info');
      return;
    }
    setSaving(true);
    let failed = 0;
    for (const key of dirtyKeys) {
      try {
        await updateSetting(key, values[key] ?? '');
        setOriginal((prev) => ({ ...prev, [key]: values[key] ?? '' }));
      } catch {
        failed++;
      }
    }
    setSaving(false);
    if (failed === 0) {
      notify('Pengaturan berhasil disimpan', 'success');
    } else {
      notify(`${failed} pengaturan gagal disimpan`, 'danger');
    }
  };

  return (
    <div className="page">
      {/* Header */}
      <div className="page-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <div
            style={{
              width: 28,
              height: 28,
              borderRadius: 'var(--radius)',
              background: 'var(--primary-soft)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--primary)',
              flexShrink: 0,
            }}
          >
            <Icon name="gear" size={14} />
          </div>
          <h1 className="page-title" style={{ margin: 0 }}>
            {title}
            {dirtyKeys.length > 0 && (
              <span
                style={{
                  marginLeft: 8,
                  fontSize: 'calc(11px * var(--font-scale, 1))',
                  fontWeight: 500,
                  color: 'var(--warning, #f59e0b)',
                  background: 'color-mix(in srgb, var(--warning, #f59e0b) 12%, transparent)',
                  padding: '2px 8px',
                  borderRadius: 'var(--radius)',
                  verticalAlign: 'middle',
                }}
              >
                {dirtyKeys.length} perubahan belum disimpan
              </span>
            )}
          </h1>
        </div>
        {settings.length > 0 && (
          <div className="page-actions">
            <button
              className="btn ghost"
              onClick={handleReset}
              disabled={dirtyKeys.length === 0 || saving}
            >
              <Icon name="refresh" size={12} />
              Reset
            </button>
            <button
              className="btn primary"
              onClick={handleSave}
              disabled={saving}
            >
              <Icon name="save" size={12} />
              {saving ? 'Menyimpan…' : 'Simpan'}
            </button>
          </div>
        )}
      </div>

      {/* Content */}
      <div
        className="scrollbar"
        style={{
          flex: 1,
          overflow: 'auto',
          padding: '20px 24px',
        }}
      >
        {loading && (
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 8,
              color: 'var(--fg-muted)',
              fontSize: 'calc(12.5px * var(--font-scale, 1))',
              padding: '40px 0',
              justifyContent: 'center',
            }}
          >
            <Icon name="refresh" size={14} />
            Memuat pengaturan…
          </div>
        )}

        {!loading && error && (
          <div
            className="card"
            style={{ maxWidth: 560, borderColor: 'var(--danger-soft, var(--border))' }}
          >
            <div style={{ display: 'flex', gap: 12, alignItems: 'flex-start', padding: '16px' }}>
              <div
                style={{
                  width: 32,
                  height: 32,
                  borderRadius: 'var(--radius)',
                  background: 'color-mix(in srgb, var(--danger) 12%, transparent)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'var(--danger)',
                  flexShrink: 0,
                }}
              >
                <Icon name="alert-triangle" size={15} />
              </div>
              <div>
                <div
                  style={{
                    fontWeight: 600,
                    fontSize: 'calc(13px * var(--font-scale, 1))',
                    marginBottom: 4,
                  }}
                >
                  Gagal memuat pengaturan
                </div>
                <div
                  style={{
                    fontSize: 'calc(12px * var(--font-scale, 1))',
                    color: 'var(--fg-muted)',
                  }}
                >
                  {error}
                </div>
              </div>
            </div>
          </div>
        )}

        {!loading && !error && settings.length === 0 && (
          <div className="card" style={{ maxWidth: 560 }}>
            <EmptyGroup group={group} />
          </div>
        )}

        {!loading && !error && settings.length > 0 && (
          <div className="card" style={{ maxWidth: 720 }}>
            <div className="card-h">
              <span
                style={{
                  display: 'inline-flex',
                  width: 22,
                  height: 22,
                  alignItems: 'center',
                  justifyContent: 'center',
                  background: 'var(--primary-soft)',
                  color: 'var(--primary-soft-fg)',
                  borderRadius: 5,
                }}
              >
                <Icon name="gear" size={12} />
              </span>
              <div>
                <div className="title">{title}</div>
                <div className="sub">
                  {settings.length} pengaturan
                  {dirtyKeys.length > 0 && ` · ${dirtyKeys.length} belum disimpan`}
                </div>
              </div>
            </div>
            <div className="card-b" style={{ paddingTop: 0 }}>
              {settings.map((s) => (
                <SettingRow
                  key={s.key}
                  setting={s}
                  value={values[s.key] ?? ''}
                  dirty={dirtyKeys.includes(s.key)}
                  onChange={(v) => handleChange(s.key, v)}
                />
              ))}
              <div style={{ paddingTop: 16, display: 'flex', gap: 8 }}>
                <button
                  className="btn primary"
                  onClick={handleSave}
                  disabled={saving}
                >
                  {saving ? 'Menyimpan…' : 'Simpan'}
                </button>
                <button
                  className="btn ghost"
                  onClick={handleReset}
                  disabled={dirtyKeys.length === 0 || saving}
                >
                  Reset
                </button>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Footer */}
      <div className="pager">
        <span className="muted">Administrator · {title}</span>
      </div>
    </div>
  );
}

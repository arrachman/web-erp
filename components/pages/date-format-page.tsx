'use client';

/**
 * Halaman admin /admin/date-format — atur token format tanggal global
 * (sys_settings `system/format/date_format`). Dipakai oleh DateInput &
 * formatDate (lib/date-format.ts). Lihat apps/web-erp/CLAUDE.md §2.31.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { DateInput } from '@/components/ui/date-input';
import { notify } from '@/lib/feedback';
import {
  getDateFormat,
  updateDateFormat,
  type DateFormat,
} from '@/lib/api/date-format';
import { invalidateDateFormatCache, formatDate } from '@/lib/date-format';

const PRESETS: Array<{ token: string; label: string; hint: string }> = [
  { token: 'DD/MM/YYYY', label: 'Hari/Bulan/Tahun', hint: '31/01/2026' },
  { token: 'DD-MM-YYYY', label: 'Hari-Bulan-Tahun', hint: '31-01-2026' },
  { token: 'MM/DD/YYYY', label: 'Bulan/Hari/Tahun (US)', hint: '01/31/2026' },
  { token: 'YYYY-MM-DD', label: 'Tahun-Bulan-Hari (ISO)', hint: '2026-01-31' },
  { token: 'DD MMMM YYYY', label: 'Hari Bulan-panjang Tahun', hint: '31 Januari 2026' },
  { token: 'D MMM YYYY', label: 'Hari Bulan-singkat Tahun', hint: '31 Jan 2026' },
];

const SAMPLE_ISO = '2026-01-31';

export function DateFormatPage() {
  const [loaded, setLoaded] = React.useState<DateFormat | null>(null);
  const [error, setError] = React.useState<string | null>(null);
  const [token, setToken] = React.useState<string>('DD/MM/YYYY');
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    let cancelled = false;
    getDateFormat()
      .then((f) => {
        if (cancelled) return;
        setLoaded(f);
        setToken(f.format);
      })
      .catch((e: unknown) => {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Gagal memuat format tanggal');
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const dirty = loaded ? token !== loaded.format : false;
  const preview = formatDate(SAMPLE_ISO, { format: token, example: '' });

  const handleSave = async () => {
    setSaving(true);
    try {
      const updated = await updateDateFormat(token);
      setLoaded(updated);
      invalidateDateFormatCache(updated);
      notify('Format tanggal disimpan', 'success');
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan format', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => {
    if (loaded) setToken(loaded.format);
  };

  return (
    <div className="page">
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
            }}
          >
            <Icon name="calendar" size={14} />
          </div>
          <h1 className="page-title" style={{ margin: 0 }}>Format Tanggal</h1>
        </div>
        <div className="page-actions">
          <button className="btn ghost" onClick={handleReset} disabled={!dirty || saving} type="button">
            <Icon name="refresh" size={12} />
            Reset
          </button>
          <button
            className="btn primary"
            onClick={handleSave}
            disabled={!dirty || saving}
            type="button"
          >
            <Icon name="save" size={12} />
            {saving ? 'Menyimpan…' : 'Simpan'}
          </button>
        </div>
      </div>

      <div className="scrollbar" style={{ flex: 1, overflow: 'auto', padding: '20px 24px' }}>
        {error && (
          <div className="card" style={{ maxWidth: 720, marginBottom: 16, borderColor: 'var(--danger-soft, var(--border))' }}>
            <div style={{ padding: 16, color: 'var(--danger)' }}>{error}</div>
          </div>
        )}

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
              <Icon name="calendar" size={12} />
            </span>
            <div>
              <div className="title">Format tampilan tanggal</div>
              <div className="sub">Berlaku global untuk semua field &amp; kolom tanggal di aplikasi.</div>
            </div>
          </div>

          <div className="card-b" style={{ paddingTop: 8 }}>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
                {PRESETS.map((p) => {
                  const active = p.token === token;
                  return (
                    <button
                      key={p.token}
                      type="button"
                      className="btn ghost"
                      onClick={() => setToken(p.token)}
                      style={{
                        justifyContent: 'space-between',
                        border: active ? '1px solid var(--primary)' : '1px solid var(--border)',
                        background: active ? 'var(--primary-soft)' : 'transparent',
                      }}
                    >
                      <span>{p.label}</span>
                      <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--fg-muted)' }}>{p.hint}</span>
                    </button>
                  );
                })}
              </div>

              <div
                style={{
                  background: 'var(--panel-2)',
                  border: '1px solid var(--border)',
                  borderRadius: 'var(--radius)',
                  padding: '14px 16px',
                }}
              >
                <div style={{ fontSize: 'calc(11px * var(--font-scale, 1))', color: 'var(--fg-muted)', textTransform: 'uppercase', letterSpacing: 0.5 }}>
                  Pratinjau
                </div>
                <div
                  style={{
                    fontFamily: 'var(--font-mono)',
                    fontSize: 'calc(20px * var(--font-scale, 1))',
                    fontWeight: 600,
                    color: 'var(--fg)',
                    marginTop: 4,
                  }}
                >
                  {preview}
                </div>
                <div style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))', color: 'var(--fg-muted)', marginTop: 8, marginBottom: 4 }}>
                  Contoh field tanggal:
                </div>
                <div style={{ maxWidth: 220 }}>
                  <DateInput value={SAMPLE_ISO} onChange={() => undefined} disabled />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="pager">
        <span className="muted">System · Format Tanggal (sys_settings &quot;system/format/date_format&quot;)</span>
      </div>
    </div>
  );
}

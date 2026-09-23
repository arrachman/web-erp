'use client';

/**
 * Halaman admin /admin/number-format — atur 3 knob sys_settings group
 * `number-format`: thousands sep, decimal sep, decimals default.
 * Lihat apps/web-erp/CLAUDE.md §2.31.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { notify } from '@/lib/feedback';
import {
  getNumberFormat,
  updateNumberFormat,
  type NumberFormat,
} from '@/lib/api/number-format';
import { invalidateNumberFormatCache } from '@/lib/format';

const THOUSANDS_OPTIONS: Array<{ value: string; label: string }> = [
  { value: '.', label: 'Titik  ( . )' },
  { value: ',', label: 'Koma  ( , )' },
  { value: ' ', label: 'Spasi  (   )' },
  { value: "'", label: 'Tanda kutip  ( ’ )' },
  { value: '', label: 'Tanpa pemisah' },
];

const DECIMAL_OPTIONS: Array<{ value: string; label: string }> = [
  { value: ',', label: 'Koma  ( , )' },
  { value: '.', label: 'Titik  ( . )' },
];

const PRESETS: Array<{ key: string; label: string; t: string; d: string; dec: number; hint: string }> = [
  { key: 'id', label: 'Indonesia (id-ID)', t: '.', d: ',', dec: 0, hint: '1.234.567' },
  { key: 'id2', label: 'Indonesia + 2 desimal', t: '.', d: ',', dec: 2, hint: '1.234.567,89' },
  { key: 'en', label: 'English (en-US)', t: ',', d: '.', dec: 0, hint: '1,234,567' },
  { key: 'en2', label: 'English + 2 desimal', t: ',', d: '.', dec: 2, hint: '1,234,567.89' },
  { key: 'plain', label: 'Tanpa pemisah', t: '', d: '.', dec: 0, hint: '1234567' },
];

function buildPreview(t: string, d: string, dec: number): string {
  const intPart = '1234567';
  const grouped = t ? intPart.replace(/\B(?=(\d{3})+(?!\d))/g, t) : intPart;
  return dec > 0 ? `${grouped}${d}${'8'.repeat(dec)}` : grouped;
}

export function NumberFormatPage() {
  const [loaded, setLoaded] = React.useState<NumberFormat | null>(null);
  const [error, setError] = React.useState<string | null>(null);
  const [thousandsSep, setThousandsSep] = React.useState<string>('.');
  const [decimalSep, setDecimalSep] = React.useState<string>(',');
  const [decimals, setDecimals] = React.useState<number>(0);
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    let cancelled = false;
    getNumberFormat()
      .then((f) => {
        if (cancelled) return;
        setLoaded(f);
        setThousandsSep(f.thousandsSep);
        setDecimalSep(f.decimalSep);
        setDecimals(f.decimals);
      })
      .catch((e: unknown) => {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Gagal memuat format angka');
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const preview = buildPreview(thousandsSep, decimalSep, decimals);
  const sepConflict = thousandsSep !== '' && thousandsSep === decimalSep;
  const dirty = loaded
    ? thousandsSep !== loaded.thousandsSep ||
      decimalSep !== loaded.decimalSep ||
      decimals !== loaded.decimals
    : false;

  const applyPreset = (t: string, d: string, dec: number) => {
    setThousandsSep(t);
    setDecimalSep(d);
    setDecimals(dec);
  };

  const handleSave = async () => {
    if (sepConflict) {
      notify('Pemisah ribuan dan desimal tidak boleh sama.', 'danger');
      return;
    }
    if (!Number.isInteger(decimals) || decimals < 0 || decimals > 6) {
      notify('Jumlah desimal harus integer 0–6.', 'danger');
      return;
    }
    setSaving(true);
    try {
      const updated = await updateNumberFormat({ thousandsSep, decimalSep, decimals });
      setLoaded(updated);
      invalidateNumberFormatCache(updated);
      notify('Format angka disimpan', 'success');
    } catch (e: unknown) {
      notify(e instanceof Error ? e.message : 'Gagal menyimpan format', 'danger');
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => {
    if (!loaded) return;
    setThousandsSep(loaded.thousandsSep);
    setDecimalSep(loaded.decimalSep);
    setDecimals(loaded.decimals);
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
            <Icon name="calculator" size={14} />
          </div>
          <h1 className="page-title" style={{ margin: 0 }}>Format Angka</h1>
        </div>
        <div className="page-actions">
          <button className="btn ghost" onClick={handleReset} disabled={!dirty || saving} type="button">
            <Icon name="refresh" size={12} />
            Reset
          </button>
          <button
            className="btn primary"
            onClick={handleSave}
            disabled={!dirty || saving || sepConflict}
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
              <Icon name="calculator" size={12} />
            </span>
            <div>
              <div className="title">Komposisi format angka</div>
              <div className="sub">Berlaku global untuk semua input numerik &amp; kolom angka di tabel.</div>
            </div>
          </div>

          <div className="card-b" style={{ paddingTop: 8 }}>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              <div style={{ display: 'grid', gridTemplateColumns: '260px 1fr', gap: 16, alignItems: 'center' }}>
                <label style={{ fontSize: 'calc(12.5px * var(--font-scale, 1))', color: 'var(--fg)' }}>
                  Pemisah ribuan
                </label>
                <Select
                  value={thousandsSep === '' ? '__empty__' : thousandsSep}
                  onValueChange={(v) => setThousandsSep(v === '__empty__' ? '' : v)}
                >
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {THOUSANDS_OPTIONS.map((o) => (
                      <SelectItem key={o.value || '__empty__'} value={o.value || '__empty__'}>{o.label}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '260px 1fr', gap: 16, alignItems: 'center' }}>
                <label style={{ fontSize: 'calc(12.5px * var(--font-scale, 1))', color: 'var(--fg)' }}>
                  Pemisah desimal
                </label>
                <Select value={decimalSep} onValueChange={setDecimalSep}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {DECIMAL_OPTIONS.map((o) => (
                      <SelectItem key={o.value} value={o.value}>{o.label}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '260px 1fr', gap: 16, alignItems: 'center' }}>
                <label style={{ fontSize: 'calc(12.5px * var(--font-scale, 1))', color: 'var(--fg)' }}>
                  Jumlah desimal default
                </label>
                <Input
                  type="number"
                  min={0}
                  max={6}
                  value={decimals}
                  onChange={(e) => setDecimals(Math.max(0, Math.min(6, Number(e.target.value) || 0)))}
                  style={{ width: 96, textAlign: 'center' }}
                />
              </div>

              {sepConflict && (
                <div style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))', color: 'var(--danger)' }}>
                  Pemisah ribuan dan desimal tidak boleh sama.
                </div>
              )}

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
                <div style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))', color: 'var(--fg-muted)', marginTop: 4 }}>
                  Contoh angka 1.234.567{decimals > 0 ? ` dengan ${decimals} digit desimal` : ''}
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="card" style={{ maxWidth: 720, marginTop: 16 }}>
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
              <Icon name="layers" size={12} />
            </span>
            <div>
              <div className="title">Preset cepat</div>
              <div className="sub">Klik salah satu untuk apply ke editor di atas.</div>
            </div>
          </div>
          <div className="card-b" style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
            {PRESETS.map((p) => {
              const active = p.t === thousandsSep && p.d === decimalSep && p.dec === decimals;
              return (
                <button
                  key={p.key}
                  type="button"
                  className="btn ghost"
                  onClick={() => applyPreset(p.t, p.d, p.dec)}
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
        </div>
      </div>

      <div className="pager">
        <span className="muted">System · Format Angka (sys_settings group &quot;number-format&quot;)</span>
      </div>
    </div>
  );
}

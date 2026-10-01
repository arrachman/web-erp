'use client';

/**
 * Right slide-over Aktivitas panel — ported from prototype
 * `notifications.jsx#ActivityPanel`. Listens to the
 * `toggle-activity` window event (dispatched by topbar activity icon)
 * and renders a filterable feed of `ACTIVITY` rows.
 */
import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { DrawerPanel } from '@/components/molecules/drawer-panel';
import { usePanelToggle } from '@/lib/drawer-toggle';
import { ACTIVITY, fmtIDR, tGlobal, type ActivityRow, type ActivityType } from '@/lib/mock';

type ActivityFilter = 'all' | ActivityType;

const FILTER_OPTIONS: ReadonlyArray<{ value: ActivityFilter; label: string }> = [
  { value: 'all', label: 'Semua' },
  { value: 'success', label: 'Sukses' },
  { value: 'danger', label: 'Ditolak/Batal' },
  { value: 'info', label: 'Info' },
  { value: 'warn', label: 'Edit' },
];

const SELECT_STYLE: React.CSSProperties = {
  height: 26,
  padding: '0 6px',
  background: 'var(--panel)',
  border: '1px solid var(--border)',
  borderRadius: 6,
  color: 'var(--fg)',
  font: 'inherit',
  fontSize: 'calc(11.5px * var(--font-scale, 1))',
};

const TARGET_STYLE: React.CSSProperties = {
  fontFamily: 'Geist Mono, monospace',
  fontSize: 'calc(11px * var(--font-scale, 1))',
};

function amountStyle(amount: number): React.CSSProperties {
  return {
    marginLeft: 6,
    fontFamily: 'Geist Mono, monospace',
    fontVariantNumeric: 'tabular-nums',
    color: amount > 0 ? 'var(--success)' : 'var(--danger)',
  };
}

export interface ActivityDrawerProps {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  t: (key: string) => string;
}

function ActivityRowView({ row }: { row: ActivityRow }): React.ReactElement {
  return (
    <div className={`activity-row ${row.type}`}>
      <span className="dot" />
      <div>
        <span className="who">{row.who}</span>{' '}
        <span className="meta">{row.what}</span>{' '}
        <span style={TARGET_STYLE}>{row.target}</span>
        {row.amount != null && (
          <span style={amountStyle(row.amount)}>
            {row.amount > 0 ? '+' : ''}
            {fmtIDR(row.amount)}
          </span>
        )}
      </div>
      <span className="ts">{row.ts}</span>
    </div>
  );
}

export function ActivityDrawer(_props: ActivityDrawerProps): React.ReactElement | null {
  const [open, setOpen] = usePanelToggle('toggle-activity');
  const [filter, setFilter] = React.useState<ActivityFilter>('all');

  const shown =
    filter === 'all' ? ACTIVITY : ACTIVITY.filter((a) => a.type === filter);

  const close = (): void => setOpen(false);

  return (
    <DrawerPanel
      open={open}
      onClose={close}
      title={tGlobal('Aktivitas')}
      icon="activity"
      sub={`${ACTIVITY.length} ${tGlobal('kejadian hari ini')}`}
      head={
        <select
          value={filter}
          onChange={(e) => setFilter(e.target.value as ActivityFilter)}
          style={SELECT_STYLE}
        >
          {FILTER_OPTIONS.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {tGlobal(opt.label)}
            </option>
          ))}
        </select>
      }
    >
      <div className="drawer-bd scrollbar">
        <div className="activity-list">
          {shown.map((a, i) => (
            // eslint-disable-next-line react/no-array-index-key
            <ActivityRowView key={i} row={a} />
          ))}
          {shown.length === 0 && (
            <div className="cm-empty" style={{ padding: 48 }}>
              <Icon name="activity" size={26} />
              <div className="muted" style={{ marginTop: 8 }}>
                {tGlobal('Tidak ada aktivitas')}
              </div>
            </div>
          )}
        </div>
      </div>
      <div className="drawer-ft">
        <button className="btn ghost" onClick={close}>
          {tGlobal('Tutup')} <Kbd>ESC</Kbd>
        </button>
      </div>
    </DrawerPanel>
  );
}

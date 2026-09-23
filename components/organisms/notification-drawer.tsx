'use client';

/**
 * Right slide-over Notifikasi panel — backed by /erp/notifications/me
 * (sys_notifications). Listens to the `toggle-notif` window event
 * (dispatched by topbar bell), keeps local state synced with API, and
 * re-broadcasts the unread count via `notif-count` so the topbar badge
 * stays in sync.
 */
import * as React from 'react';
import { Icon, type IconName } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { DrawerPanel } from '@/components/molecules/drawer-panel';
import { usePanelToggle } from '@/lib/drawer-toggle';
import { notify } from '@/lib/feedback';
import { tGlobal } from '@/lib/mock';
import {
  archiveNotification,
  listMyNotifications,
  markAllNotificationsRead,
  markNotificationRead,
  type ErpNotification,
} from '@/lib/api/notifications';

type Tab = 'all' | 'unread';

const TYPE_ICON: Record<string, IconName> = {
  success: 'check',
  info: 'info',
  warn: 'info',
  danger: 'x',
};

function iconFor(type: string): IconName {
  return TYPE_ICON[type] ?? 'info';
}

function relativeTime(iso: string): string {
  const now = Date.now();
  const then = new Date(iso).getTime();
  const diffSec = Math.max(0, Math.floor((now - then) / 1000));
  if (diffSec < 60) return 'baru saja';
  const min = Math.floor(diffSec / 60);
  if (min < 60) return `${min} mnt lalu`;
  const hr = Math.floor(min / 60);
  if (hr < 24) return `${hr} jam lalu`;
  const day = Math.floor(hr / 24);
  if (day === 1) return 'Kemarin';
  if (day < 7) return `${day} hari lalu`;
  return new Date(iso).toLocaleDateString('id-ID');
}

export interface NotificationDrawerProps {
  onNavigate: (route: string) => void;
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  t: (key: string) => string;
}

const TITLE_STYLE = (read: boolean): React.CSSProperties => ({
  fontSize: 'calc(12.5px * var(--font-scale, 1))',
  fontWeight: read ? 400 : 600,
  display: 'flex',
  alignItems: 'center',
  gap: 6,
});

const TS_STYLE: React.CSSProperties = {
  fontSize: 'calc(10.5px * var(--font-scale, 1))',
  color: 'var(--fg-faint)',
  marginTop: 3,
  fontFamily: 'Geist Mono, monospace',
};

const UNREAD_DOT_STYLE: React.CSSProperties = {
  width: 6,
  height: 6,
  borderRadius: '50%',
  background: 'var(--primary)',
};

function NotifRow({
  item,
  onClick,
  onDismiss,
}: {
  item: ErpNotification;
  onClick: () => void;
  onDismiss: (e: React.MouseEvent) => void;
}): React.ReactElement {
  const read = !!item.readAt;
  return (
    <div className="notif-row" onClick={onClick}>
      <span
        className={`confirm-ic ${item.type}`}
        style={{ width: 30, height: 30 }}
      >
        <Icon name={iconFor(item.type)} size={14} />
      </span>
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={TITLE_STYLE(read)}>
          {item.title}
          {!read && <span style={UNREAD_DOT_STYLE} />}
        </div>
        <div className="muted" style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))', marginTop: 2 }}>
          {item.body}
        </div>
        <div style={TS_STYLE}>{relativeTime(item.createdAt)}</div>
      </div>
      {item.actionUrl && (
        <Icon name="chevright" size={11} className="muted" />
      )}
      <button
        className="notif-dismiss"
        onClick={onDismiss}
        aria-label="Tutup notifikasi"
        title="Tutup"
      >
        <Icon name="x" size={11} />
      </button>
    </div>
  );
}

export function NotificationDrawer({
  onNavigate,
}: NotificationDrawerProps): React.ReactElement | null {
  const [open, setOpen] = usePanelToggle('toggle-notif');
  const [items, setItems] = React.useState<ErpNotification[]>([]);
  const [tab, setTab] = React.useState<Tab>('all');
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  const unread = items.filter((n) => !n.readAt).length;

  React.useEffect(() => {
    window.dispatchEvent(
      new CustomEvent<number>('notif-count', { detail: unread }),
    );
  }, [unread]);

  const reload = React.useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const { data } = await listMyNotifications({ limit: 50 });
      setItems(data);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Gagal memuat notifikasi';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  // Initial load + refetch tiap kali drawer dibuka.
  React.useEffect(() => {
    if (open) reload();
  }, [open, reload]);

  // Sekali saat mount supaya badge topbar punya angka tanpa harus buka drawer.
  // useRef guard mencegah double-fetch saat AppShell remount akibat redirect chain (/org → /dashboard).
  const hasFetched = React.useRef(false);
  React.useEffect(() => {
    if (hasFetched.current) return;
    hasFetched.current = true;
    reload();
  }, [reload]);

  const shown = tab === 'unread' ? items.filter((n) => !n.readAt) : items;

  const close = (): void => setOpen(false);

  const markAll = async (): Promise<void> => {
    try {
      await markAllNotificationsRead();
      const now = new Date().toISOString();
      setItems((prev) => prev.map((n) => ({ ...n, readAt: n.readAt ?? now })));
      notify('Semua notifikasi ditandai dibaca', 'info');
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Gagal menandai';
      notify(msg, 'danger');
    }
  };

  const openItem = async (n: ErpNotification): Promise<void> => {
    if (!n.readAt) {
      try {
        const updated = await markNotificationRead(n.id);
        setItems((prev) => prev.map((x) => (x.id === n.id ? updated : x)));
      } catch {
        // ignore; tetap navigate
      }
    }
    if (n.actionUrl) {
      onNavigate(n.actionUrl);
      setOpen(false);
    }
  };

  const dismissItem = async (e: React.MouseEvent, n: ErpNotification): Promise<void> => {
    e.stopPropagation();
    try {
      await archiveNotification(n.id);
      setItems((prev) => prev.filter((x) => x.id !== n.id));
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Gagal menutup notifikasi';
      notify(msg, 'danger');
    }
  };

  return (
    <DrawerPanel
      open={open}
      onClose={close}
      title={tGlobal('Notifikasi')}
      icon="bell"
      sub={`${unread} ${tGlobal('belum dibaca')}`}
      head={
        <button
          className="btn ghost sm"
          onClick={markAll}
          disabled={unread === 0}
        >
          <Icon name="check" size={11} /> {tGlobal('Tandai semua')}
        </button>
      }
    >
      <div className="tabs" style={{ padding: '0 8px' }}>
        <button
          className={`tab ${tab === 'all' ? 'active' : ''}`}
          onClick={() => setTab('all')}
        >
          {tGlobal('Semua')} <span className="muted">{items.length}</span>
        </button>
        <button
          className={`tab ${tab === 'unread' ? 'active' : ''}`}
          onClick={() => setTab('unread')}
        >
          {tGlobal('Belum dibaca')} <span className="muted">{unread}</span>
        </button>
      </div>
      <div className="drawer-bd scrollbar" style={{ padding: 0, gap: 0 }}>
        {loading && (
          <div className="cm-empty" style={{ padding: 32 }}>
            <div className="muted">{tGlobal('Memuat...')}</div>
          </div>
        )}
        {!loading && error && (
          <div className="cm-empty" style={{ padding: 32 }}>
            <Icon name="info" size={26} />
            <div className="muted" style={{ marginTop: 8 }}>{error}</div>
            <button className="btn ghost sm" style={{ marginTop: 12 }} onClick={reload}>
              <Icon name="refresh" size={11} /> {tGlobal('Coba lagi')}
            </button>
          </div>
        )}
        {!loading && !error && shown.length === 0 && (
          <div className="cm-empty" style={{ padding: 48 }}>
            <Icon name="bell" size={26} />
            <div className="muted" style={{ marginTop: 8 }}>
              {tGlobal('Tidak ada notifikasi')}
            </div>
          </div>
        )}
        {!loading && !error && shown.map((n) => (
          <NotifRow
            key={n.id}
            item={n}
            onClick={() => void openItem(n)}
            onDismiss={(e) => void dismissItem(e, n)}
          />
        ))}
      </div>
      <div className="drawer-ft">
        <button className="btn ghost" onClick={close}>
          {tGlobal('Tutup')} <Kbd>ESC</Kbd>
        </button>
        <button
          className="btn"
          onClick={() => {
            onNavigate('home');
            setOpen(false);
          }}
        >
          {tGlobal('Buka Dashboard')}
        </button>
      </div>
    </DrawerPanel>
  );
}

// Keep export shape compatible with previous imports (NotificationItem,
// NotificationType) by re-exporting useful API types.
export type { ErpNotification as NotificationItem } from '@/lib/api/notifications';
export type NotificationType = 'success' | 'info' | 'danger' | 'warn';

// Re-export for unit tests that need to detect archive action.
export { archiveNotification };

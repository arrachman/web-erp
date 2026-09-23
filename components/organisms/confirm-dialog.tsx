'use client';

import * as React from 'react';
import { Icon, type IconName } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { tGlobal } from '@/lib/mock';
import type {
  ConfirmItem,
  ConfirmOptions,
  ConfirmVariant,
} from '@/lib/feedback';

// Re-export types so callers can import either from `@/lib/feedback`
// or `@/components/organisms/confirm-dialog`. Types live in
// `lib/feedback.ts` to avoid a circular import (feedback dispatches the
// event; this component only consumes it).
export type { ConfirmItem, ConfirmOptions, ConfirmVariant };

const DEFAULT_ICON: Record<ConfirmVariant, IconName> = {
  danger: 'trash',
  warn: 'info',
  success: 'check',
  primary: 'check',
};

/**
 * Mount once inside the app shell. Listens to the global `app-confirm`
 * custom event (dispatched by `confirmAction` in `lib/feedback.ts`) and
 * renders a modal confirmation dialog. ESC cancels, Enter confirms,
 * backdrop click cancels.
 *
 * Atomic tier: **Organism**. CSS classes come from `styles/erp-components.css`
 * (`.cp-backdrop`, `.confirm-card`, `.confirm-hd`, `.confirm-ic`, `.confirm-title`,
 * `.confirm-msg`, `.confirm-body`, `.confirm-list`, `.confirm-ft`).
 */
export function ConfirmDialogHost(): React.ReactElement | null {
  const [cfg, setCfg] = React.useState<ConfirmOptions | null>(null);

  // Subscribe to the global confirm event. Detail carries the full opts.
  React.useEffect(() => {
    const handler = (event: Event) => {
      const detail = (event as CustomEvent<ConfirmOptions>).detail;
      setCfg(detail ?? {});
    };
    window.addEventListener('app-confirm', handler as EventListener);
    return () => {
      window.removeEventListener('app-confirm', handler as EventListener);
    };
  }, []);

  // Flag the global overlay state so list pages can pause their own
  // keyboard handlers (mirrors prototype `window.__overlay`).
  React.useEffect(() => {
    (window as unknown as { __overlay?: boolean }).__overlay = !!cfg;
    return () => {
      (window as unknown as { __overlay?: boolean }).__overlay = false;
    };
  }, [cfg]);

  const close = React.useCallback(() => setCfg(null), []);

  const doConfirm = React.useCallback(() => {
    const fn = cfg?.onConfirm;
    setCfg(null);
    fn?.();
  }, [cfg]);

  // ESC = close, Enter = confirm. Capture phase so the dialog wins over
  // any underlying list keyboard handlers.
  React.useEffect(() => {
    if (!cfg) return undefined;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        e.preventDefault();
        close();
      } else if (e.key === 'Enter') {
        e.preventDefault();
        doConfirm();
      }
    };
    window.addEventListener('keydown', onKey, true);
    return () => window.removeEventListener('keydown', onKey, true);
  }, [cfg, close, doConfirm]);

  if (!cfg) return null;

  const variant: ConfirmVariant = cfg.variant ?? 'primary';
  const icon: IconName = cfg.icon ?? DEFAULT_ICON[variant];
  const confirmBtnClass = variant === 'danger' ? 'btn danger' : 'btn primary';

  const onBackdropMouseDown = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) close();
  };

  return (
    <div
      className="cp-backdrop"
      style={{ zIndex: 750 }}
      onMouseDown={onBackdropMouseDown}
    >
      <div className="confirm-card fade-in" role="alertdialog" aria-modal="true">
        <div className="confirm-hd">
          <span className={`confirm-ic ${variant}`}>
            <Icon name={icon} size={16} />
          </span>
          <div style={{ flex: 1 }}>
            <div className="confirm-title">{tGlobal(cfg.title ?? 'Konfirmasi')}</div>
            {cfg.message && <div className="confirm-msg">{tGlobal(cfg.message)}</div>}
          </div>
        </div>
        {cfg.items && cfg.items.length > 0 && (
          <div className="confirm-body">
            <div className="confirm-list scrollbar">
              {cfg.items.map((it, i) => (
                <div key={i} className="row">
                  <span>{it.label}</span>
                  {it.val != null && (
                    <span style={{ fontFamily: 'Geist Mono, monospace' }}>
                      {it.val}
                    </span>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}
        <div className="confirm-ft">
          <button type="button" className="btn ghost" onClick={close}>
            {tGlobal(cfg.cancelLabel ?? 'Batal')} <Kbd>ESC</Kbd>
          </button>
          <button
            type="button"
            className={confirmBtnClass}
            autoFocus
            onClick={doConfirm}
          >
            {cfg.confirmIcon && <Icon name={cfg.confirmIcon} size={12} />}{' '}
            {tGlobal(cfg.confirmLabel ?? 'Konfirmasi')} <Kbd>↵</Kbd>
          </button>
        </div>
      </div>
    </div>
  );
}

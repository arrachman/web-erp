'use client';

/**
 * Shared shell for right slide-over drawers (Notifikasi / Aktivitas).
 * Renders the backdrop + animated panel surface with a header
 * (icon + title + sub + optional head slot + close button). The body
 * + footer are caller-owned via `children` so each drawer keeps its
 * own scroll/empty/footer layout.
 *
 * Pure presentational Molecule — no business logic, no event wiring.
 */
import * as React from 'react';
import { Icon, type IconName } from '@/components/ui/icons';

export interface DrawerPanelProps {
  open: boolean;
  onClose: () => void;
  title: string;
  icon: IconName;
  sub: string;
  /** Optional element rendered between the title block and close button. */
  head?: React.ReactNode;
  children: React.ReactNode;
}

const HD_ICON_STYLE: React.CSSProperties = {
  display: 'inline-flex',
  width: 26,
  height: 26,
  alignItems: 'center',
  justifyContent: 'center',
  background: 'var(--primary-soft)',
  color: 'var(--primary-soft-fg)',
  borderRadius: 6,
};

export function DrawerPanel({
  open,
  onClose,
  title,
  icon,
  sub,
  head,
  children,
}: DrawerPanelProps): React.ReactElement | null {
  if (!open) return null;
  return (
    <div
      className="drawer-backdrop"
      onMouseDown={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <div
        className="drawer panel-drawer"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="drawer-hd">
          <span style={HD_ICON_STYLE}>
            <Icon name={icon} size={13} />
          </span>
          <div style={{ flex: 1 }}>
            <div className="ti">{title}</div>
            <div className="muted" style={{ fontSize: 'calc(11px * var(--font-scale, 1))' }}>
              {sub}
            </div>
          </div>
          {head}
          <button className="iconbtn" onClick={onClose} aria-label="Tutup">
            <Icon name="x" size={13} />
          </button>
        </div>
        {children}
      </div>
    </div>
  );
}

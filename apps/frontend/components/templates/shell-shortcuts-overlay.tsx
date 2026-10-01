/**
 * ShortcutsOverlay — modal overlay that lists all keyboard shortcuts available
 * in the ERP shell. Rendered by AppShell when `shortcutsOpen` is true.
 */

import * as React from 'react';
import { tGlobal } from '@/lib/mock';

interface ShortcutsOverlayProps {
  onClose: () => void;
  urlRoutingEnabled?: boolean;
}

export function ShortcutsOverlay({ onClose, urlRoutingEnabled }: ShortcutsOverlayProps) {
  // Close on Escape key.
  React.useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [onClose]);

  return (
    <div className="sc-overlay" onClick={onClose}>
      <div className="sc-card" onClick={(e) => e.stopPropagation()}>
        <h3>{tGlobal('Keyboard Shortcuts')}</h3>
        <div className="sc-grid">
          <div>{tGlobal('Buka command palette')}</div>
          <div>⌘ K</div>
          {!urlRoutingEnabled && (<>
          <div>{tGlobal('Tutup tab aktif')}</div>
          <div>⌥ W</div>
          <div>{tGlobal('Pindah ke tab 1–8')}</div>
          <div>⌘ 1–8</div>
          <div>{tGlobal('Pindah ke tab terakhir')}</div>
          <div>⌘ 9</div>
          </>)}
          <div>{tGlobal('Cycle bahasa ID/EN/JA')}</div>
          <div>L</div>
          <div>{tGlobal('Tampilkan shortcut')}</div>
          <div>?</div>
          <div>{tGlobal('Tutup overlay')}</div>
          <div>ESC</div>
        </div>
        <div style={{ marginTop: 16, textAlign: 'right' }}>
          <button className="btn" onClick={onClose}>
            {tGlobal('Tutup')} ESC
          </button>
        </div>
      </div>
    </div>
  );
}

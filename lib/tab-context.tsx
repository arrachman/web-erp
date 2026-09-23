'use client';

import * as React from 'react';

/**
 * True when the consuming page lives in the *active* tab. Ported from
 * prototype `ui.jsx` `TabActiveContext`. The shell mounts every tab
 * (inactive ones `display:none`), so per-page keyboard handlers must
 * detach when their tab is hidden — otherwise all mounted tabs react to
 * the same keystroke. Default `true` so app-level shortcuts still work.
 */
export const TabActiveContext = React.createContext(true);

/**
 * Keydown subscription that auto-detaches when the page's tab is
 * inactive. Mirrors prototype `useKey`.
 */
export function useTabKey(handler: (e: KeyboardEvent) => void): void {
  const active = React.useContext(TabActiveContext);
  React.useEffect(() => {
    if (!active) return undefined;
    const fn = (e: KeyboardEvent) => handler(e);
    window.addEventListener('keydown', fn);
    return () => window.removeEventListener('keydown', fn);
  }, [active, handler]);
}

/**
 * Drawer toggle hook — drives the right-side slide-over panels
 * (Notifikasi / Aktivitas) from a window CustomEvent.
 *
 * Mirrors the prototype `usePanel(eventName)` hook, minus the
 * `window.__overlay` flag (no longer needed; panels manage their own
 * backdrop locally).
 */
import * as React from 'react';

/**
 * Subscribe to a window CustomEvent. Each dispatch toggles `open`.
 * When `open`, an ESC keydown closes the panel. Cleans up on unmount.
 *
 * @example
 *   const [open, setOpen] = usePanelToggle('toggle-notif');
 */
export function usePanelToggle(
  eventName: string,
): [boolean, (next: boolean) => void] {
  const [open, setOpen] = React.useState<boolean>(false);

  React.useEffect(() => {
    const onToggle = (): void => setOpen((prev) => !prev);
    window.addEventListener(eventName, onToggle);
    return () => window.removeEventListener(eventName, onToggle);
  }, [eventName]);

  React.useEffect(() => {
    if (!open) return undefined;
    const onKey = (e: KeyboardEvent): void => {
      if (e.key === 'Escape') setOpen(false);
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [open]);

  return [open, setOpen];
}

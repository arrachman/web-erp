/**
 * Pure DOM focus helper — move focus to the next focusable element after
 * `trigger`. Decoupled from React refs so it can be unit-tested in isolation.
 *
 * Used by both inline-Enter auto-pick and modal submit (confirm/confirmRow)
 * so focus advances A → B after the search modal closes.
 */
const FOCUSABLE_SELECTOR = [
  'input:not([disabled]):not([type="hidden"])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  'button:not([disabled])',
  'a[href]',
  '[tabindex]:not([tabindex="-1"])',
].join(', ');

export function listFocusable(root: ParentNode = document): HTMLElement[] {
  return Array.from(root.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR)).filter(
    (el) => {
      if (el.tabIndex === -1) return false;
      if (el.hasAttribute('hidden') || el.getAttribute('aria-hidden') === 'true') return false;
      // Skip elements inside a closed/inert dialog residual, if any remain.
      if (el.closest('[aria-hidden="true"]')) return false;
      return true;
    },
  );
}

export function focusNextFrom(trigger: HTMLElement | null): void {
  if (!trigger) return;
  const focusable = listFocusable();
  let idx = focusable.indexOf(trigger);
  if (idx === -1) {
    // Trigger may be nested; find nearest focusable ancestor.
    const owner = trigger.closest(FOCUSABLE_SELECTOR) as HTMLElement | null;
    idx = owner ? focusable.indexOf(owner) : -1;
  }
  if (idx === -1) return;
  const next = focusable[idx + 1];
  if (!next) return;
  next.focus();
  if (next instanceof HTMLInputElement && next.type !== 'checkbox' && next.type !== 'radio') {
    try {
      next.select?.();
    } catch {
      // select() can throw on non-text inputs in some browsers — ignore.
    }
  }
}

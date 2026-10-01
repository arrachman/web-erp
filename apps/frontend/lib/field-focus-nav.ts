/**
 * Arrow-key field navigation for forms.
 *
 * ArrowDown / ArrowUp move focus to the next / previous focusable field within
 * a container, mirroring Tab / Shift+Tab. Build a handler with
 * `createArrowFieldNav()` and attach it to a form section's `onKeyDown`; it only
 * fires from single-line text-like inputs where arrows are otherwise inert.
 * Controls that own their arrow keys — number steppers, radios, selects,
 * textareas, and anything with an open popup (`aria-expanded="true"`) — keep
 * native behavior and are skipped as sources.
 *
 * When focus is on the last/first field and an exit hook is supplied, the
 * navigation hands off to an adjacent region (e.g. ArrowDown from the last
 * header field jumps into the detail grid).
 */

const FOCUSABLE_SELECTOR = [
  'input:not([disabled]):not([type="hidden"])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  'button:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
].join(',');

/** Focusable descendants of `container`, in DOM (= tab) order. */
function focusableWithin(container: HTMLElement): HTMLElement[] {
  return Array.from(container.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR))
    .filter((el) => el.tabIndex !== -1 && el.offsetParent !== null);
}

/** True when arrows are inert on `el`, so they may be repurposed for navigation. */
function isArrowNavSource(el: EventTarget | null): el is HTMLElement {
  if (!(el instanceof HTMLElement)) return false;
  if (el.getAttribute('aria-expanded') === 'true') return false; // open combobox/menu owns arrows
  const tag = el.tagName;
  if (tag === 'TEXTAREA' || tag === 'SELECT') return false;
  if (tag !== 'INPUT') return false;
  const type = (el as HTMLInputElement).type;
  // number/range step, radio moves within the group — leave those alone.
  return type !== 'number' && type !== 'range' && type !== 'radio';
}

function focusField(el: HTMLElement): void {
  el.focus();
  if (el instanceof HTMLInputElement && el.type !== 'checkbox') el.select?.();
}

export interface ArrowFieldNavOptions {
  /** Called when ArrowDown is pressed on the last field. Return false if it could not move focus. */
  onForwardExit?: () => boolean | void;
  /** Called when ArrowUp is pressed on the first field. Return false if it could not move focus. */
  onBackwardExit?: () => boolean | void;
}

/**
 * `onKeyDown` handler: ArrowDown/ArrowUp move focus to the next/previous field
 * within `e.currentTarget`. No wrap (matches Tab/Shift+Tab). Call this from an
 * inline `onKeyDown` so the exit hooks run at event time, not during render.
 * Scope it to a section that does NOT contain a control with its own arrow
 * navigation (e.g. a data grid) — attach to the header container only, and use
 * the exit hooks to hand off into adjacent regions.
 */
export function arrowFieldNav(e: React.KeyboardEvent<HTMLElement>, opts: ArrowFieldNavOptions = {}): void {
  if (e.key !== 'ArrowDown' && e.key !== 'ArrowUp') return;
  if (!isArrowNavSource(e.target)) return;

  const fields = focusableWithin(e.currentTarget);
  const idx = fields.indexOf(e.target);
  if (idx === -1) return;

  const forward = e.key === 'ArrowDown';
  const next = fields[forward ? idx + 1 : idx - 1];
  if (next) {
    e.preventDefault();
    focusField(next);
    return;
  }

  // Off the end → optional handoff to an adjacent region (e.g. detail grid).
  const exit = forward ? opts.onForwardExit : opts.onBackwardExit;
  if (exit && exit() !== false) e.preventDefault();
}

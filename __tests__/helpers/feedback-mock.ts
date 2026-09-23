// Factory for the `@/lib/feedback` mock used across page tests.
//
// We expose vi.fn() spies for `notify` and `confirmAction`. The default
// `confirmAction` implementation invokes `opts.onConfirm()` immediately so
// destructive-path tests don't need to interact with the portal-mounted
// confirm dialog host.
//
// Usage:
//   vi.mock('@/lib/feedback', () => createFeedbackMock());
//
// Named exports only.

import { vi } from 'vitest';

export function createFeedbackMock() {
  return {
    notify: vi.fn(),
    confirmAction: vi.fn((opts: { onConfirm?: () => void | Promise<void> }) => {
      void opts.onConfirm?.();
    }),
    bulkAction: vi.fn(),
  };
}

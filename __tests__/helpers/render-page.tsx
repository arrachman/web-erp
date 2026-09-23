// Shared test utilities for ERP page smoke tests.
//
// Provides:
//   - emptyList(): canonical empty PaginatedResponse for list endpoints
//   - renderPage(): wraps @testing-library/react render with a sane default
//     (returns the testing-library result object so consumers can destructure)
//
// Named exports only (no default). Keep this file small — it's referenced by
// 24+ smoke tests, so cross-test breakage cost is high.

import { render, type RenderOptions, type RenderResult } from '@testing-library/react';
import type { ReactElement } from 'react';

export interface EmptyListShape {
  success: true;
  data: never[];
  meta: { page: 1; limit: 20; total: 0; totalPages: 0 };
}

export function emptyList(): EmptyListShape {
  return {
    success: true,
    data: [],
    meta: { page: 1, limit: 20, total: 0, totalPages: 0 },
  };
}

export function renderPage(
  ui: ReactElement,
  options?: RenderOptions,
): RenderResult {
  return render(ui, options);
}

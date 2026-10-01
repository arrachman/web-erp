import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listFiscalPeriods: vi.fn(),
  createFiscalPeriod: vi.fn(),
  updateFiscalPeriod: vi.fn(),
  deleteFiscalPeriod: vi.fn(),
}));
vi.mock('@/lib/api/fiscal-periods', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpFiscalPeriodsPage } from '@/components/pages/fiscal-periods-page';

describe('ErpFiscalPeriodsPage (smoke)', () => {
  it('renders title and calls listFiscalPeriods', async () => {
    api.listFiscalPeriods.mockResolvedValue(emptyList());
    renderPage(<ErpFiscalPeriodsPage />);
    expect(screen.getByText('Periode Fiskal')).toBeInTheDocument();
    await waitFor(() => expect(api.listFiscalPeriods).toHaveBeenCalled());
  });
});

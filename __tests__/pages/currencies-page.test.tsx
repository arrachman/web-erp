import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listCurrencies: vi.fn(),
  createCurrency: vi.fn(),
  updateCurrency: vi.fn(),
  deleteCurrency: vi.fn(),
  bulkUpdateErpCurrencyStatus: vi.fn(),
  bulkDeleteErpCurrencies: vi.fn(),
  listCurrencyRates: vi.fn(),
  upsertCurrencyRate: vi.fn(),
  deleteCurrencyRate: vi.fn(),
}));
vi.mock('@/lib/api/currencies', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpCurrenciesPage } from '@/components/pages/currencies-page';

describe('ErpCurrenciesPage (smoke)', () => {
  it('renders title and calls listCurrencies', async () => {
    api.listCurrencies.mockResolvedValue(emptyList());
    api.listCurrencyRates.mockResolvedValue(emptyList());
    renderPage(<ErpCurrenciesPage />);
    expect(screen.getByText('Mata Uang')).toBeInTheDocument();
    await waitFor(() => expect(api.listCurrencies).toHaveBeenCalled());
  });
});

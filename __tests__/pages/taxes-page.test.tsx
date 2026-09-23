import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const taxesApi = vi.hoisted(() => ({
  listTaxes: vi.fn(),
  createTax: vi.fn(),
  updateTax: vi.fn(),
  deleteTax: vi.fn(),
}));
const accountsApi = vi.hoisted(() => ({ listAccounts: vi.fn() }));
vi.mock('@/lib/api/taxes', () => taxesApi);
vi.mock('@/lib/api/accounts', () => accountsApi);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpTaxesPage } from '@/components/pages/taxes-page';

describe('ErpTaxesPage (smoke)', () => {
  it('renders title and calls listTaxes', async () => {
    taxesApi.listTaxes.mockResolvedValue(emptyList());
    accountsApi.listAccounts.mockResolvedValue(emptyList());
    renderPage(<ErpTaxesPage />);
    expect(screen.getByText('Pajak')).toBeInTheDocument();
    await waitFor(() => expect(taxesApi.listTaxes).toHaveBeenCalled());
  });
});

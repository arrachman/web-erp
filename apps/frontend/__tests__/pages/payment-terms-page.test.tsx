import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listPaymentTerms: vi.fn(),
  createPaymentTerm: vi.fn(),
  updatePaymentTerm: vi.fn(),
  deletePaymentTerm: vi.fn(),
  bulkUpdateErpPaymentTermStatus: vi.fn(),
  bulkDeleteErpPaymentTerms: vi.fn(),
}));
vi.mock('@/lib/api/payment-terms', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpPaymentTermsPage } from '@/components/pages/payment-terms-page';

describe('ErpPaymentTermsPage (smoke)', () => {
  it('renders title and calls listPaymentTerms', async () => {
    api.listPaymentTerms.mockResolvedValue(emptyList());
    renderPage(<ErpPaymentTermsPage />);
    expect(screen.getByText('Termin Pembayaran')).toBeInTheDocument();
    await waitFor(() => expect(api.listPaymentTerms).toHaveBeenCalled());
  });
});

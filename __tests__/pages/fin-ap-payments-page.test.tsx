import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listApPayments: vi.fn(),
  createApPayment: vi.fn(),
  updateApPayment: vi.fn(),
  deleteApPayment: vi.fn(),
}));
vi.mock('@/lib/api/fin-ap-payments', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpApPaymentsPage } from '@/components/pages/fin-ap-payments-page';

describe('ErpApPaymentsPage (smoke)', () => {
  it('renders title and calls listApPayments', async () => {
    api.listApPayments.mockResolvedValue(emptyList());
    renderPage(<ErpApPaymentsPage />);
    expect(screen.getByText('AP Payments')).toBeInTheDocument();
    await waitFor(() => expect(api.listApPayments).toHaveBeenCalled());
  });
});

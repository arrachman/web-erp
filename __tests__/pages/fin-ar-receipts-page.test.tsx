import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listArReceipts: vi.fn(),
  createArReceipt: vi.fn(),
  updateArReceipt: vi.fn(),
  deleteArReceipt: vi.fn(),
}));
vi.mock('@/lib/api/fin-ar-receipts', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpArReceiptsPage } from '@/components/pages/fin-ar-receipts-page';

describe('ErpArReceiptsPage (smoke)', () => {
  it('renders title and calls listArReceipts', async () => {
    api.listArReceipts.mockResolvedValue(emptyList());
    renderPage(<ErpArReceiptsPage />);
    expect(screen.getByText('AR Receipts')).toBeInTheDocument();
    await waitFor(() => expect(api.listArReceipts).toHaveBeenCalled());
  });
});

import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listLedgerEntries: vi.fn(),
}));
vi.mock('@/lib/api/fin-ledger', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpLedgerPage } from '@/components/pages/fin-ledger-page';

describe('ErpLedgerPage (smoke)', () => {
  it('renders title and calls listLedgerEntries', async () => {
    api.listLedgerEntries.mockResolvedValue(emptyList());
    renderPage(<ErpLedgerPage />);
    expect(screen.getByText('General Ledger')).toBeInTheDocument();
    await waitFor(() => expect(api.listLedgerEntries).toHaveBeenCalled());
  });
});

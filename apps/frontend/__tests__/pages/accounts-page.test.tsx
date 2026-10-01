import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  ACCOUNT_TYPES: ['ASSET', 'LIABILITY', 'EQUITY', 'REVENUE', 'EXPENSE'],
  ACCOUNT_KINDS: ['HEADER', 'POSTABLE'],
  CASH_FLOW_CATEGORIES: ['OPERATING', 'INVESTING', 'FINANCING'],
  getAccountCodeFormat: vi.fn(),
  listAccounts: vi.fn(),
  createAccount: vi.fn(),
  updateAccount: vi.fn(),
  deleteAccount: vi.fn(),
  bulkUpdateAccountStatus: vi.fn(),
  bulkDeleteAccounts: vi.fn(),
}));
vi.mock('@/lib/api/accounts', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpAccountsPage } from '@/components/pages/accounts-page';

describe('ErpAccountsPage (smoke)', () => {
  it('renders title and calls listAccounts', async () => {
    api.listAccounts.mockResolvedValue(emptyList());
    renderPage(<ErpAccountsPage />);
    expect(screen.getByText('Bagan Akun')).toBeInTheDocument();
    await waitFor(() => expect(api.listAccounts).toHaveBeenCalled());
  });
});

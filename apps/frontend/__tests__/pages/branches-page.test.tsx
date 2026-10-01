import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listBranches: vi.fn(),
  createBranch: vi.fn(),
  updateBranch: vi.fn(),
  deleteBranch: vi.fn(),
}));
vi.mock('@/lib/api/branches', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpBranchesPage } from '@/components/pages/branches-page';

describe('ErpBranchesPage (smoke)', () => {
  it('renders title and calls listBranches', async () => {
    api.listBranches.mockResolvedValue(emptyList());
    renderPage(<ErpBranchesPage />);
    expect(screen.getByText('Cabang')).toBeInTheDocument();
    await waitFor(() => expect(api.listBranches).toHaveBeenCalled());
  });
});

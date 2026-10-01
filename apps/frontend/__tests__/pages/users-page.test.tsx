import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listUsers: vi.fn(),
  createUser: vi.fn(),
  updateUser: vi.fn(),
  deleteUser: vi.fn(),
}));
vi.mock('@/lib/api/users', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpUsersPage } from '@/components/pages/users-page';

describe('ErpUsersPage (smoke)', () => {
  it('renders title and calls listUsers', async () => {
    api.listUsers.mockResolvedValue(emptyList());
    renderPage(<ErpUsersPage />);
    expect(screen.getByText('Users')).toBeInTheDocument();
    await waitFor(() => expect(api.listUsers).toHaveBeenCalled());
  });
});

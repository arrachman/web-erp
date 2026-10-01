import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listRoles: vi.fn(),
  createRole: vi.fn(),
  updateRole: vi.fn(),
  deleteRole: vi.fn(),
}));
vi.mock('@/lib/api/roles', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpRolesPage } from '@/components/pages/roles-page';

describe('ErpRolesPage (smoke)', () => {
  it('renders title and calls listRoles', async () => {
    api.listRoles.mockResolvedValue(emptyList());
    renderPage(<ErpRolesPage />);
    expect(screen.getByText('Roles')).toBeInTheDocument();
    await waitFor(() => expect(api.listRoles).toHaveBeenCalled());
  });
});

import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listPermissions: vi.fn(),
}));
vi.mock('@/lib/api/permissions', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpPermissionsPage } from '@/components/pages/permissions-page';

describe('ErpPermissionsPage (smoke)', () => {
  it('renders title and calls listPermissions', async () => {
    api.listPermissions.mockResolvedValue(emptyList());
    renderPage(<ErpPermissionsPage />);
    expect(screen.getByText('Hak Akses')).toBeInTheDocument();
    await waitFor(() => expect(api.listPermissions).toHaveBeenCalled());
  });
});

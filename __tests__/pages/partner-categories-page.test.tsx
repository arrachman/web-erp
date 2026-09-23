import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listPartnerCategories: vi.fn(),
  createPartnerCategory: vi.fn(),
  updatePartnerCategory: vi.fn(),
  deletePartnerCategory: vi.fn(),
}));
vi.mock('@/lib/api/partner-categories', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpPartnerCategoriesPage } from '@/components/pages/partner-categories-page';

describe('ErpPartnerCategoriesPage (smoke)', () => {
  it('renders title and calls listPartnerCategories', async () => {
    api.listPartnerCategories.mockResolvedValue(emptyList());
    renderPage(<ErpPartnerCategoriesPage />);
    expect(screen.getByText('Kategori Partner')).toBeInTheDocument();
    await waitFor(() => expect(api.listPartnerCategories).toHaveBeenCalled());
  });
});

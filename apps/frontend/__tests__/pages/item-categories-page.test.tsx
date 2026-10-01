import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listItemCategories: vi.fn(),
  createItemCategory: vi.fn(),
  updateItemCategory: vi.fn(),
  deleteItemCategory: vi.fn(),
}));
vi.mock('@/lib/api/item-categories', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpItemCategoriesPage } from '@/components/pages/item-categories-page';

describe('ErpItemCategoriesPage (smoke)', () => {
  it('renders title and calls listItemCategories', async () => {
    api.listItemCategories.mockResolvedValue(emptyList());
    renderPage(<ErpItemCategoriesPage />);
    expect(screen.getByText('Kategori Item')).toBeInTheDocument();
    await waitFor(() => expect(api.listItemCategories).toHaveBeenCalled());
  });
});

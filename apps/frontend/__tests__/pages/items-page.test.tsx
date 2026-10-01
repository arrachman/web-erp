import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const itemsApi = vi.hoisted(() => ({
  listItems: vi.fn(),
  createItem: vi.fn(),
  updateItem: vi.fn(),
  deleteItem: vi.fn(),
  bulkUpdateItemStatus: vi.fn(),
  bulkDeleteItems: vi.fn(),
}));
const unitsApi = vi.hoisted(() => ({ listUnits: vi.fn() }));
const catsApi = vi.hoisted(() => ({ listItemCategories: vi.fn() }));
vi.mock('@/lib/api/items', () => itemsApi);
vi.mock('@/lib/api/units', () => unitsApi);
vi.mock('@/lib/api/item-categories', () => catsApi);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpItemsPage } from '@/components/pages/items-page';

describe('ErpItemsPage (smoke)', () => {
  it('renders title and calls listItems', async () => {
    itemsApi.listItems.mockResolvedValue(emptyList());
    unitsApi.listUnits.mockResolvedValue(emptyList());
    catsApi.listItemCategories.mockResolvedValue(emptyList());
    renderPage(<ErpItemsPage />);
    expect(screen.getByText('Item')).toBeInTheDocument();
    await waitFor(() => expect(itemsApi.listItems).toHaveBeenCalled());
  });
});

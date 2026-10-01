import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const warehousesApi = vi.hoisted(() => ({
  listWarehouses: vi.fn(),
  createWarehouse: vi.fn(),
  updateWarehouse: vi.fn(),
  deleteWarehouse: vi.fn(),
}));
const locationsApi = vi.hoisted(() => ({
  listLocations: vi.fn(),
}));
vi.mock('@/lib/api/warehouses', () => warehousesApi);
vi.mock('@/lib/api/locations', () => locationsApi);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpWarehousesPage } from '@/components/pages/warehouses-page';

describe('ErpWarehousesPage (smoke)', () => {
  it('renders title and calls listWarehouses', async () => {
    warehousesApi.listWarehouses.mockResolvedValue(emptyList());
    locationsApi.listLocations.mockResolvedValue(emptyList());
    renderPage(<ErpWarehousesPage />);
    expect(screen.getByText('Gudang')).toBeInTheDocument();
    await waitFor(() => expect(warehousesApi.listWarehouses).toHaveBeenCalled());
  });
});

import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const locationsApi = vi.hoisted(() => ({
  listLocations: vi.fn(),
  createLocation: vi.fn(),
  updateLocation: vi.fn(),
  deleteLocation: vi.fn(),
}));
const branchesApi = vi.hoisted(() => ({
  listBranches: vi.fn(),
}));
vi.mock('@/lib/api/locations', () => locationsApi);
vi.mock('@/lib/api/branches', () => branchesApi);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpLocationsPage } from '@/components/pages/locations-page';

describe('ErpLocationsPage (smoke)', () => {
  it('renders title and calls listLocations', async () => {
    locationsApi.listLocations.mockResolvedValue(emptyList());
    branchesApi.listBranches.mockResolvedValue(emptyList());
    renderPage(<ErpLocationsPage />);
    expect(screen.getByText('Lokasi')).toBeInTheDocument();
    await waitFor(() => expect(locationsApi.listLocations).toHaveBeenCalled());
  });
});

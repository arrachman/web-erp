// Smoke test for ErpUnitsPage — mocks the units API client and asserts the
// page renders its title and triggers the initial list call.

import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listUnits: vi.fn(),
  createUnit: vi.fn(),
  updateUnit: vi.fn(),
  deleteUnit: vi.fn(),
}));

vi.mock('@/lib/api/units', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpUnitsPage } from '@/components/pages/units-page';

import { fireEvent } from '@testing-library/react';

describe('ErpUnitsPage (smoke)', () => {
  it('renders title and calls listUnits', async () => {
    api.listUnits.mockResolvedValue(emptyList());
    renderPage(<ErpUnitsPage />);
    expect(screen.getByText('Satuan')).toBeInTheDocument();
    await waitFor(() => expect(api.listUnits).toHaveBeenCalled());
  });
});

describe('ErpUnitsPage (interaction: create)', () => {
  it('opens Add modal, fills code+name, saves → calls createUnit', async () => {
    api.listUnits.mockResolvedValue(emptyList());
    api.createUnit.mockResolvedValue({
      id: '1', code: 'KG', name: 'Kilogram', isActive: true,
      createdAt: '', updatedAt: '',
    });
    renderPage(<ErpUnitsPage />);
    await waitFor(() => expect(api.listUnits).toHaveBeenCalled());

    // Open Add modal
    fireEvent.click(screen.getByText('Tambah'));
    // Fill code + name (form uses ids uf2-code / uf2-name)
    fireEvent.change(document.getElementById('uf2-code') as HTMLInputElement, { target: { value: 'KG' } });
    fireEvent.change(document.getElementById('uf2-name') as HTMLInputElement, { target: { value: 'Kilogram' } });
    // Save
    fireEvent.click(screen.getByText('Simpan'));

    await waitFor(() =>
      expect(api.createUnit).toHaveBeenCalledWith({
        code: 'KG', name: 'Kilogram', isActive: true,
      }),
    );
  });
});

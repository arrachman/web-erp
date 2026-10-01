import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listPartners: vi.fn(),
  createPartner: vi.fn(),
  updatePartner: vi.fn(),
  deletePartner: vi.fn(),
}));
vi.mock('@/lib/api/partners', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpPartnersPage } from '@/components/pages/partners-page';

describe('ErpPartnersPage (smoke)', () => {
  it('renders title and calls listPartners', async () => {
    api.listPartners.mockResolvedValue(emptyList());
    renderPage(<ErpPartnersPage />);
    expect(screen.getByText('Partner')).toBeInTheDocument();
    await waitFor(() => expect(api.listPartners).toHaveBeenCalled());
  });
});

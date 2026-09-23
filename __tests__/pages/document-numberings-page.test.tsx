import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listDocumentNumberings: vi.fn(),
  createDocumentNumbering: vi.fn(),
  updateDocumentNumbering: vi.fn(),
  deleteDocumentNumbering: vi.fn(),
}));
vi.mock('@/lib/api/document-numberings', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpDocumentNumberingsPage } from '@/components/pages/document-numberings-page';

describe('ErpDocumentNumberingsPage (smoke)', () => {
  it('renders title and calls listDocumentNumberings', async () => {
    api.listDocumentNumberings.mockResolvedValue(emptyList());
    renderPage(<ErpDocumentNumberingsPage />);
    expect(screen.getByText('Penomoran Dokumen')).toBeInTheDocument();
    await waitFor(() => expect(api.listDocumentNumberings).toHaveBeenCalled());
  });
});

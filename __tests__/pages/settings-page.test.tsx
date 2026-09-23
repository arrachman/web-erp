import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  getSettings: vi.fn(),
  updateSetting: vi.fn(),
}));
vi.mock('@/lib/api/settings', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpSettingsPage } from '@/components/pages/settings-page';

describe('ErpSettingsPage (smoke)', () => {
  it('renders title and calls getSettings', async () => {
    api.getSettings.mockResolvedValue(emptyList());
    renderPage(<ErpSettingsPage />);
    expect(screen.getByText('System Settings')).toBeInTheDocument();
    await waitFor(() => expect(api.getSettings).toHaveBeenCalled());
  });
});

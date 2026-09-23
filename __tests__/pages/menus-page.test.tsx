import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listSysMenus: vi.fn(),
  getSysMenuTree: vi.fn(),
  createSysMenu: vi.fn(),
  updateSysMenu: vi.fn(),
  deleteSysMenu: vi.fn(),
  reorderSysMenus: vi.fn(),
  bulkUpdateSysMenuStatus: vi.fn(),
  bulkDeleteSysMenus: vi.fn(),
  ERP_MENU_TYPES: ['MODULE', 'GROUP', 'ITEM'],
}));
vi.mock('@/lib/api/sys-menus', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpMenusPage } from '@/components/pages/menus-page';

import { fireEvent } from '@testing-library/react';

describe('ErpMenusPage (smoke)', () => {
  it('renders title and calls listSysMenus', async () => {
    api.listSysMenus.mockResolvedValue(emptyList());
    renderPage(<ErpMenusPage />);
    expect(screen.getByText('Manajer Menu')).toBeInTheDocument();
    await waitFor(() => expect(api.listSysMenus).toHaveBeenCalled());
  });
});

describe('ErpMenusPage (interaction: create)', () => {
  it('opens Add modal, fills code+title (type=MODULE default), saves → calls createSysMenu', async () => {
    api.listSysMenus.mockResolvedValue(emptyList());
    api.createSysMenu.mockResolvedValue({
      id: '1', code: 'NEW_MODULE', title: 'New Module', type: 'MODULE',
      parentId: null, path: null, icon: null, sortOrder: 0, isActive: true,
      createdAt: '', updatedAt: '',
    });
    renderPage(<ErpMenusPage />);
    await waitFor(() => expect(api.listSysMenus).toHaveBeenCalled());

    fireEvent.click(screen.getByText('Tambah'));
    fireEvent.change(document.getElementById('mf-code') as HTMLInputElement, { target: { value: 'NEW_MODULE' } });
    fireEvent.change(document.getElementById('mf-title') as HTMLInputElement, { target: { value: 'New Module' } });
    fireEvent.click(screen.getByText('Simpan'));

    await waitFor(() => expect(api.createSysMenu).toHaveBeenCalled());
    const payload = api.createSysMenu.mock.calls[0][0];
    expect(payload.code).toBe('NEW_MODULE');
    expect(payload.title).toBe('New Module');
    expect(payload.type).toBe('MODULE');
  });
});

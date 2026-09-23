import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { emptyList, renderPage } from '../helpers/render-page';
import { createFeedbackMock } from '../helpers/feedback-mock';

const api = vi.hoisted(() => ({
  listJournalEntries: vi.fn(),
  createJournalEntry: vi.fn(),
  updateJournalEntry: vi.fn(),
  deleteJournalEntry: vi.fn(),
}));
vi.mock('@/lib/api/fin-journal-entries', () => api);
vi.mock('@/lib/feedback', () => createFeedbackMock());

import { ErpJournalEntriesPage } from '@/components/pages/fin-journal-entries-page';

import { fireEvent } from '@testing-library/react';

describe('ErpJournalEntriesPage (smoke)', () => {
  it('renders title and calls listJournalEntries', async () => {
    api.listJournalEntries.mockResolvedValue(emptyList());
    renderPage(<ErpJournalEntriesPage />);
    expect(screen.getByText('Journal Entries')).toBeInTheDocument();
    await waitFor(() => expect(api.listJournalEntries).toHaveBeenCalled());
  });
});

describe('ErpJournalEntriesPage (interaction: create with 2 lines)', () => {
  it('opens Add, fills header + adds 2 lines, saves → calls createJournalEntry with 2 lines', async () => {
    api.listJournalEntries.mockResolvedValue(emptyList());
    api.createJournalEntry.mockResolvedValue({ id: '1' });
    renderPage(<ErpJournalEntriesPage />);
    await waitFor(() => expect(api.listJournalEntries).toHaveBeenCalled());

    fireEvent.click(screen.getByText('Tambah'));
    fireEvent.change(document.getElementById('je-doc') as HTMLInputElement, { target: { value: 'JV-TEST-001' } });
    fireEvent.change(document.getElementById('je-desc') as HTMLInputElement, { target: { value: 'Test entry' } });

    // Add two lines via "+ Tambah Baris"
    const addLineBtn = screen.getByText('+ Tambah Baris');
    fireEvent.click(addLineBtn);
    fireEvent.click(addLineBtn);

    fireEvent.click(screen.getByText('Simpan'));

    await waitFor(() => expect(api.createJournalEntry).toHaveBeenCalled());
    const payload = api.createJournalEntry.mock.calls[0][0];
    expect(payload.docNumber).toBe('JV-TEST-001');
    expect(payload.description).toBe('Test entry');
    expect(payload.lines).toHaveLength(2);
    expect(payload.lines[0].lineNo).toBe(1);
    expect(payload.lines[1].lineNo).toBe(2);
  });
});

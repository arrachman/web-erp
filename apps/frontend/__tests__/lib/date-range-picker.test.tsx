import * as React from 'react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { DateRangePicker } from '@/components/ui/date-range-picker';

vi.mock('@/lib/api/date-format', () => ({
  getDateFormat: vi.fn().mockResolvedValue({ format: 'DD/MM/YYYY', example: '31/01/2026' }),
}));

/**
 * Controlled harness — mirrors currencies-rates form state so selection
 * round-trips through props (same path as production).
 */
function Harness() {
  const [from, setFrom] = React.useState('');
  const [to, setTo] = React.useState('');
  return (
    <div>
      <DateRangePicker
        from={from}
        to={to}
        onChangeFrom={setFrom}
        onChangeTo={setTo}
      />
      <div data-testid="from">{from}</div>
      <div data-testid="to">{to}</div>
    </div>
  );
}

function openCalendar() {
  fireEvent.click(screen.getByTitle('Buka kalender'));
}

function dayButtons(): HTMLElement[] {
  // rdp v9: class "rdp-day_button" (UI.DayButton = "day_button")
  return Array.from(
    document.querySelectorAll('button.rdp-day_button'),
  ) as HTMLElement[];
}

function enabledDays(): HTMLElement[] {
  return dayButtons().filter((b) => !b.hasAttribute('disabled') && !b.getAttribute('aria-disabled'));
}

describe('DateRangePicker range UX', () => {
  beforeEach(() => {
    document.body.innerHTML = '';
  });

  it('keeps popover open after picking start and end; commits only on Terapkan', async () => {
    render(<Harness />);
    openCalendar();

    await waitFor(() => {
      expect(enabledDays().length).toBeGreaterThan(10);
    });

    const days = enabledDays();
    const start = days[5];
    const end = days[10];
    expect(start).toBeTruthy();
    expect(end).toBeTruthy();
    expect(start).not.toBe(end);

    fireEvent.click(start);
    fireEvent.click(end);

    // Draft only — committed props still empty until Terapkan
    expect(screen.getByTestId('from').textContent).toBe('');
    expect(screen.getByTestId('to').textContent).toBe('');
    // Calendar stays open
    expect(enabledDays().length).toBeGreaterThan(0);

    fireEvent.click(screen.getByRole('button', { name: 'Terapkan' }));

    await waitFor(() => {
      expect(screen.getByTestId('from').textContent).toMatch(/^\d{4}-\d{2}-\d{2}$/);
      expect(screen.getByTestId('to').textContent).toMatch(/^\d{4}-\d{2}-\d{2}$/);
    });
    // Popover closes after apply
    await waitFor(() => {
      expect(dayButtons().length).toBe(0);
    });
  });

  it('allows same-day range: second click on start, then Terapkan', async () => {
    render(<Harness />);
    openCalendar();
    await waitFor(() => expect(enabledDays().length).toBeGreaterThan(10));

    const day = enabledDays()[5];
    fireEvent.click(day);
    fireEvent.click(day);

    // Still draft
    expect(screen.getByTestId('from').textContent).toBe('');
    fireEvent.click(screen.getByRole('button', { name: 'Terapkan' }));

    await waitFor(() => {
      const from = screen.getByTestId('from').textContent;
      const to = screen.getByTestId('to').textContent;
      expect(from).toMatch(/^\d{4}-\d{2}-\d{2}$/);
      expect(to).toBe(from);
    });
  });

  it('does not commit draft until Terapkan is clicked', async () => {
    render(<Harness />);
    openCalendar();
    await waitFor(() => expect(enabledDays().length).toBeGreaterThan(10));

    fireEvent.click(enabledDays()[5]);
    fireEvent.click(enabledDays()[10]);

    // After both day clicks, committed props remain empty (draft only)
    expect(screen.getByTestId('from').textContent).toBe('');
    expect(screen.getByTestId('to').textContent).toBe('');
    // Popover still open with Terapkan available
    expect(screen.getByRole('button', { name: 'Terapkan' })).toBeInTheDocument();
    expect(enabledDays().length).toBeGreaterThan(0);
  });
});

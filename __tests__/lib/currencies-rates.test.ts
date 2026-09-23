import { describe, expect, it } from 'vitest';
import { expandRateDates } from '@/components/pages/currencies-rates';

describe('expandRateDates', () => {
  it('returns a single day when to is empty', () => {
    expect(expandRateDates('2026-07-08', '')).toEqual(['2026-07-08']);
  });

  it('expands inclusive day range (8 Jul → 17 Aug 2026 = 41 days)', () => {
    const days = expandRateDates('2026-07-08', '2026-08-17');
    expect(days).not.toBeNull();
    expect(days![0]).toBe('2026-07-08');
    expect(days![days!.length - 1]).toBe('2026-08-17');
    expect(days).toHaveLength(41);
    // consecutive
    expect(days![1]).toBe('2026-07-09');
  });

  it('returns null when end is before start', () => {
    expect(expandRateDates('2026-08-17', '2026-07-08')).toBeNull();
  });

  it('returns null for invalid from', () => {
    expect(expandRateDates('', '2026-08-17')).toBeNull();
    expect(expandRateDates('not-a-date', '2026-08-17')).toBeNull();
  });

  it('returns null when span exceeds 366 days', () => {
    expect(expandRateDates('2025-01-01', '2026-12-31')).toBeNull();
  });
});

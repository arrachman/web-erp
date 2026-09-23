import { describe, expect, it } from 'vitest';
import {
  firstChildAccountCode,
  incrementAccountCode,
  suggestNextAccountCode,
} from '@/lib/accounts-code-generator';
import type { AccountCodeFormat } from '@/lib/api/accounts';

const format423: AccountCodeFormat = {
  segments: [4, 2, 3],
  separator: '.',
  patternSource: '^\\d{4}\\.\\d{2}\\.\\d{3}$',
  maxLength: 11,
  example: '1111.00.000',
  accountCount: 0,
  locked: false,
};

describe('accounts-code-generator', () => {
  it('suggests first root code when no siblings', () => {
    expect(firstChildAccountCode(null, format423)).toBe('1000.00.000');
    expect(suggestNextAccountCode(null, [], format423)).toBe('1000.00.000');
  });

  it('suggests first child under HEADER parent (next free segment = 1)', () => {
    expect(firstChildAccountCode('1100.00.000', format423)).toBe('1100.01.000');
    expect(firstChildAccountCode('1000.00.000', format423)).toBe('1000.01.000');
  });

  it('increments sibling sequence under parent', () => {
    expect(
      suggestNextAccountCode('1000.00.000', ['1100.00.000', '1200.00.000', '1300.00.000'], format423),
    ).toBe('1400.00.000');

    expect(
      suggestNextAccountCode(
        '1100.00.000',
        ['1101.01.001', '1102.01.001', '1110.01.001'],
        format423,
      ),
    ).toBe('1111.01.001');
  });

  it('starts at 1 when parent has no children', () => {
    expect(suggestNextAccountCode('2100.00.000', [], format423)).toBe('2100.01.000');
  });

  it('increments last segment with pad when needed', () => {
    expect(incrementAccountCode('1101.01.001', format423)).toBe('1101.01.002');
    expect(incrementAccountCode('1101.01.999', format423)).toBe('1101.02.000');
  });

  it('supports compact formats without separator', () => {
    const compact: AccountCodeFormat = {
      ...format423,
      segments: [1, 2, 3],
      separator: '',
      patternSource: '^\\d{1}\\d{2}\\d{3}$',
      maxLength: 6,
      example: '100000',
    };
    expect(firstChildAccountCode(null, compact)).toBe('100000');
    expect(suggestNextAccountCode(null, ['100000', '200000'], compact)).toBe('300000');
    expect(firstChildAccountCode('100000', compact)).toBe('101000');
  });
});

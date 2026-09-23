import { describe, expect, it, vi } from 'vitest';

vi.mock('@/lib/api/accounts', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api/accounts')>('@/lib/api/accounts');
  return {
    ...actual,
    getAccountCodeFormat: vi.fn(),
    listAccounts: vi.fn(),
  };
});

vi.mock('@/lib/api/currencies', () => ({
  listCurrencies: vi.fn(),
}));

vi.mock('@/lib/api/banks', () => ({
  listBanks: vi.fn(),
}));

import {
  defaultAccountForm,
  fromAccount,
  toAccountPayload,
  validateAccount,
} from '@/components/pages/accounts-form';
import type { ErpAccount } from '@/lib/api/accounts';

describe('accounts form mapping', () => {
  it('includes posting details for postable accounts', () => {
    const form = {
      ...defaultAccountForm(),
      code: '1110.01.001',
      name: 'Bank BCA - Giro IDR',
      accountKind: 'POSTABLE' as const,
      currencyId: '1',
      bankId: '5',
      bankAccountNo: '123-456-7890',
      branchIds: ['10'],
      locationIds: ['20'],
      divisionIds: ['30'],
    };

    expect(toAccountPayload(form)).toMatchObject({
      currencyId: '1',
      bankId: '5',
      bankAccountNo: '123-456-7890',
      branchIds: ['10'],
      locationIds: ['20'],
      divisionIds: ['30'],
    });
    expect(toAccountPayload(form)).not.toHaveProperty('isControlAccount');
  });

  it('clears posting details from header payloads', () => {
    const form = {
      ...defaultAccountForm(),
      code: '1100.00.000',
      name: 'Aset Lancar',
      accountKind: 'HEADER' as const,
      currencyId: '1',
      bankId: '5',
      bankAccountNo: '123-456-7890',
      branchIds: ['10'],
    };

    expect(toAccountPayload(form)).toMatchObject({
      currencyId: null,
      bankId: null,
      bankAccountNo: undefined,
      branchIds: [],
      locationIds: [],
      divisionIds: [],
    });
  });

  it('hydrates parent, currency, bank and multi-dim labels from an account record', () => {
    const account: ErpAccount = {
      id: '10',
      code: '1110.01.001',
      name: 'Bank BCA - Giro IDR',
      type: 'ASSET',
      kind: 'POSTABLE',
      normalBalance: 'DEBIT',
      parentId: '2',
      parent: { id: '2', code: '1100.00.000', name: 'Aset Lancar' },
      currencyId: '1',
      currency: { id: '1', code: 'IDR', name: 'Rupiah', symbol: 'Rp' },
      bankId: '5',
      bank: { id: '5', code: 'BCA', name: 'Bank BCA' },
      bankAccountNo: '123-456-7890',
      dimBranches: [
        { branchId: '10', branch: { id: '10', code: 'HO', name: 'Head Office' } },
      ],
      dimLocations: [
        { locationId: '20', location: { id: '20', code: 'LOC1', name: 'Gudang 1' } },
      ],
      dimDivisions: [
        { divisionId: '30', division: { id: '30', code: 'DIV1', name: 'Ops' } },
      ],
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    };

    expect(fromAccount(account)).toMatchObject({
      parentLabel: '1100.00.000 — Aset Lancar',
      currencyLabel: 'IDR — Rupiah',
      bankId: '5',
      bankLabel: 'BCA — Bank BCA',
      bankAccountNo: '123-456-7890',
      branchIds: ['10'],
      locationIds: ['20'],
      divisionIds: ['30'],
    });
  });

  it('validates paired bank fields', () => {
    const errors = validateAccount({
      ...defaultAccountForm(),
      code: '1110.01.001',
      name: 'Bank BCA',
      bankAccountNo: '123-456-7890',
    });

    expect(errors.bankId).toBe('Bank wajib diisi bila No. Rekening diisi');
  });
});

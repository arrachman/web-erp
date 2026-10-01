'use client';

/**
 * Bank Keluar (SM) filter bar — thin wrapper over the shared cash/bank filter
 * bar (§2.40) with bank labels ("Bayar Ke", drawer title "Bank Keluar").
 */

import * as React from 'react';
import { CashBankFiltersBar } from './cash-bank-filters';
import {
  emptyCashBankFilters,
  type CashBankFilters,
} from './cash-bank-filter-fields';

export type BdFilters = CashBankFilters;
export const emptyBdFilters = emptyCashBankFilters;

export function BankDisbursementFilters({
  value,
  onChange,
}: {
  value: BdFilters;
  onChange: (f: BdFilters) => void;
}) {
  return (
    <CashBankFiltersBar
      value={value}
      onChange={onChange}
      entityName="Bank Keluar"
      partnerLabel="Bayar Ke"
    />
  );
}

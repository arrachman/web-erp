'use client';

/**
 * Kas Keluar (CD) filter bar — thin wrapper over the shared cash/bank filter bar
 * (§2.40) with CD labels ("Bayar Ke", drawer title "Kas Keluar").
 */

import * as React from 'react';
import { CashBankFiltersBar } from './cash-bank-filters';
import {
  emptyCashBankFilters,
  type CashBankFilters,
} from './cash-bank-filter-fields';

export type CdFilters = CashBankFilters;
export const emptyCdFilters = emptyCashBankFilters;

export function CashDisbursementFilters({
  value,
  onChange,
}: {
  value: CdFilters;
  onChange: (f: CdFilters) => void;
}) {
  return (
    <CashBankFiltersBar
      value={value}
      onChange={onChange}
      entityName="Kas Keluar"
      partnerLabel="Bayar Ke"
    />
  );
}

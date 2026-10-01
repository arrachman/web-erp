'use client';

/**
 * Kas Masuk (CR) filter bar — thin wrapper over the shared cash/bank filter bar
 * (§2.40) with CR labels ("Terima Dari", drawer title "Kas Masuk").
 */

import * as React from 'react';
import { CashBankFiltersBar } from './cash-bank-filters';
import {
  emptyCashBankFilters,
  type CashBankFilters,
} from './cash-bank-filter-fields';

export type CrFilters = CashBankFilters;
export const emptyCrFilters = emptyCashBankFilters;

export function CashReceiptFilters({
  value,
  onChange,
}: {
  value: CrFilters;
  onChange: (f: CrFilters) => void;
}) {
  return (
    <CashBankFiltersBar
      value={value}
      onChange={onChange}
      entityName="Kas Masuk"
      partnerLabel="Terima Dari"
    />
  );
}

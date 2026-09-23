// Header SearchSelect loaders specific to Purchasing transaction forms.
// Supplier = partners with partner type kind SUPPLIER; payment term = md_payment_terms.
// Branch/Location/Warehouse/Currency reuse items-form-lookups loaders.

import { listPaymentTerms } from '@/lib/api/payment-terms';
export { loadSupplierOptions } from './items-form-lookups';

/** Payment-term picker (Termin). */
export const loadPaymentTermOptions = async (search: string, page: number, limit: number) => {
  const res = await listPaymentTerms({ search: search || undefined, page, limit, isActive: true });
  return {
    data: res.data.map((x) => ({ value: x.id, label: x.name, code: x.code })),
    total: res.meta.total,
  };
};

import { listAccounts, type ErpAccountType } from '@/lib/api/accounts';
import { listCurrencies } from '@/lib/api/currencies';

const accountOptionLoader = (accountType: ErpAccountType) =>
  async (search: string, page: number, limit: number) => {
    const res = await listAccounts({
      page,
      limit,
      search: search || undefined,
      accountType,
      accountKind: 'POSTABLE',
      isActive: true,
    });
    return {
      data: res.data.map((a) => ({ value: a.id, label: a.name, code: a.code })),
      total: res.meta.total,
    };
  };

export const loadReceivableAccounts = accountOptionLoader('ASSET');
export const loadPayableAccounts = accountOptionLoader('LIABILITY');

export const loadCurrencyOptions = async (search: string, page: number, limit: number) => {
  const res = await listCurrencies({ search: search || undefined, page, limit, isActive: true });
  return {
    data: res.data.map((c) => ({ value: c.id, label: `${c.code} — ${c.name}`, code: c.code })),
    total: res.meta?.total ?? res.data.length,
  };
};

import { type ErpPartner, type CreatePartnerPayload } from '@/lib/api/partners';
import { type ErpPartnerTypeKind } from '@/lib/api/partner-types';
import { validateForm } from '@/lib/form-validation';

// ─── Partner type helpers ─────────────────────────────────────────────────────

export function partnerTypeLabel(p: ErpPartner): string {
  return p.partnerType?.name ?? '—';
}

export function partnerTypeKind(p: ErpPartner): ErpPartnerTypeKind | '' {
  return p.partnerType?.kind ?? '';
}

// ─── Form interface ────────────────────────────────────────────────────────────

export interface PartnerForm {
  id: string;
  code: string;
  name: string;
  partnerTypeId: string;
  partnerTypeLabel: string;
  partnerTypeKind: ErpPartnerTypeKind | '';
  taxNumber: string;
  customerCategoryId: string;
  customerCategoryLabel: string;
  supplierCategoryId: string;
  supplierCategoryLabel: string;
  salesmanCategoryId: string;
  salesmanCategoryLabel: string;
  salesmanId: string;
  salesmanLabel: string;
  receivableAccountId: string;
  receivableAccountLabel: string;
  payableAccountId: string;
  payableAccountLabel: string;
  branchIds: string[];
  branchLabels: Record<string, string>;
  warehouseIds: string[];
  warehouseLabels: Record<string, string>;
  locationIds: string[];
  locationLabels: Record<string, string>;
  isActive: boolean;
  // Transaksi tab
  currencyId: string;
  currencyLabel: string;
  saleTermId: string;
  saleTermLabel: string;
  purchaseTermId: string;
  purchaseTermLabel: string;
  arCreditLimit: string;
  apCreditLimit: string;
  salesPriceTier: string;
}

export const defaultForm = (): PartnerForm => ({
  id: '',
  code: '',
  name: '',
  partnerTypeId: '',
  partnerTypeLabel: '',
  partnerTypeKind: '',
  taxNumber: '',
  customerCategoryId: '',
  customerCategoryLabel: '',
  supplierCategoryId: '',
  supplierCategoryLabel: '',
  salesmanCategoryId: '',
  salesmanCategoryLabel: '',
  salesmanId: '',
  salesmanLabel: '',
  receivableAccountId: '',
  receivableAccountLabel: '',
  payableAccountId: '',
  payableAccountLabel: '',
  branchIds: [],
  branchLabels: {},
  warehouseIds: [],
  warehouseLabels: {},
  locationIds: [],
  locationLabels: {},
  isActive: true,
  currencyId: '',
  currencyLabel: '',
  saleTermId: '',
  saleTermLabel: '',
  purchaseTermId: '',
  purchaseTermLabel: '',
  arCreditLimit: '',
  apCreditLimit: '',
  salesPriceTier: '1',
});

// ─── Helpers ──────────────────────────────────────────────────────────────────

function dimFromRows<T>(
  rows: T[] | undefined,
  getId: (r: T) => string,
  getRef: (r: T) => { name: string } | null | undefined,
): { ids: string[]; labels: Record<string, string> } {
  const ids: string[] = [];
  const labels: Record<string, string> = {};
  (rows ?? []).forEach((r) => {
    const id = getId(r);
    ids.push(id);
    const ref = getRef(r);
    if (ref) labels[id] = ref.name;
  });
  return { ids, labels };
}

const catLabel = (cat?: { name: string } | null) => cat?.name ?? '';
const acctLabel = (acct?: { code: string; name: string } | null) =>
  acct ? `${acct.code} — ${acct.name}` : '';
const termLabel = (t?: { code: string; name: string } | null) =>
  t ? `${t.code} — ${t.name}` : '';

export const fromRecord = (p: ErpPartner): PartnerForm => {
  const b = dimFromRows(p.dimBranches, (r) => r.branchId, (r) => r.branch);
  const w = dimFromRows(p.dimWarehouses, (r) => r.warehouseId, (r) => r.warehouse);
  const l = dimFromRows(p.dimLocations, (r) => r.locationId, (r) => r.location);
  return {
    id: p.id,
    code: p.code,
    name: p.name,
    partnerTypeId: p.partnerTypeId ?? '',
    partnerTypeLabel: p.partnerType?.name ?? '',
    partnerTypeKind: partnerTypeKind(p),
    taxNumber: p.taxNumber ?? '',
    customerCategoryId: p.customerCategoryId ?? '',
    customerCategoryLabel: catLabel(p.customerCategory),
    supplierCategoryId: p.supplierCategoryId ?? '',
    supplierCategoryLabel: catLabel(p.supplierCategory),
    salesmanCategoryId: p.salesmanCategoryId ?? '',
    salesmanCategoryLabel: catLabel(p.salesmanCategory),
    salesmanId: p.salesmanId ?? '',
    salesmanLabel: p.salesman?.name ?? '',
    receivableAccountId: p.receivableAccountId ?? '',
    receivableAccountLabel: acctLabel(p.receivableAccount),
    payableAccountId: p.payableAccountId ?? '',
    payableAccountLabel: acctLabel(p.payableAccount),
    branchIds: b.ids,
    branchLabels: b.labels,
    warehouseIds: w.ids,
    warehouseLabels: w.labels,
    locationIds: l.ids,
    locationLabels: l.labels,
    isActive: p.isActive,
    currencyId: p.currencyId ?? '',
    currencyLabel: p.currency ? `${p.currency.code} — ${p.currency.name}` : '',
    saleTermId: p.saleTermId ?? '',
    saleTermLabel: termLabel(p.saleTerm),
    purchaseTermId: p.purchaseTermId ?? '',
    purchaseTermLabel: termLabel(p.purchaseTerm),
    arCreditLimit: p.arCreditLimit ?? '',
    apCreditLimit: p.apCreditLimit ?? '',
    salesPriceTier: String(p.salesPriceTier ?? 1),
  };
};

export const toPayload = (f: PartnerForm): CreatePartnerPayload => ({
  code: f.code,
  name: f.name,
  partnerTypeId: f.partnerTypeId,
  customerCategoryId: f.partnerTypeKind === 'CUSTOMER' ? (f.customerCategoryId || null) : null,
  supplierCategoryId: f.partnerTypeKind === 'SUPPLIER' ? (f.supplierCategoryId || null) : null,
  salesmanCategoryId: f.partnerTypeKind === 'SALESMAN' ? (f.salesmanCategoryId || null) : null,
  salesmanId: f.partnerTypeKind === 'CUSTOMER' ? (f.salesmanId || null) : null,
  taxNumber: f.taxNumber || undefined,
  receivableAccountId: f.receivableAccountId || null,
  payableAccountId: f.payableAccountId || null,
  currencyId: f.currencyId || null,
  saleTermId: f.saleTermId || null,
  purchaseTermId: f.purchaseTermId || null,
  arCreditLimit: f.arCreditLimit || null,
  apCreditLimit: f.apCreditLimit || null,
  salesPriceTier: f.salesPriceTier ? Number(f.salesPriceTier) : 1,
  branchIds: f.branchIds,
  warehouseIds: f.warehouseIds,
  locationIds: f.locationIds,
  isActive: f.isActive,
});

export const validatePartner = (form: PartnerForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'partnerTypeId', label: 'Tipe', required: true },
    {
      field: 'salesmanId',
      label: 'Salesman',
      validate: (value, f) =>
        f.partnerTypeKind === 'CUSTOMER' && !value ? 'Salesman wajib diisi' : undefined,
    },
  ]);

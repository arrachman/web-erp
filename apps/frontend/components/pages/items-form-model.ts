/**
 * Item create/edit form — types, defaults, and static option lists.
 * Adapters (fromItem/toItemPayload/validateItem) live in ./items-form.
 */

import type {
  ErpItemType,
  ItemOthersData, ItemCustomData,
} from '@/lib/api/items';

export const ITEM_TYPES: ErpItemType[] = ['INVENTORY', 'SERVICE', 'CONSUMABLE', 'ASSET', 'NON_INVENTORY'];

export type ItemStockTrackingMode = 'none' | 'batch' | 'serial';

export const ITEM_STOCK_TRACKING_OPTIONS = [
  {
    value: 'none',
    label: 'Tidak pakai',
    description: 'Stok hanya dicatat berdasarkan jumlah.',
    example: 'Cocok untuk bahan curah dan ATK.',
  },
  {
    value: 'batch',
    label: 'Batch / Lot',
    description: 'Stok dikelompokkan berdasarkan batch atau lot.',
    example: 'Cocok untuk makanan, obat, dan bahan baku.',
  },
  {
    value: 'serial',
    label: 'Serial No.',
    description: 'Setiap unit stok memiliki nomor unik.',
    example: 'Cocok untuk mesin, laptop, dan elektronik.',
  },
] satisfies ReadonlyArray<{
  value: ItemStockTrackingMode;
  label: string;
  description: string;
  example: string;
}>;

export const stockTrackingModeFromFlags = (tracksBatch: boolean, tracksSerial: boolean): ItemStockTrackingMode => {
  if (tracksSerial) return 'serial';
  if (tracksBatch) return 'batch';
  return 'none';
};

export const stockTrackingFlagsFromMode = (mode: ItemStockTrackingMode) => ({
  tracksBatch: mode === 'batch',
  tracksSerial: mode === 'serial',
});

/** One row in the "Distributor" section (legacy m1_item_supplier). `key` is a
 *  stable client id so SearchSelect display state survives row add/remove. */
export interface ItemDistributorFormRow {
  key: string;
  partnerId: string; partnerLabel?: string;
}

/** One row in the per-warehouse stock override table (Stok Min/Maks + Min Order
 *  per Gudang). `key` is a stable client id so SearchSelect state survives
 *  row add/remove (not sent to API). */
export interface ItemWarehouseStockFormRow {
  key: string;
  warehouseId: string; warehouseLabel?: string;
  minStock: string;
  maxStock: string;
  minOrderQty: string;
}

export interface ItemFormData {
  /** Record id (kosong saat create) — dipakai section Media untuk upload file. */
  id: string;
  code: string;
  name: string;
  itemType: ErpItemType;
  description: string;
  barcode: string;

  // Classification lookups (id + label for edit-mode display)
  categoryId: string; categoryLabel?: string;
  unitId: string; unitLabel?: string;
  kindId: string; kindLabel?: string;
  productClassId: string; productClassLabel?: string;

  // Atribut produk (legacy "Atribut") — id + label for edit-mode display
  brandId: string; brandLabel?: string;
  materialId: string; materialLabel?: string;
  sizeId: string; sizeLabel?: string;
  colorId: string; colorLabel?: string;
  sectionId: string; sectionLabel?: string;
  designerId: string; designerLabel?: string;
  nozzleId: string; nozzleLabel?: string;
  oemId: string; oemLabel?: string;
  vendorId: string; vendorLabel?: string;
  fieldUnitId: string; fieldUnitLabel?: string;
  fieldUnitConversionFactor: string; // read-only, dari md_units.conversionFactor

  // GL / org dimensions
  divisionId: string; divisionLabel?: string;
  subdivisionId: string; subdivisionLabel?: string;
  departmentId: string; departmentLabel?: string;
  subDepartmentId: string; subDepartmentLabel?: string;
  // Multi-select dims (Cabang / Gudang Default / Lokasi Default) + label maps for chips
  branchIds: string[]; branchLabels: Record<string, string>;
  defaultWarehouseIds: string[]; defaultWarehouseLabels: Record<string, string>;
  defaultLocationIds: string[]; defaultLocationLabels: Record<string, string>;
  projectId: string; projectLabel?: string;
  costCenterId: string; costCenterLabel?: string;

  // Costs & prices
  lastHpp: string;            // "HPP Terakhir" (net cost of latest purchase, readonly)
  averageCost: string;        // "HPP Rata-rata" (moving average, internal; not rendered in form)
  purchasePrice: string;      // "Harga Beli Terakhir" (readonly, from latest purchase)
  purchaseDiscount: string;   // "Diskon Pembelian" (percent)
  salePrices: string[];       // "Harga Jual 1..N" (index 0 = level 1, dinamis/unlimited)
  saleDiscounts: string[];    // "Diskon Jual 1..N" (index 0 = level 1, dinamis/unlimited)

  // Distributors ("Distributor" tab: supplier partner per row)
  distributors: ItemDistributorFormRow[];

  // Lain-lain ("Lain-lain" tab) + Custom ("Custom" tab) → metadata sidecar
  others: ItemOthersData;
  custom: ItemCustomData;

  // Stock (global defaults; per-warehouse overrides in warehouseStocks)
  minStock: string;
  maxStock: string;
  reorderQty: string;
  minOrderQty: string;
  warehouseStocks: ItemWarehouseStockFormRow[];
  tracksSerial: boolean;
  tracksBatch: boolean;
  tracksBin: boolean;

  // Accounts & tax
  inventoryAccountId: string; inventoryAccountLabel?: string;
  salesAccountId: string; salesAccountLabel?: string;
  cogsAccountId: string; cogsAccountLabel?: string;
  salesReturnAccountId: string; salesReturnAccountLabel?: string;
  salesDiscountAccountId: string; salesDiscountAccountLabel?: string;
  purchaseReturnAccountId: string; purchaseReturnAccountLabel?: string;
  purchaseDiscountAccountId: string; purchaseDiscountAccountLabel?: string;
  consignmentAccountId: string; consignmentAccountLabel?: string;
  purchaseTaxId: string; purchaseTaxLabel?: string;
  purchaseTax2Id: string; purchaseTax2Label?: string;
  saleTaxId: string; saleTaxLabel?: string;
  saleTax2Id: string; saleTax2Label?: string;

  // Supplier & physical
  primarySupplierId: string; primarySupplierLabel?: string;
  weight: string;

  // Atribut fisik & regulasi (legacy "Atribut")
  length: string;
  width: string;
  height: string;
  volume: string;
  conversionKgPcs: string;
  registrationNo: string;
  isReturnable: boolean;
  isMobile: boolean;

  // Validity & flags
  ageCategory: string;
  validUntil: string;
  isVatable: boolean;
  isSpecial: boolean;
  isActive: boolean;
}

export const defaultItemForm = (): ItemFormData => ({
  id: '',
  code: '', name: '', itemType: 'INVENTORY', description: '', barcode: '',
  categoryId: '', unitId: '', kindId: '', productClassId: '',
  brandId: '', materialId: '', sizeId: '', colorId: '', sectionId: '',
  designerId: '', nozzleId: '', oemId: '', vendorId: '', fieldUnitId: '', fieldUnitConversionFactor: '1',
  divisionId: '', subdivisionId: '', departmentId: '', subDepartmentId: '',
  branchIds: [], branchLabels: {},
  defaultWarehouseIds: [], defaultWarehouseLabels: {},
  defaultLocationIds: [], defaultLocationLabels: {},
  projectId: '', costCenterId: '',
  lastHpp: '', averageCost: '', purchasePrice: '', purchaseDiscount: '',
  salePrices: [''], saleDiscounts: [''],
  distributors: [],
  others: {}, custom: {},
  minStock: '', maxStock: '', reorderQty: '', minOrderQty: '',
  warehouseStocks: [],
  tracksSerial: false, tracksBatch: false, tracksBin: false,
  inventoryAccountId: '', salesAccountId: '', cogsAccountId: '',
  salesReturnAccountId: '', salesDiscountAccountId: '',
  purchaseReturnAccountId: '', purchaseDiscountAccountId: '', consignmentAccountId: '',
  purchaseTaxId: '', purchaseTax2Id: '', saleTaxId: '', saleTax2Id: '',
  primarySupplierId: '', weight: '',
  length: '', width: '', height: '', volume: '', conversionKgPcs: '1', registrationNo: '',
  isReturnable: true, isMobile: false,
  ageCategory: '', validUntil: '', isVatable: true, isSpecial: false, isActive: true,
});

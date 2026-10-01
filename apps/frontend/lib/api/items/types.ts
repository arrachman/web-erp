// ERP Item resource — types for md_items

export type ErpItemType =
  | 'INVENTORY'
  | 'SERVICE'
  | 'CONSUMABLE'
  | 'ASSET'
  | 'NON_INVENTORY';

export type ErpCostingMethod = 'AVG' | 'FIFO' | 'STD';

/** Internal relation reference — NOT re-exported from the public barrel. */
export interface RelationRef {
  id: string;
  code?: string;
  name?: string;
}

/** One sale price tier (legacy "Harga Jual N" + "Diskon Jual N"). */
export interface ItemPriceTier {
  level: number;
  price: string;
  discountPercent: string;
}

/** One item distributor (legacy "Distributor" tab: a supplier/distributor partner). */
export interface ItemDistributorRow {
  partnerId: string;
  partner?: RelationRef | null;
}

/** One per-warehouse stock level override (Stok Min/Maks + Min Order per Gudang).
 *  Global values on the item stay the default; a row here overrides one warehouse. */
export interface ItemWarehouseStockRow {
  warehouseId: string;
  minStock: string;
  maxStock: string;
  minOrderQty: string;
  warehouse?: RelationRef | null;
}

/** Legacy "Lain-lain" tab — alias names + free-text notes (in metadata.others). */
export interface ItemOthersData {
  aliasName1?: string; aliasName2?: string; aliasName3?: string; aliasName4?: string;
  notesRc?: string;
  catatan?: string;
}

/** Legacy "Custom" tab — production/moulding attributes (in metadata.custom). */
export interface ItemCustomData {
  productionCategory?: string; productionGroup?: string;
  maxQtySo?: string; capacityPerHour?: string; maxQtyRc?: string; allowance?: string;
  wip1?: string; wip2?: string; wip3?: string;
  mouldFinish?: string; moldSemi1?: string; moldSemi2?: string;
  min1?: string; max1?: string; min2?: string; max2?: string;
}

/** Free-form JSON sidecar; ERP item form stores Lain-lain/Custom tabs here. */
export interface ItemMetadata {
  others?: ItemOthersData;
  custom?: ItemCustomData;
  [key: string]: unknown;
}

export interface ErpItem {
  id: string;
  code: string;
  name: string;
  itemType: ErpItemType;
  costMethod: ErpCostingMethod;
  categoryId: string;
  unitId: string;
  description?: string | null;
  barcode?: string | null;

  // Classification
  kindId?: string | null;
  productClassId?: string | null;
  brandId?: string | null;
  materialId?: string | null;
  itemModelId?: string | null;
  sizeId?: string | null;
  colorId?: string | null;
  sectionId?: string | null;
  designerId?: string | null;
  nozzleId?: string | null;
  oemId?: string | null;
  vendorId?: string | null;
  fieldUnitId?: string | null;

  // GL / org dimensions
  divisionId?: string | null;
  subdivisionId?: string | null;
  departmentId?: string | null;
  subDepartmentId?: string | null;
  branchId?: string | null;
  defaultLocationId?: string | null;
  defaultWarehouseId?: string | null;
  projectId?: string | null;
  costCenterId?: string | null;

  // Multi-select GL dimensions (md_item_dim_*; single ids above mirror the first selection)
  dimBranches?: { branchId: string; branch?: RelationRef | null }[];
  dimWarehouses?: { warehouseId: string; warehouse?: RelationRef | null }[];
  dimLocations?: { locationId: string; location?: RelationRef | null }[];

  // Costs & prices (all system-managed/readonly except purchaseDiscount + sale tiers)
  standardCost?: string | null;      // legacy "Hpp Update" (manual HPP) — no longer surfaced
  averageCost?: string | null;       // "HPP Rata-rata" (moving average, readonly)
  lastHpp?: string | null;           // "HPP Terakhir" (net cost of latest purchase, readonly)
  purchasePrice?: string | null;     // "Harga Beli Terakhir" (from latest purchase, readonly)
  purchaseDiscount?: string | null;  // "Diskon Pembelian" (percent)
  salePrice?: string | null;         // "Harga Jual 1" (mirror of tier level 1)
  sellingPrice?: string | null; // legacy alias of salePrice
  prices?: ItemPriceTier[];          // "Harga Jual 1..10" + "Diskon Jual 1..10"
  distributors?: ItemDistributorRow[]; // "Distributor" tab: supplier partners
  metadata?: ItemMetadata | null;    // "Lain-lain" + "Custom" tabs (JSON sidecar)

  // Stock & tracking
  minStock?: string | null;
  maxStock?: string | null;
  reorderQty?: string | null;
  minOrderQty?: string | null;
  warehouseStocks?: ItemWarehouseStockRow[]; // per-warehouse overrides (kosong = pakai global)
  tracksSerial?: boolean;
  tracksBatch?: boolean;
  tracksBin?: boolean;

  // GL accounts
  inventoryAccountId?: string | null;
  salesAccountId?: string | null;
  cogsAccountId?: string | null;
  salesReturnAccountId?: string | null;
  salesDiscountAccountId?: string | null;
  purchaseReturnAccountId?: string | null;
  purchaseDiscountAccountId?: string | null;
  consignmentAccountId?: string | null;

  // Tax
  purchaseTaxId?: string | null;
  purchaseTax2Id?: string | null;
  saleTaxId?: string | null;
  saleTax2Id?: string | null;

  // Supplier & physical
  primarySupplierId?: string | null;
  weight?: string | null;

  // Atribut fisik & regulasi (legacy "Atribut")
  length?: string | null;
  width?: string | null;
  height?: string | null;
  volume?: string | null;
  conversionKgPcs?: string | null;
  registrationNo?: string | null;
  isReturnable?: boolean;
  isMobile?: boolean;

  // Validity & flags
  ageCategory?: string | null;
  validUntil?: string | null;
  isVatable: boolean;
  isSpecial: boolean;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;

  // Includes
  category?: RelationRef | null;
  unit?: RelationRef | null;
  kind?: RelationRef | null;
  productClass?: RelationRef | null;
  division?: RelationRef | null;
  subdivision?: RelationRef | null;
  department?: RelationRef | null;
  subDepartment?: RelationRef | null;
  branch?: RelationRef | null;
  defaultLocation?: RelationRef | null;
  defaultWarehouse?: RelationRef | null;
  project?: RelationRef | null;
  costCenter?: RelationRef | null;
  inventoryAccount?: RelationRef | null;
  salesAccount?: RelationRef | null;
  cogsAccount?: RelationRef | null;
  salesReturnAccount?: RelationRef | null;
  salesDiscountAccount?: RelationRef | null;
  purchaseReturnAccount?: RelationRef | null;
  purchaseDiscountAccount?: RelationRef | null;
  consignmentAccount?: RelationRef | null;
  purchaseTax?: RelationRef | null;
  purchaseTax2?: RelationRef | null;
  saleTax?: RelationRef | null;
  saleTax2?: RelationRef | null;
  primarySupplier?: RelationRef | null;
  brand?: RelationRef | null;
  material?: RelationRef | null;
  size?: RelationRef | null;
  color?: RelationRef | null;
  section?: RelationRef | null;
  designer?: RelationRef | null;
  nozzle?: RelationRef | null;
  oem?: RelationRef | null;
  vendor?: RelationRef | null;
  fieldUnit?: (RelationRef & { conversionFactor?: string }) | null;
}
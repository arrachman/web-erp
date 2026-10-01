// ERP Item resource — create/update payloads

import type { ErpCostingMethod, ErpItemType, ItemCustomData, ItemOthersData } from './types';

export interface CreateItemPayload {
  code: string;
  name: string;
  itemType: ErpItemType;
  costMethod?: ErpCostingMethod;
  categoryId: string;
  unitId: string;
  description?: string;
  barcode?: string;

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

  divisionId?: string | null;
  subdivisionId?: string | null;
  departmentId?: string | null;
  subDepartmentId?: string | null;
  branchId?: string | null;
  defaultLocationId?: string | null;
  defaultWarehouseId?: string | null;
  projectId?: string | null;
  costCenterId?: string | null;

  // Multi-select GL dimensions; backend mirrors single ids to the first entry
  branchIds?: string[];
  defaultWarehouseIds?: string[];
  defaultLocationIds?: string[];

  // standardCost ("HPP Update") removed; lastHpp/averageCost are system-managed (read-only).
  purchasePrice?: string;
  purchaseDiscount?: string;
  salePrice?: string;
  prices?: { level: number; price?: string; discountPercent?: string }[];
  distributors?: { partnerId: string }[];
  others?: ItemOthersData;   // "Lain-lain" tab → metadata.others
  custom?: ItemCustomData;   // "Custom" tab → metadata.custom

  minStock?: string;
  maxStock?: string;
  reorderQty?: string;
  minOrderQty?: string;
  warehouseStocks?: { warehouseId: string; minStock?: string; maxStock?: string; minOrderQty?: string }[];

  tracksSerial?: boolean;
  tracksBatch?: boolean;
  tracksBin?: boolean;

  inventoryAccountId?: string | null;
  salesAccountId?: string | null;
  cogsAccountId?: string | null;
  salesReturnAccountId?: string | null;
  salesDiscountAccountId?: string | null;
  purchaseReturnAccountId?: string | null;
  purchaseDiscountAccountId?: string | null;
  consignmentAccountId?: string | null;
  purchaseTaxId?: string | null;
  purchaseTax2Id?: string | null;
  saleTaxId?: string | null;
  saleTax2Id?: string | null;
  primarySupplierId?: string | null;
  weight?: string;

  // Atribut fisik & regulasi (legacy "Atribut")
  length?: string;
  width?: string;
  height?: string;
  volume?: string;
  conversionKgPcs?: string;
  registrationNo?: string | null;
  isReturnable?: boolean;
  isMobile?: boolean;

  ageCategory?: string | null;
  validUntil?: string | null;
  isVatable?: boolean;
  isSpecial?: boolean;
  isActive?: boolean;
}

export type UpdateItemPayload = Partial<CreateItemPayload>;
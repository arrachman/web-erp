import { Prisma } from '@prisma/client';
import { CreateErpPartnerDto } from './dto/create-erp-partner.dto';
import { UpdateErpPartnerDto } from './dto/update-erp-partner.dto';

/** Build junction rows for one multi-select dimension. */
export function buildDimRows<K extends string>(
  ids: string[] | undefined,
  key: K,
): Record<K, bigint>[] | undefined {
  if (!ids) return undefined;
  const unique = Array.from(new Set(ids.filter((v) => v !== '')));
  return unique.map((v) => ({ [key]: BigInt(v) }) as Record<K, bigint>);
}

/**
 * Denormalized single branch column = first branch id of the array (legacy
 * fallback). Returns undefined when the array was not sent (no change).
 */
export function firstBranchSync(branchIds: string[] | undefined): bigint | null | undefined {
  if (branchIds === undefined) return undefined;
  const f = branchIds.find((v) => v !== '');
  return f ? BigInt(f) : null;
}

/**
 * Build the Prisma `data` payload for create.
 *
 * Absent nullable IDs are coerced to `null` (create semantics). `buildDimRows`
 * is intentionally called twice per dimension — once for the conditional
 * spread gate, once for the actual `create` payload — preserving the original
 * service's evaluation behavior exactly.
 */
export function buildErpPartnerCreateData(
  dto: CreateErpPartnerDto,
  actorBigInt: bigint | null,
): Prisma.ErpPartnerUncheckedCreateInput {
  const categoryBigInt = dto.categoryId ? BigInt(dto.categoryId) : null;
  const partnerTypeBigInt = BigInt(dto.partnerTypeId);
  const customerCatBigInt = dto.customerCategoryId ? BigInt(dto.customerCategoryId) : null;
  const supplierCatBigInt = dto.supplierCategoryId ? BigInt(dto.supplierCategoryId) : null;
  const salesmanCatBigInt = dto.salesmanCategoryId ? BigInt(dto.salesmanCategoryId) : null;
  const salesmanBigInt = dto.salesmanId ? BigInt(dto.salesmanId) : null;
  const receivableBigInt = dto.receivableAccountId ? BigInt(dto.receivableAccountId) : null;
  const payableBigInt = dto.payableAccountId ? BigInt(dto.payableAccountId) : null;
  const currencyBigInt = dto.currencyId ? BigInt(dto.currencyId) : null;
  const saleTermBigInt = dto.saleTermId ? BigInt(dto.saleTermId) : null;
  const purchaseTermBigInt = dto.purchaseTermId ? BigInt(dto.purchaseTermId) : null;

  return {
    code: dto.code,
    name: dto.name,
    categoryId: categoryBigInt,
    partnerTypeId: partnerTypeBigInt,
    customerCategoryId: customerCatBigInt,
    supplierCategoryId: supplierCatBigInt,
    salesmanCategoryId: salesmanCatBigInt,
    salesmanId: salesmanBigInt,
    taxNumber: dto.taxNumber,
    isTaxable: dto.isTaxable ?? false,
    receivableAccountId: receivableBigInt,
    payableAccountId: payableBigInt,
    currencyId: currencyBigInt,
    saleTermId: saleTermBigInt,
    purchaseTermId: purchaseTermBigInt,
    arCreditLimit: dto.arCreditLimit ?? undefined,
    apCreditLimit: dto.apCreditLimit ?? undefined,
    salesPriceTier: dto.salesPriceTier ?? 1,
    branchId: firstBranchSync(dto.branchIds) ?? null,
    isActive: dto.isActive ?? true,
    createdById: actorBigInt,
    updatedById: actorBigInt,
    ...(buildDimRows(dto.branchIds, 'branchId')
      ? { dimBranches: { create: buildDimRows(dto.branchIds, 'branchId') } }
      : {}),
    ...(buildDimRows(dto.warehouseIds, 'warehouseId')
      ? { dimWarehouses: { create: buildDimRows(dto.warehouseIds, 'warehouseId') } }
      : {}),
    ...(buildDimRows(dto.locationIds, 'locationId')
      ? { dimLocations: { create: buildDimRows(dto.locationIds, 'locationId') } }
      : {}),
  };
}

/**
 * Build the Prisma `data` payload for update.
 *
 * CRITICAL create-vs-update distinction: update distinguishes `undefined`
 * ("no change", field absent in patch) from `null` ("clear the value").
 * Nullable ID conversions therefore return `undefined` when the DTO field is
 * absent, and `null` when it was sent but falsy/empty. Nested dimension ops
 * preserve `deleteMany: {}` + `create` inside the same Prisma update op, and
 * are only attached when the corresponding ids array was sent.
 */
export function buildErpPartnerUpdatePatch(
  dto: UpdateErpPartnerDto,
  actorBigInt: bigint | null,
): Prisma.ErpPartnerUncheckedUpdateInput {
  const categoryBigInt =
    dto.categoryId !== undefined ? (dto.categoryId ? BigInt(dto.categoryId) : undefined) : undefined;
  const partnerTypeBigInt =
    dto.partnerTypeId !== undefined
      ? dto.partnerTypeId
        ? BigInt(dto.partnerTypeId)
        : undefined
      : undefined;
  const customerCatBigInt =
    dto.customerCategoryId !== undefined
      ? dto.customerCategoryId
        ? BigInt(dto.customerCategoryId)
        : undefined
      : undefined;
  const supplierCatBigInt =
    dto.supplierCategoryId !== undefined
      ? dto.supplierCategoryId
        ? BigInt(dto.supplierCategoryId)
        : undefined
      : undefined;
  const salesmanCatBigInt =
    dto.salesmanCategoryId !== undefined
      ? dto.salesmanCategoryId
        ? BigInt(dto.salesmanCategoryId)
        : undefined
      : undefined;
  const salesmanBigInt =
    dto.salesmanId !== undefined ? (dto.salesmanId ? BigInt(dto.salesmanId) : undefined) : undefined;
  const receivableBigInt =
    dto.receivableAccountId !== undefined
      ? dto.receivableAccountId
        ? BigInt(dto.receivableAccountId)
        : undefined
      : undefined;
  const payableBigInt =
    dto.payableAccountId !== undefined
      ? dto.payableAccountId
        ? BigInt(dto.payableAccountId)
        : undefined
      : undefined;
  const currencyBigInt =
    dto.currencyId !== undefined ? (dto.currencyId ? BigInt(dto.currencyId) : undefined) : undefined;
  const saleTermBigInt =
    dto.saleTermId !== undefined ? (dto.saleTermId ? BigInt(dto.saleTermId) : undefined) : undefined;
  const purchaseTermBigInt =
    dto.purchaseTermId !== undefined
      ? dto.purchaseTermId
        ? BigInt(dto.purchaseTermId)
        : undefined
      : undefined;

  return {
    code: dto.code,
    name: dto.name,
    categoryId: categoryBigInt,
    partnerTypeId: partnerTypeBigInt,
    customerCategoryId: customerCatBigInt,
    supplierCategoryId: supplierCatBigInt,
    salesmanCategoryId: salesmanCatBigInt,
    salesmanId: salesmanBigInt,
    taxNumber: dto.taxNumber,
    isTaxable: dto.isTaxable,
    receivableAccountId: receivableBigInt,
    payableAccountId: payableBigInt,
    currencyId: currencyBigInt,
    saleTermId: saleTermBigInt,
    purchaseTermId: purchaseTermBigInt,
    arCreditLimit: dto.arCreditLimit !== undefined ? dto.arCreditLimit : undefined,
    apCreditLimit: dto.apCreditLimit !== undefined ? dto.apCreditLimit : undefined,
    salesPriceTier: dto.salesPriceTier !== undefined ? dto.salesPriceTier : undefined,
    branchId: firstBranchSync(dto.branchIds),
    isActive: dto.isActive,
    updatedById: actorBigInt,
    ...(dto.branchIds !== undefined
      ? { dimBranches: { deleteMany: {}, create: buildDimRows(dto.branchIds, 'branchId') } }
      : {}),
    ...(dto.warehouseIds !== undefined
      ? {
          dimWarehouses: {
            deleteMany: {},
            create: buildDimRows(dto.warehouseIds, 'warehouseId'),
          },
        }
      : {}),
    ...(dto.locationIds !== undefined
      ? {
          dimLocations: {
            deleteMany: {},
            create: buildDimRows(dto.locationIds, 'locationId'),
          },
        }
      : {}),
  };
}
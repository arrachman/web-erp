import { Prisma } from '@prisma/client';
import { QueryErpPartnerDto } from './dto/query-erp-partner.dto';

// Multi-select dimension junctions (md_partner_dim_*) — selected for read so the
// edit form can prefill the chips.
export const PARTNER_DIM_INCLUDE = {
  dimBranches: {
    select: { branchId: true, branch: { select: { id: true, code: true, name: true } } },
    orderBy: { id: 'asc' as const },
  },
  dimWarehouses: {
    select: { warehouseId: true, warehouse: { select: { id: true, code: true, name: true } } },
    orderBy: { id: 'asc' as const },
  },
  dimLocations: {
    select: { locationId: true, location: { select: { id: true, code: true, name: true } } },
    orderBy: { id: 'asc' as const },
  },
} as const;

export const PARTNER_TRX_INCLUDE = {
  currency: { select: { id: true, code: true, name: true, symbol: true } },
  saleTerm: { select: { id: true, code: true, name: true } },
  purchaseTerm: { select: { id: true, code: true, name: true } },
} as const;

export const PARTNER_REF_INCLUDE = {
  salesman: { select: { id: true, code: true, name: true } },
} as const;

/**
 * Category & account selects — used identically by findAll and findOne, so
 * shared as ONE consolidated const instead of duplicating the literal.
 */
export const PARTNER_RELATION_INCLUDE = {
  category: { select: { id: true, code: true, name: true, kind: true, salesTier: true } },
  partnerType: { select: { id: true, code: true, name: true, kind: true } },
  customerCategory: { select: { id: true, code: true, name: true } },
  supplierCategory: { select: { id: true, code: true, name: true } },
  salesmanCategory: { select: { id: true, code: true, name: true } },
  receivableAccount: { select: { id: true, code: true, name: true } },
  payableAccount: { select: { id: true, code: true, name: true } },
} as const;

/**
 * Address with its geographic relations — used by findOne (election) and
 * addAddress. Consolidated here so the nested include lives in ONE place.
 */
export const PARTNER_ADDRESS_GEO_INCLUDE = {
  country: { select: { id: true, name: true } },
  province: { select: { id: true, name: true } },
  city: { select: { id: true, name: true } },
  area: { select: { id: true, name: true, postalCode: true } },
  subArea: { select: { id: true, name: true, postalCode: true } },
} as const;

/** Relation include reused at the list (findAll) view. */
export const PARTNER_LIST_INCLUDE = {
  ...PARTNER_RELATION_INCLUDE,
  ...PARTNER_DIM_INCLUDE,
  ...PARTNER_TRX_INCLUDE,
  ...PARTNER_REF_INCLUDE,
} as const;

/** Relation include reused at the detail (findOne) view. */
export const PARTNER_DETAIL_INCLUDE = {
  ...PARTNER_RELATION_INCLUDE,
  addresses: {
    where: { deletedAt: null },
    orderBy: { createdAt: 'asc' },
    include: PARTNER_ADDRESS_GEO_INCLUDE,
  },
  contacts: { where: { deletedAt: null }, orderBy: { createdAt: 'asc' } },
  bankAccounts: { where: { deletedAt: null }, orderBy: { createdAt: 'asc' } },
  ...PARTNER_DIM_INCLUDE,
  ...PARTNER_TRX_INCLUDE,
  ...PARTNER_REF_INCLUDE,
} as const;

/** Relation include reused at create/update (CRUD mutation responses). */
export const PARTNER_MUTATION_INCLUDE = {
  ...PARTNER_RELATION_INCLUDE,
  ...PARTNER_DIM_INCLUDE,
  ...PARTNER_TRX_INCLUDE,
  ...PARTNER_REF_INCLUDE,
} as const;

/** Build the Prisma `where` clause for findAll from the query DTO. */
export function buildErpPartnerWhere(
  query: QueryErpPartnerDto,
): Prisma.ErpPartnerWhereInput {
  const where: Prisma.ErpPartnerWhereInput = { deletedAt: null };

  if (query.search?.trim()) {
    const q = query.search.trim();
    where.OR = [
      { code: { equals: q, mode: 'insensitive' } },
      { name: { contains: q, mode: 'insensitive' } },
      { taxNumber: { contains: q, mode: 'insensitive' } },
    ];
  }

  if (query.categoryId !== undefined) {
    where.categoryId = BigInt(query.categoryId);
  }

  if (query.partnerTypeId !== undefined) {
    where.partnerTypeId = BigInt(query.partnerTypeId);
  }

  if (query.typeKind !== undefined) {
    where.partnerType = { kind: query.typeKind, deletedAt: null, isActive: true };
  }

  if (query.isActive !== undefined) {
    where.isActive = query.isActive;
  }

  return where;
}

/** Build the dynamic `orderBy` clause preserving `[sortBy]: sortDir` semantics. */
export function buildErpPartnerOrderBy(
  query: QueryErpPartnerDto,
): Prisma.ErpPartnerOrderByWithRelationInput[] {
  const sortBy = query.sortBy ?? 'createdAt';
  const sortDir = query.sortDir ?? 'desc';
  return [{ [sortBy]: sortDir }];
}
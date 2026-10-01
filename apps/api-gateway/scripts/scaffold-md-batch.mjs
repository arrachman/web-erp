#!/usr/bin/env node
// One-shot scaffolder for the md-legacy-batch entities (2026-05-20).
// Generates NestJS module (controller+service+module+DTOs) + FE page + FE API client
// for each entity in the catalog. Pattern mirrors erp-divisions.

import { mkdirSync, writeFileSync, existsSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const apiRoot = resolve(__dirname, '..', 'src');
const feRoot = resolve(__dirname, '..', '..', 'web-erp');

/** @type {Array<{model:string,table:string,kebab:string,plural:string,labelId:string,labelEn:string,code:string,extraFields?:Array<{name:string,type:string,required?:boolean,db:string,decorators?:string[],example?:any,desc?:string}>,extraSortable?:string[]}>} */
const ENTITIES = [
  { model: 'ErpBrand', table: 'md_brands', kebab: 'brands', plural: 'brands', labelId: 'Brand', labelEn: 'Brand', code: 'BRD' },
  { model: 'ErpMaterial', table: 'md_materials', kebab: 'materials', plural: 'materials', labelId: 'Material', labelEn: 'Material', code: 'MAT' },
  { model: 'ErpItemModel', table: 'md_item_models', kebab: 'item-models', plural: 'itemModels', labelId: 'Model', labelEn: 'Model', code: 'MDL' },
  { model: 'ErpSize', table: 'md_sizes', kebab: 'sizes', plural: 'sizes', labelId: 'Ukuran', labelEn: 'Size', code: 'SZ' },
  { model: 'ErpSection', table: 'md_sections', kebab: 'sections', plural: 'sections', labelId: 'Seksi', labelEn: 'Section', code: 'SEC' },
  { model: 'ErpItemKind', table: 'md_item_types', kebab: 'item-types', plural: 'itemKinds', labelId: 'Tipe Item', labelEn: 'Item Type', code: 'ITP' },
  { model: 'ErpProductClass', table: 'md_product_classes', kebab: 'product-classes', plural: 'productClasses', labelId: 'Kelas Produk', labelEn: 'Product Class', code: 'PCL' },
  { model: 'ErpBank', table: 'md_banks', kebab: 'banks', plural: 'banks', labelId: 'Bank', labelEn: 'Bank', code: 'BNK' },
  { model: 'ErpExpedition', table: 'md_expeditions', kebab: 'expeditions', plural: 'expeditions', labelId: 'Ekspedisi', labelEn: 'Expedition', code: 'EXP' },
  { model: 'ErpOtherCost', table: 'md_other_costs', kebab: 'other-costs', plural: 'otherCosts', labelId: 'Biaya Lain', labelEn: 'Other Cost', code: 'OCT' },
  {
    model: 'ErpCommission', table: 'md_commissions', kebab: 'commissions', plural: 'commissions', labelId: 'Komisi', labelEn: 'Commission', code: 'CMS',
    extraFields: [{ name: 'amount', type: 'string', db: 'Decimal', desc: 'Commission amount (Decimal as string)', example: '0', decorators: ['@IsOptional()', '@IsString()'] }],
  },
  {
    model: 'ErpStockAdjustmentType', table: 'md_stock_adjustment_types', kebab: 'stock-adjustment-types', plural: 'stockAdjustmentTypes', labelId: 'Tipe Penyesuaian Stok', labelEn: 'Stock Adjustment Type', code: 'SAT',
    extraFields: [{ name: 'direction', type: 'string', db: 'string', desc: 'IN|OUT|TRANSFER', example: 'IN', decorators: ['@IsOptional()', "@IsIn(['IN','OUT','TRANSFER'])"] }],
  },
  {
    model: 'ErpCountry', table: 'md_countries', kebab: 'countries', plural: 'countries', labelId: 'Negara', labelEn: 'Country', code: 'CTY',
    extraFields: [{ name: 'isoCode', type: 'string', db: 'string?', desc: 'ISO 3166-1 alpha-2', example: 'ID', decorators: ['@IsOptional()', '@IsString()'] }],
  },
  {
    model: 'ErpProvince', table: 'md_provinces', kebab: 'provinces', plural: 'provinces', labelId: 'Provinsi', labelEn: 'Province', code: 'PRV',
    extraFields: [{ name: 'countryId', type: 'string', db: 'BigInt', required: true, desc: 'Country id', example: '1', decorators: ['@IsString()'] }],
    extraSortable: ['countryId'],
  },
  {
    model: 'ErpCity', table: 'md_cities', kebab: 'cities', plural: 'cities', labelId: 'Kota', labelEn: 'City', code: 'CTYC',
    extraFields: [{ name: 'provinceId', type: 'string', db: 'BigInt', required: true, desc: 'Province id', example: '1', decorators: ['@IsString()'] }],
    extraSortable: ['provinceId'],
  },
  {
    model: 'ErpArea', table: 'md_areas', kebab: 'areas', plural: 'areas', labelId: 'Area', labelEn: 'Area', code: 'AREA',
    extraFields: [{ name: 'cityId', type: 'string', db: 'BigInt', required: true, desc: 'City id', example: '1', decorators: ['@IsString()'] }],
    extraSortable: ['cityId'],
  },
  {
    model: 'ErpItemLocation', table: 'md_item_locations', kebab: 'item-locations', plural: 'itemLocations', labelId: 'Lokasi Item', labelEn: 'Item Location', code: 'ILC',
    extraFields: [{ name: 'warehouseId', type: 'string', db: 'BigInt?', desc: 'Warehouse id', example: '1', decorators: ['@IsOptional()', '@IsString()'] }],
  },
  {
    model: 'ErpPartnerSubCategory', table: 'md_partner_sub_categories', kebab: 'partner-sub-categories', plural: 'partnerSubCategories', labelId: 'Sub Kategori Partner', labelEn: 'Partner Sub Category', code: 'PSC',
    extraFields: [{ name: 'type', type: "'CUSTOMER'|'SUPPLIER'|'SALESMAN'", db: 'string', required: true, desc: 'Customer/Supplier/Salesman', example: 'CUSTOMER', decorators: ["@IsIn(['CUSTOMER','SUPPLIER','SALESMAN'])"] }],
    extraSortable: ['type'],
  },
  { model: 'ErpPriceCategory', table: 'md_price_categories', kebab: 'price-categories', plural: 'priceCategories', labelId: 'Kategori Harga', labelEn: 'Price Category', code: 'PRC' },
  { model: 'ErpTransactionNote', table: 'md_transaction_notes', kebab: 'transaction-notes', plural: 'transactionNotes', labelId: 'Catatan Transaksi', labelEn: 'Transaction Note', code: 'TXN' },
  {
    model: 'ErpItemPermission', table: 'md_item_permissions', kebab: 'item-permissions', plural: 'itemPermissions', labelId: 'Izin Item', labelEn: 'Item Permission', code: 'IPM',
    extraFields: [
      { name: 'itemId', type: 'string', db: 'BigInt', required: true, desc: 'Item id', example: '1', decorators: ['@IsString()'] },
      { name: 'roleId', type: 'string', db: 'BigInt', required: true, desc: 'Role id', example: '1', decorators: ['@IsString()'] },
      { name: 'canView', type: 'boolean', db: 'boolean', desc: 'Can view', example: true, decorators: ['@IsOptional()', '@IsBoolean()'] },
      { name: 'canSell', type: 'boolean', db: 'boolean', desc: 'Can sell', example: true, decorators: ['@IsOptional()', '@IsBoolean()'] },
      { name: 'canBuy', type: 'boolean', db: 'boolean', desc: 'Can buy', example: true, decorators: ['@IsOptional()', '@IsBoolean()'] },
    ],
  },
];

// Items WITHOUT `code` unique (PartnerSubCategory has @@unique([code, type]) but for simplicity we still treat code unique-ish per type — for now, skip the per-type check in service; the @@unique([code,type]) at DB level prevents true duplicates)
const NO_GLOBAL_CODE_UNIQUE = new Set(['ErpPartnerSubCategory']);

// --- File templates ---

function tplCreateDto(e) {
  const extras = (e.extraFields || []).map((f) => `  @ApiPropertyOptional({ example: ${JSON.stringify(f.example)}, description: ${JSON.stringify(f.desc || '')} })
${(f.decorators || ['@IsOptional()', '@IsString()']).map((d) => `  ${d}`).join('\n')}
  ${f.name}${f.required ? '!' : '?'}: ${f.type};`).join('\n\n');
  return `import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsIn, IsOptional, IsString, MaxLength } from 'class-validator';

export class Create${e.model}Dto {
  @ApiProperty({ example: '${e.code}-001' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: '${e.labelEn} Sample' })
  @IsString()
  @MaxLength(150)
  name!: string;

${extras}

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}
`;
}

function tplUpdateDto(e) {
  return `import { PartialType } from '@nestjs/swagger';
import { Create${e.model}Dto } from './create-${e.kebab}.dto';

export class Update${e.model}Dto extends PartialType(Create${e.model}Dto) {}
`;
}

function tplQueryDto(e) {
  const sortable = ['code', 'name', 'isActive', 'createdAt', ...(e.extraSortable || [])];
  return `import { ApiPropertyOptional } from '@nestjs/swagger';
import { Transform, Type } from 'class-transformer';
import { IsBoolean, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';

const SORTABLE_FIELDS = ${JSON.stringify(sortable)} as const;
type SortableField = (typeof SORTABLE_FIELDS)[number];

export class Query${e.model}Dto {
  @ApiPropertyOptional({ default: 1 }) @IsOptional() @Type(() => Number) @IsInt() @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ default: 10 }) @IsOptional() @Type(() => Number) @IsInt() @Min(1) @Max(100)
  limit?: number = 10;

  @ApiPropertyOptional() @IsOptional() @IsString()
  search?: string;

  @ApiPropertyOptional() @IsOptional()
  @Transform(({ value }) => (value === 'true' ? true : value === 'false' ? false : value))
  @IsBoolean()
  isActive?: boolean;

  @ApiPropertyOptional({ enum: SORTABLE_FIELDS }) @IsOptional() @IsIn(SORTABLE_FIELDS)
  sortBy?: SortableField;

  @ApiPropertyOptional({ enum: ['asc', 'desc'] }) @IsOptional() @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc';
}
`;
}

function tplBulkDto(e) {
  return `import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsArray, IsBoolean, IsOptional, IsString, ArrayMinSize } from 'class-validator';

export class Bulk${e.model}Dto {
  @ApiProperty({ type: [String] }) @IsArray() @ArrayMinSize(1) @IsString({ each: true })
  ids!: string[];
}

export class BulkStatus${e.model}Dto {
  @ApiProperty({ type: [String] }) @IsArray() @ArrayMinSize(1) @IsString({ each: true })
  ids!: string[];

  @ApiPropertyOptional() @IsBoolean() @IsOptional()
  isActive!: boolean;
}
`;
}

function prismaField(e, field) {
  // map DTO field → Prisma data assignment
  const f = e.extraFields?.find((x) => x.name === field);
  if (!f) return null;
  if (f.db === 'BigInt') return `dto.${field} ? BigInt(dto.${field}) : null`;
  if (f.db === 'BigInt?') return `dto.${field} === undefined ? undefined : dto.${field} ? BigInt(dto.${field}) : null`;
  if (f.db === 'Decimal') return `dto.${field}`;
  return `dto.${field}`;
}

function tplService(e) {
  const prismaProp = e.model.charAt(3).toLowerCase() + e.model.slice(4); // ErpBrand → erpBrand → brand prop on prisma is erpBrand
  const prismaAccessor = e.model.charAt(0).toLowerCase() + e.model.slice(1); // erpBrand
  const extraCreate = (e.extraFields || []).map((f) => `        ${f.name}: ${prismaField(e, f.name) || 'dto.' + f.name},`).join('\n');
  const extraUpdate = (e.extraFields || []).map((f) => `        ${f.name}: ${prismaField(e, f.name) || 'dto.' + f.name},`).join('\n');
  const uniqueKey = `${e.table}_code_key`;
  const dupCheck = NO_GLOBAL_CODE_UNIQUE.has(e.model)
    ? `// skip global unique check; @@unique([code,type]) at DB layer handles duplicates`
    : `const existing = await this.prisma.${prismaAccessor}.findFirst({ where: { code: dto.code }, select: { id: true, deletedAt: true } });
    if (existing) throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code, isSoftDeleted: Boolean(existing.deletedAt) });`;
  const dupCheckUpdate = NO_GLOBAL_CODE_UNIQUE.has(e.model)
    ? ''
    : `if (dto.code && dto.code !== existing.code) {
      const duplicate = await this.prisma.${prismaAccessor}.findFirst({ where: { code: dto.code, NOT: { id } }, select: { id: true, deletedAt: true } });
      if (duplicate) throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code, isSoftDeleted: Boolean(duplicate.deletedAt) });
    }`;
  return `import { Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { isUniqueViolation, throwDuplicate } from '../common/errors/duplicate.util';
import { PrismaService } from '../prisma/prisma.service';
import { ErpAuditService } from '../erp-audit/erp-audit.service';
import { diffFields } from '../erp-common/utils/diff-fields.util';
import { Bulk${e.model}Dto, BulkStatus${e.model}Dto } from './dto/bulk-${e.kebab}.dto';
import { Create${e.model}Dto } from './dto/create-${e.kebab}.dto';
import { Query${e.model}Dto } from './dto/query-${e.kebab}.dto';
import { Update${e.model}Dto } from './dto/update-${e.kebab}.dto';

const ENTITY = '${e.model}';
const FIELD_LABEL = '${e.labelEn} code';
const UNIQUE_KEY = '${uniqueKey}';
const LABEL_ID = '${e.labelId}';

@Injectable()
export class ${e.model}sService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly audit: ErpAuditService,
  ) {}

  async create(dto: Create${e.model}Dto, actorId?: string) {
    ${dupCheck}
    const actorBigInt = actorId ? BigInt(actorId) : null;
    let created;
    try {
      created = await this.prisma.${prismaAccessor}.create({
        data: {
          code: dto.code,
          name: dto.name,
${extraCreate}
          isActive: dto.isActive ?? true,
          createdById: actorBigInt,
          updatedById: actorBigInt,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', UNIQUE_KEY])) {
        throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code });
      }
      throw error;
    }
    this.audit.log({
      action: 'CREATE', entityName: ENTITY, entityId: created.id,
      summary: \`\${LABEL_ID} \${created.code} dibuat\`, actorId: actorBigInt ?? undefined,
    });
    return { success: true, data: created };
  }

  async findAll(query: Query${e.model}Dto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 10;
    const skip = (page - 1) * limit;
    const where: Prisma.${e.model}WhereInput = { deletedAt: null };
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { code: { contains: q, mode: 'insensitive' } },
        { name: { contains: q, mode: 'insensitive' } },
      ];
    }
    if (query.isActive !== undefined) where.isActive = query.isActive;
    const sortBy = query.sortBy ?? 'createdAt';
    const sortDir = query.sortDir ?? 'desc';
    const [items, total] = await this.prisma.$transaction([
      this.prisma.${prismaAccessor}.findMany({ where, orderBy: [{ [sortBy]: sortDir }], skip, take: limit }),
      this.prisma.${prismaAccessor}.count({ where }),
    ]);
    return { success: true, data: items, meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 } };
  }

  async findOne(id: bigint) {
    const item = await this.prisma.${prismaAccessor}.findFirst({ where: { id, deletedAt: null } });
    if (!item) throw new NotFoundException(\`\${ENTITY} not found\`);
    return { success: true, data: item };
  }

  async update(id: bigint, dto: Update${e.model}Dto, actorId?: string) {
    const existing = await this.prisma.${prismaAccessor}.findFirst({ where: { id, deletedAt: null } });
    if (!existing) throw new NotFoundException(\`\${ENTITY} not found\`);
    ${dupCheckUpdate}
    const actorBigInt = actorId ? BigInt(actorId) : null;
    let updated;
    try {
      updated = await this.prisma.${prismaAccessor}.update({
        where: { id },
        data: {
          code: dto.code,
          name: dto.name,
${extraUpdate}
          isActive: dto.isActive,
          updatedById: actorBigInt,
        },
      });
    } catch (error) {
      if (isUniqueViolation(error, ['code', UNIQUE_KEY])) {
        throwDuplicate({ fieldLabel: FIELD_LABEL, value: dto.code ?? existing.code });
      }
      throw error;
    }
    const changes = diffFields(existing as unknown as Record<string, unknown>, updated as unknown as Record<string, unknown>);
    this.audit.log({
      action: 'UPDATE', entityName: ENTITY, entityId: id, changes,
      summary: \`\${LABEL_ID} \${updated.code} diperbarui\`, actorId: actorBigInt ?? undefined,
    });
    return { success: true, data: updated };
  }

  async bulkUpdateStatus(dto: BulkStatus${e.model}Dto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.${prismaAccessor}.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { isActive: dto.isActive, updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async bulkDelete(dto: Bulk${e.model}Dto, actorId?: string) {
    const ids = dto.ids.map((id) => BigInt(id));
    const actorBigInt = actorId ? BigInt(actorId) : null;
    const { count } = await this.prisma.${prismaAccessor}.updateMany({
      where: { id: { in: ids }, deletedAt: null },
      data: { deletedAt: new Date(), updatedById: actorBigInt, updatedAt: new Date() },
    });
    return { success: true, affected: count };
  }

  async remove(id: bigint, actorId?: string) {
    const existing = await this.prisma.${prismaAccessor}.findFirst({ where: { id, deletedAt: null }, select: { id: true } });
    if (!existing) throw new NotFoundException(\`\${ENTITY} not found\`);
    const actorBigInt = actorId ? BigInt(actorId) : null;
    await this.prisma.${prismaAccessor}.update({ where: { id }, data: { deletedAt: new Date(), updatedById: actorBigInt } });
    this.audit.log({ action: 'DELETE', entityName: ENTITY, entityId: id, summary: \`\${LABEL_ID} id=\${id} dihapus\`, actorId: actorBigInt ?? undefined });
    return { success: true, message: \`\${ENTITY} deleted\` };
  }
}
`;
}

function tplController(e) {
  return `import { Body, Controller, Delete, Get, Param, Patch, Post, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { Bulk${e.model}Dto, BulkStatus${e.model}Dto } from './dto/bulk-${e.kebab}.dto';
import { Create${e.model}Dto } from './dto/create-${e.kebab}.dto';
import { Query${e.model}Dto } from './dto/query-${e.kebab}.dto';
import { Update${e.model}Dto } from './dto/update-${e.kebab}.dto';
import { ${e.model}sService } from './${e.kebab}.service';

@ApiTags('ERP ${e.labelEn}s')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/${e.kebab}')
export class ${e.model}sController {
  constructor(private readonly service: ${e.model}sService) {}

  @Post()
  @ApiOperation({ summary: 'Create ${e.labelEn}' })
  @ApiResponse({ status: 201 })
  create(@Body() dto: Create${e.model}Dto, @Request() req: any) { return this.service.create(dto, req.user?.id); }

  @Get()
  @ApiOperation({ summary: 'List ${e.labelEn}' })
  findAll(@Query() query: Query${e.model}Dto) { return this.service.findAll(query); }

  @Get(':id')
  @ApiOperation({ summary: 'Get ${e.labelEn}' })
  findOne(@Param('id') id: string) { return this.service.findOne(BigInt(id)); }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ${e.labelEn}' })
  update(@Param('id') id: string, @Body() dto: Update${e.model}Dto, @Request() req: any) { return this.service.update(BigInt(id), dto, req.user?.id); }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate' })
  bulkUpdateStatus(@Body() dto: BulkStatus${e.model}Dto, @Request() req: any) { return this.service.bulkUpdateStatus(dto, req.user?.id); }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk soft-delete' })
  bulkDelete(@Body() dto: Bulk${e.model}Dto, @Request() req: any) { return this.service.bulkDelete(dto, req.user?.id); }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete ${e.labelEn}' })
  remove(@Param('id') id: string, @Request() req: any) { return this.service.remove(BigInt(id), req.user?.id); }
}
`;
}

function tplModule(e) {
  return `import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ${e.model}sController } from './${e.kebab}.controller';
import { ${e.model}sService } from './${e.kebab}.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [${e.model}sController],
  providers: [${e.model}sService],
  exports: [${e.model}sService],
})
export class ${e.model}sModule {}
`;
}

function tplFeApi(e) {
  const extraTypeFields = (e.extraFields || []).map((f) => `  ${f.name}${f.required ? '' : '?'}: ${f.type === "'CUSTOMER'|'SUPPLIER'|'SALESMAN'" ? f.type : f.type === 'boolean' ? 'boolean | null' : f.type + ' | null'};`).join('\n');
  const createPayloadFields = (e.extraFields || []).map((f) => `  ${f.name}${f.required ? '' : '?'}: ${f.type};`).join('\n');
  return `// ERP ${e.labelEn} resource API — CRUD for ${e.table}
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ${e.model} {
  id: string;
  code: string;
  name: string;
${extraTypeFields}
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Create${e.model}Payload {
  code: string;
  name: string;
${createPayloadFields}
  isActive?: boolean;
}

export type Update${e.model}Payload = Partial<Create${e.model}Payload>;

const BASE = '/${e.kebab}';

export async function list${capitalize(e.plural)}(params?: PaginationParams): Promise<PaginatedResponse<${e.model}>> {
  return apiGet<PaginatedResponse<${e.model}>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function create${e.model}(payload: Create${e.model}Payload): Promise<${e.model}> {
  const res = await apiPost<ApiResponse<${e.model}>>(BASE, payload);
  return res.data;
}

export async function update${e.model}(id: string, payload: Update${e.model}Payload): Promise<${e.model}> {
  const res = await apiPatch<ApiResponse<${e.model}>>(\`\${BASE}/\${id}\`, payload);
  return res.data;
}

export async function delete${e.model}(id: string): Promise<void> {
  await apiDelete<void>(\`\${BASE}/\${id}\`);
}

export async function bulkUpdate${e.model}Status(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(\`\${BASE}/bulk/status\`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDelete${e.model}s(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(\`\${BASE}/bulk\`, { ids });
  return { affected: res.affected };
}
`;
}

function tplFePage(e) {
  const apiFile = e.kebab; // file name in lib/api
  // For now, simple form with code + name + isActive. Extra fields appear as plain inputs.
  const extraFormFields = (e.extraFields || []).map((f) => {
    if (f.type === 'boolean') {
      return `      <FormField label="${capitalize(f.name)}" htmlFor="ef-${f.name}">
        <BooleanRadio id="ef-${f.name}" value={Boolean(data.${f.name})} onValueChange={(v) => set('${f.name}', v)} />
      </FormField>`;
    }
    return `      <FormField label="${capitalize(f.name)}" htmlFor="ef-${f.name}">
        <Input id="ef-${f.name}" value={data.${f.name} ?? ''} onChange={(e) => set('${f.name}', e.target.value)} />
      </FormField>`;
  }).join('\n');
  const formStateFields = (e.extraFields || []).map((f) => {
    const def = f.type === 'boolean' ? 'true' : "''";
    return `  ${f.name}: ${f.type === 'boolean' ? 'boolean' : 'string'};`;
  }).join('\n');
  const defaultFormExtra = (e.extraFields || []).map((f) => {
    const def = f.type === 'boolean' ? 'true' : f.type === "'CUSTOMER'|'SUPPLIER'|'SALESMAN'" ? "'CUSTOMER'" : "''";
    return `  ${f.name}: ${def},`;
  }).join('\n');
  const fromRecordExtra = (e.extraFields || []).map((f) => {
    if (f.type === 'boolean') return `  ${f.name}: Boolean(r.${f.name}),`;
    return `  ${f.name}: r.${f.name} == null ? '' : String(r.${f.name}),`;
  }).join('\n');
  const toPayloadExtra = (e.extraFields || []).map((f) => {
    if (f.type === 'boolean') return `  ${f.name}: f.${f.name},`;
    return `  ${f.name}: f.${f.name} || undefined,`;
  }).join('\n');
  return `'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  list${capitalize(e.plural)}, create${e.model}, update${e.model}, delete${e.model},
  bulkUpdate${e.model}Status, bulkDelete${e.model}s,
  type ${e.model}, type Create${e.model}Payload,
} from '@/lib/api/${apiFile}';

interface FormData {
  code: string;
  name: string;
${formStateFields}
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
${defaultFormExtra}
  isActive: true,
});

const fromRecord = (r: ${e.model}): FormData => ({
  code: r.code,
  name: r.name,
${fromRecordExtra}
  isActive: r.isActive,
});

const toPayload = (f: FormData): Create${e.model}Payload => ({
  code: f.code,
  name: f.name,
${toPayloadExtra}
  isActive: f.isActive,
});

function FormFields({ data, onChange }: { data: FormData; onChange: (d: FormData) => void }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="ef-code" required>
        <Input id="ef-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="${e.code}-001" />
      </FormField>
      <FormField label="Nama" htmlFor="ef-name" required>
        <Input id="ef-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="${e.labelEn}" />
      </FormField>
${extraFormFields}
      <FormField label="Status" htmlFor="ef-active">
        <BooleanRadio id="ef-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ${e.model}sPage() {
  return (
    <SimpleMasterPage<${e.model}, FormData>
      title="${e.labelEn}"
      code="${e.code}"
      entityLabel="${e.labelEn.toLowerCase()}"
      storageKey="${e.kebab}"
      auditEntityName="${e.model}"
      list={list${capitalize(e.plural)}}
      create={create${e.model}}
      update={update${e.model}}
      remove={delete${e.model}}
      bulkStatus={bulkUpdate${e.model}Status}
      bulkDelete={bulkDelete${e.model}s}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
    />
  );
}
`;
}

function capitalize(s) { return s.charAt(0).toUpperCase() + s.slice(1); }

function writeIfMissing(path, content) {
  if (existsSync(path)) {
    console.log(`SKIP ${path} (exists)`);
    return false;
  }
  mkdirSync(dirname(path), { recursive: true });
  writeFileSync(path, content);
  console.log(`WROTE ${path}`);
  return true;
}

let written = 0;
for (const e of ENTITIES) {
  const apiDir = join(apiRoot, `erp-${e.kebab}`);
  written += writeIfMissing(join(apiDir, 'dto', `create-${e.kebab}.dto.ts`), tplCreateDto(e));
  written += writeIfMissing(join(apiDir, 'dto', `update-${e.kebab}.dto.ts`), tplUpdateDto(e));
  written += writeIfMissing(join(apiDir, 'dto', `query-${e.kebab}.dto.ts`), tplQueryDto(e));
  written += writeIfMissing(join(apiDir, 'dto', `bulk-${e.kebab}.dto.ts`), tplBulkDto(e));
  written += writeIfMissing(join(apiDir, `${e.kebab}.service.ts`), tplService(e));
  written += writeIfMissing(join(apiDir, `${e.kebab}.controller.ts`), tplController(e));
  written += writeIfMissing(join(apiDir, `${e.kebab}.module.ts`), tplModule(e));
  written += writeIfMissing(join(feRoot, 'lib', 'api', `${e.kebab}.ts`), tplFeApi(e));
  written += writeIfMissing(join(feRoot, 'components', 'pages', `${e.kebab}-page.tsx`), tplFePage(e));
}
console.log(`\nDone. Wrote ${written} files for ${ENTITIES.length} entities.`);

// Emit AppModule import/registration snippets
console.log('\n--- AppModule snippets to paste ---');
for (const e of ENTITIES) {
  console.log(`import { ${e.model}sModule } from './erp-${e.kebab}/${e.kebab}.module';`);
}
console.log('// imports array:');
for (const e of ENTITIES) {
  console.log(`    ${e.model}sModule,`);
}

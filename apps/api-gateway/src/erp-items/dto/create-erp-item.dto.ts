import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsArray,
  IsBoolean,
  IsDateString,
  IsEnum,
  IsNotEmpty,
  IsOptional,
  IsString,
  MaxLength,
  ValidateNested,
} from 'class-validator';
import { Type } from 'class-transformer';
import { ErpCostingMethod, ErpItemType } from '@prisma/client';
import { ItemPriceDto } from './item-price.dto';
import { ItemDistributorDto } from './item-distributor.dto';
import { ItemWarehouseStockDto } from './item-warehouse-stock.dto';
import { ItemOthersDto, ItemCustomDto } from './item-metadata.dto';

export class CreateErpItemDto {
  // ─── Core identity ────────────────────────────────────────────────
  @ApiProperty({ example: 'ITM-001' })
  @IsString()
  @IsNotEmpty()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Steel Rod 10mm' })
  @IsString()
  @IsNotEmpty()
  @MaxLength(255)
  name!: string;

  @ApiProperty({ enum: ErpItemType, example: ErpItemType.INVENTORY })
  @IsEnum(ErpItemType)
  itemType!: ErpItemType;

  @ApiProperty({ example: '1', description: 'Category ID (BigInt as string)' })
  @IsString()
  @IsNotEmpty()
  categoryId!: string;

  @ApiProperty({ example: '1', description: 'Base unit ID (BigInt as string)' })
  @IsString()
  @IsNotEmpty()
  unitId!: string;

  @ApiPropertyOptional({ example: 'D6 10mm steel rod' })
  @IsOptional()
  @IsString()
  @MaxLength(1000)
  description?: string;

  @ApiPropertyOptional({ example: '12345678' })
  @IsOptional()
  @IsString()
  @MaxLength(100)
  barcode?: string;

  // ─── Costing method (HPP) ─────────────────────────────────────────
  @ApiPropertyOptional({ enum: ErpCostingMethod, default: ErpCostingMethod.AVG })
  @IsOptional()
  @IsEnum(ErpCostingMethod)
  costMethod?: ErpCostingMethod;

  // ─── Classification (lookups) ─────────────────────────────────────
  @ApiPropertyOptional({ description: 'Item kind ID (md_item_types) — legacy "Tipe"' })
  @IsOptional()
  @IsString()
  kindId?: string | null;

  @ApiPropertyOptional({
    description: 'Product class ID (md_product_classes) — legacy "Kelas Produk"',
  })
  @IsOptional()
  @IsString()
  productClassId?: string | null;

  @ApiPropertyOptional() @IsOptional() @IsString() brandId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() materialId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() itemModelId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() sizeId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() colorId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() sectionId?: string | null;

  // ─── Atribut produk (legacy "Atribut") ────────────────────────────
  @ApiPropertyOptional({ description: 'Desainer ID (md_designers)' })
  @IsOptional()
  @IsString()
  designerId?: string | null;
  @ApiPropertyOptional({ description: 'Nozzle ID (md_nozzles)' })
  @IsOptional()
  @IsString()
  nozzleId?: string | null;
  @ApiPropertyOptional({ description: 'OEM ID (md_oems)' })
  @IsOptional()
  @IsString()
  oemId?: string | null;
  @ApiPropertyOptional({ description: 'Vendor ID (md_partners) — legacy "Vendor"' })
  @IsOptional()
  @IsString()
  vendorId?: string | null;
  @ApiPropertyOptional({ description: 'Satuan Jual Default / default selling unit ID (md_units)' })
  @IsOptional()
  @IsString()
  fieldUnitId?: string | null;
  // ─── GL / organizational dimensions ───────────────────────────────
  @ApiPropertyOptional() @IsOptional() @IsString() divisionId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() subdivisionId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() departmentId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() subDepartmentId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() defaultLocationId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() defaultWarehouseId?: string | null;

  // Multi-select GL dimensions (md_item_dim_*). When sent, the matching single
  // column above is synced server-side to the first id (denormalized default).
  @ApiPropertyOptional({ type: [String], description: 'Cabang multi-select (BigInt ids)' })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  branchIds?: string[];

  @ApiPropertyOptional({ type: [String], description: 'Gudang Default multi-select (BigInt ids)' })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  defaultWarehouseIds?: string[];

  @ApiPropertyOptional({ type: [String], description: 'Lokasi Default multi-select (BigInt ids)' })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  defaultLocationIds?: string[];
  @ApiPropertyOptional() @IsOptional() @IsString() projectId?: string | null;
  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string | null;

  // ─── Costs & prices ───────────────────────────────────────────────
  // System-managed (read-only in UI), written by purchase posting, not by this DTO:
  //   purchasePrice = "Harga Beli Terakhir" (last gross buy price)
  //   lastHpp       = "HPP Terakhir"       (net landed cost of latest receipt)
  //   averageCost   = "HPP Rata-rata"      (moving average)
  // standardCost ("HPP Update", manual) was removed from the master form.
  @ApiPropertyOptional({ example: '50000', description: 'Harga Beli Terakhir (system-managed; accepted for seed/import only)' })
  @IsOptional()
  @IsString()
  purchasePrice?: string;
  @ApiPropertyOptional({ example: '5', description: 'Diskon Pembelian (percent)' })
  @IsOptional()
  @IsString()
  purchaseDiscount?: string;
  @ApiPropertyOptional({
    example: '75000',
    description: 'Harga Jual 1 (mirror of price tier level 1)',
  })
  @IsOptional()
  @IsString()
  salePrice?: string;

  @ApiPropertyOptional({
    type: [ItemPriceDto],
    description: 'Sale price tiers (Harga Jual 1..10 + Diskon Jual 1..10)',
  })
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => ItemPriceDto)
  prices?: ItemPriceDto[];

  @ApiPropertyOptional({
    type: [ItemDistributorDto],
    description: 'Item distributors (Distributor tab: supplier partners)',
  })
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => ItemDistributorDto)
  distributors?: ItemDistributorDto[];

  // ─── Stock levels ─────────────────────────────────────────────────
  @ApiPropertyOptional({ example: '10' }) @IsOptional() @IsString() minStock?: string;
  @ApiPropertyOptional({ example: '500' }) @IsOptional() @IsString() maxStock?: string;
  @ApiPropertyOptional({ example: '50' }) @IsOptional() @IsString() reorderQty?: string;
  @ApiPropertyOptional({ example: '5' }) @IsOptional() @IsString() minOrderQty?: string;

  @ApiPropertyOptional({
    type: [ItemWarehouseStockDto],
    description:
      'Per-warehouse overrides of Stok Min/Maks + Min Order (global values above stay the default)',
  })
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => ItemWarehouseStockDto)
  warehouseStocks?: ItemWarehouseStockDto[];

  // ─── Tracking flags ───────────────────────────────────────────────
  @ApiPropertyOptional({ default: false }) @IsOptional() @IsBoolean() tracksSerial?: boolean =
    false;
  @ApiPropertyOptional({ default: false }) @IsOptional() @IsBoolean() tracksBatch?: boolean = false;
  @ApiPropertyOptional({ default: false }) @IsOptional() @IsBoolean() tracksBin?: boolean = false;

  // ─── GL accounts ──────────────────────────────────────────────────
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() inventoryAccountId?:
    | string
    | null;
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() salesAccountId?: string | null;
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() cogsAccountId?: string | null;
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() salesReturnAccountId?:
    | string
    | null;
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() salesDiscountAccountId?:
    | string
    | null;
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() purchaseReturnAccountId?:
    | string
    | null;
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() purchaseDiscountAccountId?:
    | string
    | null;
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() consignmentAccountId?:
    | string
    | null;

  // ─── Tax ──────────────────────────────────────────────────────────
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() purchaseTaxId?: string | null;
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() purchaseTax2Id?: string | null;
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() saleTaxId?: string | null;
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() saleTax2Id?: string | null;

  // ─── Supplier & physical ──────────────────────────────────────────
  @ApiPropertyOptional({ nullable: true }) @IsOptional() @IsString() primarySupplierId?:
    | string
    | null;
  @ApiPropertyOptional({ example: '1.5' }) @IsOptional() @IsString() weight?: string;

  // ─── Dimensi fisik & regulasi (legacy "Atribut") ──────────────────
  @ApiPropertyOptional({ example: '120', description: 'Panjang' })
  @IsOptional()
  @IsString()
  length?: string;
  @ApiPropertyOptional({ example: '60', description: 'Lebar' })
  @IsOptional()
  @IsString()
  width?: string;
  @ApiPropertyOptional({ example: '30', description: 'Tinggi' })
  @IsOptional()
  @IsString()
  height?: string;
  @ApiPropertyOptional({ example: '0.216', description: 'Volume' })
  @IsOptional()
  @IsString()
  volume?: string;
  @ApiPropertyOptional({ example: '1', description: 'Konversi Kg/Pcs' })
  @IsOptional()
  @IsString()
  conversionKgPcs?: string;
  @ApiPropertyOptional({ description: 'No. Ijin Edar' }) @IsOptional() @IsString() registrationNo?:
    | string
    | null;
  @ApiPropertyOptional({ default: true, description: 'Retur — dapat diretur' })
  @IsOptional()
  @IsBoolean()
  isReturnable?: boolean = true;
  @ApiPropertyOptional({ default: false, description: 'Mobile' })
  @IsOptional()
  @IsBoolean()
  isMobile?: boolean = false;

  // ─── Validity & flags (legacy parity) ─────────────────────────────
  @ApiPropertyOptional({ description: 'Kategori Umur (freetext)' })
  @IsOptional()
  @IsString()
  ageCategory?: string | null;

  @ApiPropertyOptional({ description: 'Berlaku s.d (ISO date YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  validUntil?: string | null;

  @ApiPropertyOptional({ default: true, description: 'BKP — Barang Kena Pajak (VATable)' })
  @IsOptional()
  @IsBoolean()
  isVatable?: boolean = true;

  @ApiPropertyOptional({ default: false, description: 'Spesial' })
  @IsOptional()
  @IsBoolean()
  isSpecial?: boolean = false;

  @ApiPropertyOptional({ default: true }) @IsOptional() @IsBoolean() isActive?: boolean = true;

  // ─── Lain-lain & Custom (legacy tabs) — persisted in md_items.metadata ────
  @ApiPropertyOptional({
    type: ItemOthersDto,
    description: 'Legacy "Lain-lain" tab (alias names + notes) → metadata.others',
  })
  @IsOptional()
  @ValidateNested()
  @Type(() => ItemOthersDto)
  others?: ItemOthersDto;

  @ApiPropertyOptional({
    type: ItemCustomDto,
    description: 'Legacy "Custom" tab (production/moulding attrs) → metadata.custom',
  })
  @IsOptional()
  @ValidateNested()
  @Type(() => ItemCustomDto)
  custom?: ItemCustomDto;
}

import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  ArrayMinSize,
  IsArray,
  IsBoolean,
  IsDateString,
  IsEnum,
  IsInt,
  IsNotEmpty,
  IsOptional,
  IsString,
  Min,
  ValidateNested,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

/** Full Senti approval state machine (§2.7) — mirrors DB ErpDocumentStatus. */
export enum ErpDocumentStatusDto {
  DRAFT = 'DRAFT',
  NEED_APPROVE = 'NEED_APPROVE',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
  POSTED = 'POSTED',
  VOID = 'VOID',
  CANCELLED = 'CANCELLED',
}

/** Harga termasuk / belum termasuk pajak — mirrors DB ErpPriceMode. */
export enum ErpPriceModeDto {
  TAX_INCLUSIVE = 'TAX_INCLUSIVE',
  TAX_EXCLUSIVE = 'TAX_EXCLUSIVE',
}

/** One item line of a return receipt (header + detail = master/detail document). */
export class SlsReturnReceiptLineDto {
  @ApiProperty({ example: '1001', description: 'Item (md_items) id' })
  @IsString()
  @IsNotEmpty()
  itemId!: string;

  @ApiProperty({ example: '10.0000', description: 'Quantity (transaction unit)' })
  @IsDecimalString()
  quantity!: string;

  @ApiProperty({ example: '5', description: 'Unit (md_units) id' })
  @IsString()
  @IsNotEmpty()
  unitId!: string;

  @ApiPropertyOptional({ example: '150000.0000', description: 'Unit price; defaults to 0' })
  @IsOptional()
  @IsDecimalString()
  unitPrice?: string;

  @ApiPropertyOptional({ example: '0.0000', description: 'Discount percent (line)' })
  @IsOptional()
  @IsDecimalString()
  discountPercent?: string;

  @ApiPropertyOptional({ example: '0.0000', description: 'Discount amount (line)' })
  @IsOptional()
  @IsDecimalString()
  discountAmount?: string;

  @ApiPropertyOptional({ description: 'Tax 1 (md_taxes) id' })
  @IsOptional()
  @IsString()
  tax1Id?: string;

  @ApiPropertyOptional({ example: '0.0000' })
  @IsOptional()
  @IsDecimalString()
  tax1Amount?: string;

  @ApiPropertyOptional({ description: 'Tax 2 (md_taxes) id' })
  @IsOptional()
  @IsString()
  tax2Id?: string;

  @ApiPropertyOptional({ example: '0.0000' })
  @IsOptional()
  @IsDecimalString()
  tax2Amount?: string;

  @ApiPropertyOptional({ description: 'Warehouse (md_warehouses) id — Gudang' })
  @IsOptional()
  @IsString()
  warehouseId?: string;

  @ApiPropertyOptional({ description: 'Inventory account (md_accounts) id' })
  @IsOptional()
  @IsString()
  inventoryAccountId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() divisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() subdivisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() projectId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ description: 'Custom line fields from Kustomisasi Grid (JSONB)' })
  @IsOptional()
  customFields?: Record<string, unknown>;

  @ApiProperty({ example: 1 })
  @IsInt()
  @Min(1)
  lineNo!: number;
}

export class CreateSlsReturnReceiptDto {
  @ApiPropertyOptional({
    description: 'Auto-generate docNumber via sys_document_numberings',
    default: true,
  })
  @IsOptional()
  @IsBoolean()
  auto?: boolean;

  @ApiPropertyOptional({
    description: 'Manual doc number; omit (or set auto=true) to server-generate',
  })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiProperty({ example: '2026-06-02', description: 'Tanggal dokumen (YYYY-MM-DD)' })
  @IsDateString()
  docDate!: string;

  @ApiPropertyOptional({
    description: 'Fiscal period id; derived from docDate when omitted',
  })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty({ example: '1', description: 'Branch (md_branches) id — Cabang' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiPropertyOptional({ description: 'Location (md_locations) id — Lokasi' })
  @IsOptional()
  @IsString()
  locationId?: string;

  @ApiPropertyOptional({ description: 'Warehouse (md_warehouses) id — Gudang' })
  @IsOptional()
  @IsString()
  warehouseId?: string;

  @ApiPropertyOptional({ description: 'Customer (md_partners) id — Pelanggan' })
  @IsOptional()
  @IsString()
  customerId?: string;

  @ApiPropertyOptional({ description: 'Payment term (md_payment_terms) id — Termin' })
  @IsOptional()
  @IsString()
  paymentTermId?: string;

  @ApiPropertyOptional({ example: '2026-07-02', description: 'Jatuh tempo (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dueDate?: string;

  @ApiProperty({ example: '1', description: 'Currency (md_currencies) id — Uang' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000' })
  @IsDecimalString()
  exchangeRate!: string;

  @ApiPropertyOptional({ enum: ErpPriceModeDto, default: ErpPriceModeDto.TAX_EXCLUSIVE })
  @IsOptional()
  @IsEnum(ErpPriceModeDto)
  priceMode?: ErpPriceModeDto;

  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() referenceNo?: string;

  @ApiPropertyOptional({ example: '2026-06-01', description: 'Tgl referensi (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  referenceDate?: string;

  @ApiPropertyOptional({ description: 'AR control account (md_accounts) id — Rek Piutang' })
  @IsOptional()
  @IsString()
  receivableAccountId?: string;

  @ApiPropertyOptional({ description: 'Sales dept (md_divisions) id — Bagian Penjualan' })
  @IsOptional()
  @IsString()
  salesDeptId?: string;

  @ApiPropertyOptional({ example: '0.0000', description: 'Header discount percent' })
  @IsOptional()
  @IsDecimalString()
  discountPercent?: string;

  @ApiPropertyOptional({ example: '0.0000', description: 'Header discount amount' })
  @IsOptional()
  @IsDecimalString()
  discountAmount?: string;

  @ApiPropertyOptional({ example: '0.0000', description: 'Header tax 1 amount' })
  @IsOptional()
  @IsDecimalString()
  tax1Amount?: string;

  @ApiPropertyOptional({ example: '0.0000', description: 'Header tax 2 amount' })
  @IsOptional()
  @IsDecimalString()
  tax2Amount?: string;

  @ApiPropertyOptional({ example: '0.0000', description: 'Other cost amount (Biaya Lain)' })
  @IsOptional()
  @IsDecimalString()
  otherCostAmount?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto, default: ErpDocumentStatusDto.DRAFT })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional({ description: 'Invoice (sls_invoices) id — Faktur Penjualan' })
  @IsOptional()
  @IsString()
  invoiceId?: string;

  @ApiPropertyOptional({ description: 'Return (sls_returns) id — Sales Return' })
  @IsOptional()
  @IsString()
  returnId?: string;

  @ApiPropertyOptional({ description: 'Settlement status' })
  @IsOptional()
  @IsString()
  settlementStatus?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiPropertyOptional({ description: 'Custom header fields from Form Builder (JSONB)' })
  @IsOptional()
  customFields?: Record<string, unknown>;

  @ApiProperty({ type: [SlsReturnReceiptLineDto] })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => SlsReturnReceiptLineDto)
  lines!: SlsReturnReceiptLineDto[];
}

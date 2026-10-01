import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  ArrayMinSize, IsArray, IsBoolean, IsDateString, IsEnum, IsIn,
  IsInt, IsNotEmpty, IsOptional, IsString, Min, ValidateNested,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

export enum ErpDocumentStatusDto {
  DRAFT = 'DRAFT', NEED_APPROVE = 'NEED_APPROVE', APPROVED = 'APPROVED',
  REJECTED = 'REJECTED', POSTED = 'POSTED', VOID = 'VOID', CANCELLED = 'CANCELLED',
}
export enum ErpPriceModeDto { TAX_INCLUSIVE = 'TAX_INCLUSIVE', TAX_EXCLUSIVE = 'TAX_EXCLUSIVE' }
export enum ErpQcStatusDto { PENDING = 'PENDING', PASSED = 'PASSED', FAILED = 'FAILED', PARTIAL = 'PARTIAL' }

/** One item line of a goods receipt (header + detail). */
export class PurGoodsReceiptLineDto {
  @ApiProperty({ example: '1001', description: 'Item (md_items) id' })
  @IsString() @IsNotEmpty() itemId!: string;

  @ApiProperty({ example: '10.0000' }) @IsDecimalString() quantity!: string;
  @ApiProperty({ example: '5', description: 'Unit (md_units) id' }) @IsString() @IsNotEmpty() unitId!: string;

  @ApiPropertyOptional({ example: '0.0000' }) @IsOptional() @IsDecimalString() unitPrice?: string;
  @ApiPropertyOptional({ description: 'Unit cost (COGS) for valuation' }) @IsOptional() @IsDecimalString() unitCost?: string;

  // QC delta — all have DB defaults (0 / PENDING) so optional in DTO.
  @ApiPropertyOptional({ example: '10.0000', description: 'Qty accepted into stock' })
  @IsOptional() @IsDecimalString() acceptedQty?: string;
  @ApiPropertyOptional({ example: '0.0000', description: 'Qty rejected (candidate return)' })
  @IsOptional() @IsDecimalString() rejectedQty?: string;
  @ApiPropertyOptional({ example: '0.0000', description: 'Qty in quarantine (unclassified)' })
  @IsOptional() @IsDecimalString() quarantineQty?: string;
  @ApiPropertyOptional({ enum: ErpQcStatusDto, default: ErpQcStatusDto.PENDING })
  @IsOptional() @IsIn(Object.values(ErpQcStatusDto)) qcStatus?: string;

  @ApiPropertyOptional() @IsOptional() @IsDecimalString() discountPercent?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() discountAmount?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() tax1Id?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() tax1Amount?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() tax2Id?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() tax2Amount?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() warehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() inventoryAccountId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() accruedPayableAccountId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() divisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() subdivisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() projectId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() orderLineId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1 }) @IsInt() @Min(1) lineNo!: number;
}

export class CreatePurGoodsReceiptDto {
  @ApiPropertyOptional({ default: true }) @IsOptional() @IsBoolean() auto?: boolean;
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;
  @ApiProperty({ example: '2026-06-02' }) @IsDateString() docDate!: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;
  @ApiProperty({ example: '1' }) @IsString() @IsNotEmpty() branchId!: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() warehouseId?: string;
  @ApiPropertyOptional({ description: 'Supplier (md_partners) id' }) @IsOptional() @IsString() supplierId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() paymentTermId?: string;
  @ApiPropertyOptional() @IsOptional() @IsDateString() dueDate?: string;
  @ApiProperty({ example: '1' }) @IsString() @IsNotEmpty() currencyId!: string;
  @ApiProperty({ example: '1.000000' }) @IsDecimalString() exchangeRate!: string;
  @ApiPropertyOptional({ enum: ErpPriceModeDto, default: ErpPriceModeDto.TAX_EXCLUSIVE })
  @IsOptional() @IsEnum(ErpPriceModeDto) priceMode?: ErpPriceModeDto;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() referenceNo?: string;
  @ApiPropertyOptional() @IsOptional() @IsDateString() referenceDate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() payableAccountId?: string;
  // Upstream chain link — GRN is typically created from an approved PO.
  @ApiPropertyOptional({ description: 'Source purchase order (pur_orders) id' })
  @IsOptional() @IsString() orderId?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() discountPercent?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() discountAmount?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() tax1Amount?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() tax2Amount?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() otherCostAmount?: string;
  @ApiPropertyOptional({ enum: ErpDocumentStatusDto, default: ErpDocumentStatusDto.DRAFT })
  @IsOptional() @IsEnum(ErpDocumentStatusDto) status?: ErpDocumentStatusDto;
  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiProperty({ type: [PurGoodsReceiptLineDto] })
  @IsArray() @ArrayMinSize(0) @ValidateNested({ each: true }) @Type(() => PurGoodsReceiptLineDto)
  lines!: PurGoodsReceiptLineDto[];
}

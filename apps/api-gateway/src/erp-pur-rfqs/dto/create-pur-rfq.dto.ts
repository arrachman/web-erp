import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  ArrayMinSize, IsArray, IsBoolean, IsDateString, IsEnum,
  IsInt, IsNotEmpty, IsOptional, IsString, Min, ValidateNested,
} from 'class-validator';

export enum ErpDocumentStatusDto {
  DRAFT = 'DRAFT', NEED_APPROVE = 'NEED_APPROVE', APPROVED = 'APPROVED',
  REJECTED = 'REJECTED', POSTED = 'POSTED', VOID = 'VOID', CANCELLED = 'CANCELLED',
}

/**
 * RFQ "lines" = invited suppliers (pur_rfq_suppliers), NOT item rows.
 * Items are inherited from the linked requisitionId.
 */
export class PurRfqSupplierLineDto {
  @ApiProperty({ example: '100', description: 'Supplier (md_partners) id' })
  @IsString() @IsNotEmpty() supplierId!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1 }) @IsInt() @Min(1) lineNo!: number;
}

export class CreatePurRfqDto {
  @ApiPropertyOptional({ default: true }) @IsOptional() @IsBoolean() auto?: boolean;
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;
  @ApiProperty({ example: '2026-06-02' }) @IsDateString() docDate!: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;
  @ApiProperty({ example: '1' }) @IsString() @IsNotEmpty() branchId!: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() warehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() currencyId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() exchangeRate?: string;
  @ApiPropertyOptional({ description: 'Source requisition (pur_requisitions) id' }) @IsOptional() @IsString() requisitionId?: string;
  @ApiPropertyOptional({ example: '2026-06-15', description: 'Batas akhir penawaran' }) @IsOptional() @IsDateString() validFrom?: string;
  @ApiPropertyOptional() @IsOptional() @IsDateString() validTo?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() referenceNo?: string;
  @ApiPropertyOptional({ enum: ErpDocumentStatusDto, default: ErpDocumentStatusDto.DRAFT })
  @IsOptional() @IsEnum(ErpDocumentStatusDto) status?: ErpDocumentStatusDto;
  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  /** Invited supplier lines. */
  @ApiProperty({ type: [PurRfqSupplierLineDto] })
  @IsArray() @ArrayMinSize(0) @ValidateNested({ each: true }) @Type(() => PurRfqSupplierLineDto)
  suppliers!: PurRfqSupplierLineDto[];
}

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

/** Senti 7-state document status (§2.7). */
export enum ErpDocumentStatusDto {
  DRAFT = 'DRAFT',
  NEED_APPROVE = 'NEED_APPROVE',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
  POSTED = 'POSTED',
  VOID = 'VOID',
  CANCELLED = 'CANCELLED',
}

/** One item line of an opening stock → inv_opening_stock_lines. */
export class InvOpeningStockLineDto {
  @ApiProperty() @IsNotEmpty() @IsString() itemId!: string;
  @ApiProperty({ example: '1' }) @IsDecimalString() quantity!: string;
  @ApiProperty() @IsNotEmpty() @IsString() unitId!: string;
  @ApiProperty({ example: '25000' }) @IsDecimalString() unitCost!: string;

  @ApiPropertyOptional({ description: 'Gudang baris — fallback ke header bila kosong' })
  @IsOptional()
  @IsString()
  warehouseId?: string;

  @ApiPropertyOptional({ description: 'Akun persediaan — di-resolve server-side bila kosong' })
  @IsOptional()
  @IsString()
  inventoryAccountId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() divisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() subdivisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() projectId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1 }) @IsInt() @Min(1) lineNo!: number;
}

/** Create payload for an opening stock (header + item lines). */
export class CreateInvOpeningStockDto {
  @ApiPropertyOptional({ default: true }) @IsOptional() @IsBoolean() auto?: boolean;
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiProperty({ description: 'YYYY-MM-DD' }) @IsDateString() openingDate!: string;
  @ApiPropertyOptional({ description: 'Derived from openingDate if omitted' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty() @IsNotEmpty() @IsString() branchId!: string;
  @ApiProperty() @IsNotEmpty() @IsString() warehouseId!: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional({ description: 'Klasifikasi opening (kind)' }) @IsOptional() @IsString() kind?: string;

  @ApiProperty() @IsNotEmpty() @IsString() currencyId!: string;
  @ApiProperty({ example: '1' }) @IsDecimalString() exchangeRate!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiProperty({ type: [InvOpeningStockLineDto] })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => InvOpeningStockLineDto)
  lines!: InvOpeningStockLineDto[];
}

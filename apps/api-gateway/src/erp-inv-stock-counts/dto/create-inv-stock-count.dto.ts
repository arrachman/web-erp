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

/** Stock count type discriminator — selects the opname mode. */
export enum ErpStockCountTypeDto {
  FULL = 'FULL', // Full physical count
  CYCLE = 'CYCLE', // Cycle count
  SPOT = 'SPOT', // Spot / ad-hoc count
}

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

/** One item line of a stock count → inv_stock_count_lines. */
export class InvStockCountLineDto {
  @ApiProperty() @IsNotEmpty() @IsString() itemId!: string;
  @ApiProperty() @IsNotEmpty() @IsString() unitId!: string;

  @ApiPropertyOptional({ description: 'Gudang per baris (fallback ke header)' })
  @IsOptional()
  @IsString()
  warehouseId?: string;

  @ApiPropertyOptional({ example: '0', description: 'Qty sistem (default 0)' })
  @IsOptional()
  @IsDecimalString()
  systemQty?: string;

  @ApiProperty({ example: '1', description: 'Qty fisik hasil opname' })
  @IsDecimalString()
  physicalQty!: string;

  @ApiPropertyOptional({ description: 'Qty bagus (default = physicalQty)' })
  @IsOptional()
  @IsDecimalString()
  goodQty?: string;

  @ApiPropertyOptional({ description: 'Qty rusak (default 0)' })
  @IsOptional()
  @IsDecimalString()
  damagedQty?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() divisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() subdivisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() projectId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1 }) @IsInt() @Min(1) lineNo!: number;
}

/** Create payload for a stock count (header + item lines). */
export class CreateInvStockCountDto {
  @ApiPropertyOptional({ default: true }) @IsOptional() @IsBoolean() auto?: boolean;
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiProperty({ description: 'YYYY-MM-DD' }) @IsDateString() countDate!: string;
  @ApiPropertyOptional({ description: 'Derived from countDate if omitted' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty() @IsNotEmpty() @IsString() branchId!: string;
  @ApiProperty() @IsNotEmpty() @IsString() warehouseId!: string;

  @ApiPropertyOptional({ enum: ErpStockCountTypeDto, default: ErpStockCountTypeDto.FULL })
  @IsOptional()
  @IsEnum(ErpStockCountTypeDto)
  countType?: ErpStockCountTypeDto;

  @ApiPropertyOptional() @IsOptional() @IsInt() @Min(0) stepNo?: number;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiProperty({ type: [InvStockCountLineDto] })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => InvStockCountLineDto)
  lines!: InvStockCountLineDto[];
}

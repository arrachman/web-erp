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

/** Direction of a stock adjustment line (INCREASE = stock up, DECREASE = stock down). */
export enum ErpAdjustmentDirectionDto {
  INCREASE = 'INCREASE',
  DECREASE = 'DECREASE',
}

/** One item line of a stock adjustment → inv_stock_adjustment_lines. */
export class InvStockAdjustmentLineDto {
  @ApiProperty() @IsNotEmpty() @IsString() itemId!: string;

  @ApiProperty({ enum: ErpAdjustmentDirectionDto })
  @IsEnum(ErpAdjustmentDirectionDto)
  direction!: ErpAdjustmentDirectionDto;

  @ApiProperty({ example: '1' }) @IsDecimalString() quantity!: string;
  @ApiProperty() @IsNotEmpty() @IsString() unitId!: string;

  @ApiPropertyOptional() @IsOptional() @IsDecimalString() unitCost?: string;
  @ApiPropertyOptional({ description: 'Falls back to header warehouseId if omitted' })
  @IsOptional()
  @IsString()
  warehouseId?: string;
  @ApiPropertyOptional({ description: 'Akun persediaan — fallback ke master item / Setting inventory' })
  @IsOptional()
  @IsString()
  inventoryAccountId?: string;
  @ApiPropertyOptional({ description: 'Akun lawan penyesuaian — fallback ke Setting inventory' })
  @IsOptional()
  @IsString()
  contraAccountId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() costCenterId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() divisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() subdivisionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() projectId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1 }) @IsInt() @Min(1) lineNo!: number;
}

/** Create payload for a stock adjustment (header + item lines). */
export class CreateInvStockAdjustmentDto {
  @ApiPropertyOptional({ default: true }) @IsOptional() @IsBoolean() auto?: boolean;
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiProperty({ description: 'YYYY-MM-DD' }) @IsDateString() adjustmentDate!: string;
  @ApiPropertyOptional({ description: 'Derived from adjustmentDate if omitted' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty() @IsNotEmpty() @IsString() branchId!: string;
  @ApiProperty() @IsNotEmpty() @IsString() warehouseId!: string;

  @ApiPropertyOptional({ description: 'Jenis penyesuaian (free text)' })
  @IsOptional()
  @IsString()
  kind?: string;
  @ApiPropertyOptional({ description: 'Stock opname terkait (stock_count_id)' })
  @IsOptional()
  @IsString()
  stockCountId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiProperty({ type: [InvStockAdjustmentLineDto] })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => InvStockAdjustmentLineDto)
  lines!: InvStockAdjustmentLineDto[];
}

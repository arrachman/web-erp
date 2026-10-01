import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsDateString, IsEnum, IsOptional, IsString } from 'class-validator';

export enum ErpCostingMethodDto {
  AVG = 'AVG',
  FIFO = 'FIFO',
  STD = 'STD',
}

/** Trigger payload for a price-adjustment (cost-recalculation) process. */
export class CreateInvPriceAdjustmentDto {
  @ApiPropertyOptional({ description: 'Doc number (PA-XXXXXX). Auto-generated if omitted.' })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiProperty({ description: 'From date of recalc scope (YYYY-MM-DD)' })
  @IsDateString()
  fromDate!: string;

  @ApiPropertyOptional({ description: 'To date of recalc scope (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  toDate?: string;

  @ApiPropertyOptional({ description: 'Derived from fromDate if omitted' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiPropertyOptional({ description: 'Scope: specific warehouse id' })
  @IsOptional()
  @IsString()
  warehouseId?: string;

  @ApiPropertyOptional({ description: 'Scope: specific item id' })
  @IsOptional()
  @IsString()
  itemId?: string;

  @ApiPropertyOptional({ enum: ErpCostingMethodDto, default: 'AVG' })
  @IsOptional()
  @IsEnum(ErpCostingMethodDto)
  costingMethod?: ErpCostingMethodDto = ErpCostingMethodDto.AVG;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  notes?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  legacyCode?: string;
}

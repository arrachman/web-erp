import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsDateString, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpDocumentStatusDto } from './create-inv-stock-adjustment.dto';

export const INV_ADJUSTMENT_SORTABLE = [
  'docNumber',
  'adjustmentDate',
  'status',
  'createdAt',
] as const;

export class QueryInvStockAdjustmentsDto {
  @ApiPropertyOptional({ default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ default: 10 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(100)
  limit?: number = 10;

  @ApiPropertyOptional({ description: 'docNumber / description' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: INV_ADJUSTMENT_SORTABLE, default: 'adjustmentDate' })
  @IsOptional()
  @IsIn(INV_ADJUSTMENT_SORTABLE as unknown as string[])
  sortBy?: string = 'adjustmentDate';

  @ApiPropertyOptional({ enum: ['asc', 'desc'], default: 'desc' })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc' = 'desc';

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional({ description: 'Gudang' }) @IsOptional() @IsString() warehouseId?: string;

  @ApiPropertyOptional({ description: 'adjustmentDate >= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateFrom?: string;

  @ApiPropertyOptional({ description: 'adjustmentDate <= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateTo?: string;

  @ApiPropertyOptional({ description: 'User input (createdById)' })
  @IsOptional()
  @IsString()
  createdById?: string;
}

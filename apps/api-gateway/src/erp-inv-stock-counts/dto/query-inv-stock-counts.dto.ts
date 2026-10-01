import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsDateString, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpDocumentStatusDto, ErpStockCountTypeDto } from './create-inv-stock-count.dto';

export const INV_COUNT_SORTABLE = [
  'docNumber',
  'countDate',
  'status',
  'createdAt',
] as const;

export class QueryInvStockCountsDto {
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

  @ApiPropertyOptional({ enum: INV_COUNT_SORTABLE, default: 'countDate' })
  @IsOptional()
  @IsIn(INV_COUNT_SORTABLE as unknown as string[])
  sortBy?: string = 'countDate';

  @ApiPropertyOptional({ enum: ['asc', 'desc'], default: 'desc' })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc' = 'desc';

  @ApiPropertyOptional({ enum: ErpStockCountTypeDto, description: 'Filter per mode opname (FULL/CYCLE/SPOT)' })
  @IsOptional()
  @IsEnum(ErpStockCountTypeDto)
  countType?: ErpStockCountTypeDto;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional({ description: 'Gudang opname' }) @IsOptional() @IsString() warehouseId?: string;

  @ApiPropertyOptional({ description: 'countDate >= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateFrom?: string;

  @ApiPropertyOptional({ description: 'countDate <= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateTo?: string;

  @ApiPropertyOptional({ description: 'No Dokumen >= (range start)' })
  @IsOptional()
  @IsString()
  docNumberFrom?: string;

  @ApiPropertyOptional({ description: 'No Dokumen <= (range end)' })
  @IsOptional()
  @IsString()
  docNumberTo?: string;

  @ApiPropertyOptional({ description: 'Uraian (description contains)' })
  @IsOptional()
  @IsString()
  description?: string;

  @ApiPropertyOptional({ description: 'User input (createdById)' })
  @IsOptional()
  @IsString()
  createdById?: string;
}

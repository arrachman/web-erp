import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsDateString, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpDocumentStatusDto } from './create-inv-daily-check.dto';

export const INV_DAILY_CHECK_SORTABLE = [
  'docNumber',
  'checkDate',
  'status',
  'createdAt',
] as const;

export class QueryInvDailyChecksDto {
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

  @ApiPropertyOptional({ enum: INV_DAILY_CHECK_SORTABLE, default: 'checkDate' })
  @IsOptional()
  @IsIn(INV_DAILY_CHECK_SORTABLE as unknown as string[])
  sortBy?: string = 'checkDate';

  @ApiPropertyOptional({ enum: ['asc', 'desc'], default: 'desc' })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc' = 'desc';

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;

  @ApiPropertyOptional({ description: 'checkDate >= (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dateFrom?: string;

  @ApiPropertyOptional({ description: 'checkDate <= (YYYY-MM-DD)' })
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

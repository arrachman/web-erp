import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import {
  ErpDocumentStatusDto,
  ErpGiroEntryKindDto,
  ErpGiroTypeDto,
} from './create-giro-entry.dto';

const SORTABLE = ['entryDate', 'docNumber', 'status', 'createdAt', 'updatedAt'] as const;

export class QueryGiroEntryDto {
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

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: ErpGiroEntryKindDto })
  @IsOptional()
  @IsEnum(ErpGiroEntryKindDto)
  kind?: ErpGiroEntryKindDto;

  @ApiPropertyOptional({ enum: ErpGiroTypeDto })
  @IsOptional()
  @IsEnum(ErpGiroTypeDto)
  type?: ErpGiroTypeDto;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() partnerId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() dateFrom?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() dateTo?: string;

  @ApiPropertyOptional({ enum: SORTABLE, default: 'entryDate' })
  @IsOptional()
  @IsIn(SORTABLE as unknown as string[])
  sortBy?: string;

  @ApiPropertyOptional({ enum: ['asc', 'desc'], default: 'desc' })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc';
}

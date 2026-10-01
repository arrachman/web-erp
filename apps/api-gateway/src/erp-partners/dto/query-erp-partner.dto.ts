import { ApiPropertyOptional } from '@nestjs/swagger';
import { Transform, Type } from 'class-transformer';
import { IsBoolean, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpPartnerTypeKind } from '@prisma/client';

const SORTABLE_FIELDS = [
  'code',
  'name',
  'isActive',
  'createdAt',
] as const;
type SortableField = (typeof SORTABLE_FIELDS)[number];

export class QueryErpPartnerDto {
  @ApiPropertyOptional({ example: 1, default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ example: 10, default: 10 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(100)
  limit?: number = 10;

  @ApiPropertyOptional({ example: 'maju' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ example: '1', description: 'Filter by category ID' })
  @IsOptional()
  @IsString()
  categoryId?: string;

  @ApiPropertyOptional({ example: '1', description: 'Filter by partner type ID' })
  @IsOptional()
  @IsString()
  partnerTypeId?: string;

  @ApiPropertyOptional({ enum: ErpPartnerTypeKind, description: 'Filter by partner type kind (peran)' })
  @IsOptional()
  @IsEnum(ErpPartnerTypeKind)
  typeKind?: ErpPartnerTypeKind;

  @ApiPropertyOptional({ example: true })
  @IsOptional()
  @Transform(({ value }) => {
    if (value === 'true') return true;
    if (value === 'false') return false;
    return value;
  })
  @IsBoolean()
  isActive?: boolean;

  @ApiPropertyOptional({ example: 'name', enum: SORTABLE_FIELDS })
  @IsOptional()
  @IsIn(SORTABLE_FIELDS)
  sortBy?: SortableField;

  @ApiPropertyOptional({ example: 'asc', enum: ['asc', 'desc'] })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc';
}

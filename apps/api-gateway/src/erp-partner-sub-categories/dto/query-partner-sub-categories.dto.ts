import { ApiPropertyOptional } from '@nestjs/swagger';
import { Transform, Type } from 'class-transformer';
import { IsBoolean, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';

const SORTABLE_FIELDS = ["code","name","isActive","createdAt","type"] as const;
type SortableField = (typeof SORTABLE_FIELDS)[number];

const PSC_TYPES = ['CUSTOMER', 'SUPPLIER', 'SALESMAN'] as const;
export type ErpPartnerSubCategoryTypeFilter = (typeof PSC_TYPES)[number];

export class QueryErpPartnerSubCategoryDto {
  @ApiPropertyOptional({ default: 1 }) @IsOptional() @Type(() => Number) @IsInt() @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ default: 10 }) @IsOptional() @Type(() => Number) @IsInt() @Min(1) @Max(100)
  limit?: number = 10;

  @ApiPropertyOptional() @IsOptional() @IsString()
  search?: string;

  @ApiPropertyOptional() @IsOptional()
  @Transform(({ value }) => (value === 'true' ? true : value === 'false' ? false : value))
  @IsBoolean()
  isActive?: boolean;

  @ApiPropertyOptional({ enum: PSC_TYPES }) @IsOptional() @IsIn(PSC_TYPES)
  type?: ErpPartnerSubCategoryTypeFilter;

  @ApiPropertyOptional({ enum: SORTABLE_FIELDS }) @IsOptional() @IsIn(SORTABLE_FIELDS)
  sortBy?: SortableField;

  @ApiPropertyOptional({ enum: ['asc', 'desc'] }) @IsOptional() @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc';
}

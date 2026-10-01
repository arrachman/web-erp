import { ApiPropertyOptional } from '@nestjs/swagger';
import { Transform, Type } from 'class-transformer';
import { IsBoolean, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpAccountType, ErpAccountKind, ErpNormalBalance } from '@prisma/client';

const SORTABLE_FIELDS = ['code', 'name', 'type', 'kind', 'level', 'isActive', 'createdAt'] as const;
type SortableField = (typeof SORTABLE_FIELDS)[number];

export class QueryErpAccountDto {
  @ApiPropertyOptional({ example: 1, default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number = 1;

  // Prefer GET /accounts/tree for hierarchy. List max 500 for search/export only.
  @ApiPropertyOptional({ example: 10, default: 10, maximum: 500 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(500)
  limit?: number = 10;

  @ApiPropertyOptional({ example: 'Cash' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: ErpAccountType })
  @IsOptional()
  @IsEnum(ErpAccountType)
  accountType?: ErpAccountType;

  @ApiPropertyOptional({ enum: ErpAccountKind })
  @IsOptional()
  @IsEnum(ErpAccountKind)
  accountKind?: ErpAccountKind;

  @ApiPropertyOptional({ enum: ErpNormalBalance })
  @IsOptional()
  @IsEnum(ErpNormalBalance)
  normalBalance?: ErpNormalBalance;

  @ApiPropertyOptional({ example: '1', description: 'Filter by parent account ID' })
  @IsOptional()
  @IsString()
  parentId?: string;

  @ApiPropertyOptional({ example: true })
  @IsOptional()
  @Transform(({ value }) => {
    if (value === 'true') return true;
    if (value === 'false') return false;
    return value;
  })
  @IsBoolean()
  isActive?: boolean;

  @ApiPropertyOptional({ example: 'code', enum: SORTABLE_FIELDS })
  @IsOptional()
  @IsIn(SORTABLE_FIELDS)
  sortBy?: SortableField;

  @ApiPropertyOptional({ example: 'asc', enum: ['asc', 'desc'] })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc';
}


/** Children of one parent for lazy CoA tree (no pagination — one level only). */
export class QueryAccountTreeDto {
  @ApiPropertyOptional({
    description: "Parent id, or omit/'null' for roots",
  })
  @IsOptional()
  @IsString()
  parentId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsEnum(ErpAccountType)
  accountType?: ErpAccountType;

  @ApiPropertyOptional()
  @IsOptional()
  @IsEnum(ErpAccountKind)
  accountKind?: ErpAccountKind;

  @ApiPropertyOptional()
  @IsOptional()
  @Transform(({ value }) => {
    if (value === 'true') return true;
    if (value === 'false') return false;
    return value;
  })
  @IsBoolean()
  isActive?: boolean;
}

import { ApiPropertyOptional } from '@nestjs/swagger';
import { Transform, Type } from 'class-transformer';
import { IsBoolean, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpItemType } from '@prisma/client';

const SORTABLE_FIELDS = ['code', 'name', 'type', 'costMethod', 'salePrice', 'isActive', 'createdAt', 'updatedAt'] as const;
type SortableField = (typeof SORTABLE_FIELDS)[number];

export class QueryErpItemDto {
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

  @ApiPropertyOptional({ example: 'steel', description: 'Search by code, name, or barcode' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: ErpItemType, description: 'Filter by item type' })
  @IsOptional()
  @IsEnum(ErpItemType)
  itemType?: ErpItemType;

  @ApiPropertyOptional({ example: '1', description: 'Filter by category ID (BigInt as string)' })
  @IsOptional()
  @IsString()
  categoryId?: string;

  @ApiPropertyOptional({ example: '1', description: 'Filter by unit ID (BigInt as string)' })
  @IsOptional()
  @IsString()
  unitId?: string;

  @ApiPropertyOptional({ description: 'Filter by item kind (Tipe)' })
  @IsOptional()
  @IsString()
  kindId?: string;

  @ApiPropertyOptional({ description: 'Filter by product class (Kelas Produk)' })
  @IsOptional()
  @IsString()
  productClassId?: string;

  @ApiPropertyOptional({ description: 'Filter by branch' })
  @IsOptional()
  @IsString()
  branchId?: string;

  @ApiPropertyOptional({ description: 'Filter by default warehouse' })
  @IsOptional()
  @IsString()
  defaultWarehouseId?: string;

  @ApiPropertyOptional({ example: true, description: 'Filter by active status' })
  @IsOptional()
  @Transform(({ value }: { value: unknown }) => {
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

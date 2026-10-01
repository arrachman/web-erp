import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { ErpMenuType } from '@prisma/client';
import {
  IsBoolean,
  IsEnum,
  IsInt,
  IsOptional,
  IsString,
  MaxLength,
  Min,
} from 'class-validator';

export class CreateErpSysMenuDto {
  @ApiProperty({ example: 'INVENTORY_ITEMS', description: 'Unique menu code' })
  @IsString()
  @MaxLength(100)
  code!: string;

  @ApiProperty({ example: 'Inventory Items', description: 'Menu display label' })
  @IsString()
  @MaxLength(200)
  title!: string;

  @ApiPropertyOptional({ example: '/erp/inventory/items', description: 'Navigation path' })
  @IsOptional()
  @IsString()
  @MaxLength(300)
  path?: string;

  @ApiPropertyOptional({ example: 'PackageIcon', description: 'Icon identifier' })
  @IsOptional()
  @IsString()
  @MaxLength(100)
  icon?: string;

  @ApiProperty({ enum: ErpMenuType, example: ErpMenuType.ITEM })
  @IsEnum(ErpMenuType)
  type!: ErpMenuType;

  @ApiPropertyOptional({ example: '1', description: 'Parent menu ID (string, nullable)' })
  @IsOptional()
  @IsString()
  parentId?: string | null;

  @ApiProperty({ example: 10, description: 'Sort order position' })
  @IsInt()
  @Min(0)
  sortOrder!: number;

  @ApiProperty({ example: true })
  @IsBoolean()
  isActive!: boolean;
}

import { ApiPropertyOptional } from '@nestjs/swagger';
import { ErpMenuType } from '@prisma/client';
import { Transform } from 'class-transformer';
import { IsBoolean, IsEnum, IsOptional, IsString, MaxLength } from 'class-validator';

export class QueryErpSysMenuDto {
  @ApiPropertyOptional({ enum: ErpMenuType, description: 'Filter by menu type' })
  @IsOptional()
  @IsEnum(ErpMenuType)
  type?: ErpMenuType;

  @ApiPropertyOptional({ example: '1', description: 'Filter by parent ID (use "null" for root items)' })
  @IsOptional()
  @IsString()
  @MaxLength(50)
  parentId?: string;

  @ApiPropertyOptional({ example: true, description: 'Filter by active status' })
  @IsOptional()
  @Transform(({ value }) => value === 'true' || value === true)
  @IsBoolean()
  isActive?: boolean;
}

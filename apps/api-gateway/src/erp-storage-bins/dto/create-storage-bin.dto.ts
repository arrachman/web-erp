import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsIn, IsOptional, IsString, MaxLength } from 'class-validator';

export const BIN_TYPES = ['ZONE', 'RACK', 'BIN'] as const;
export type BinType = (typeof BIN_TYPES)[number];

export class CreateErpStorageBinDto {
  @ApiProperty({ example: 'A1-01' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Rak A1-01' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiProperty({ example: '1', description: 'Warehouse id (wajib)' })
  @IsString()
  warehouseId!: string;

  @ApiPropertyOptional({ example: '1', description: 'Parent bin id (zona/rak induk, segudang)' })
  @IsOptional()
  @IsString()
  parentId?: string;

  @ApiPropertyOptional({ enum: BIN_TYPES, default: 'BIN' })
  @IsOptional()
  @IsIn(BIN_TYPES)
  binType?: BinType;

  @ApiPropertyOptional({ example: 'Dekat pintu masuk' })
  @IsOptional()
  @IsString()
  @MaxLength(500)
  notes?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

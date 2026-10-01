import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpWarehouseDto {
  @ApiProperty({ example: 'WH-001', description: 'Unique warehouse code' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Gudang Bahan Baku A' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiProperty({ example: '2', description: 'Location ID as numeric string (BigInt) — required' })
  @IsString()
  locationId!: string;

  @ApiPropertyOptional({ example: false, default: false })
  @IsOptional()
  @IsBoolean()
  allowNegativeStock?: boolean = false;

  @ApiPropertyOptional({ example: 'Gudang utama bahan baku produksi' })
  @IsOptional()
  @IsString()
  notes?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

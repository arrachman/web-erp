import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpLocationDto {
  @ApiProperty({ example: 'LOC-001', description: 'Unique location code' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Gudang Utara' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiProperty({ example: '1', description: 'Branch ID as numeric string (BigInt)' })
  @IsString()
  branchId!: string;

  @ApiPropertyOptional({ example: 'Jl. Industri No. 10' })
  @IsOptional()
  @IsString()
  @MaxLength(255)
  addressLine1?: string;

  @ApiPropertyOptional({ example: 'Bekasi' })
  @IsOptional()
  @IsString()
  @MaxLength(100)
  city?: string;

  @ApiPropertyOptional({ example: '17141' })
  @IsOptional()
  @IsString()
  @MaxLength(20)
  postalCode?: string;

  @ApiPropertyOptional({ example: '021-8881234' })
  @IsOptional()
  @IsString()
  @MaxLength(50)
  phone?: string;

  @ApiPropertyOptional({ example: 'Lokasi penyimpanan bahan baku' })
  @IsOptional()
  @IsString()
  notes?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

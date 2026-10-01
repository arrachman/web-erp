import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsNotEmpty, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpCityDto {
  @ApiProperty({ example: 'KAB-SIMEULUE', description: 'Kode unik human-readable (slug dari nama)' })
  @IsString()
  @MaxLength(100)
  code!: string;

  @ApiProperty({ example: 'Kab. Simeulue' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiPropertyOptional({ example: '1101', description: 'Kode BPS 4-digit kab/kota' })
  @IsOptional()
  @IsString()
  @MaxLength(10)
  bpsCode?: string;

  @ApiProperty({ example: '1', description: 'Province id (required)' })
  @IsString()
  @IsNotEmpty()
  provinceId!: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

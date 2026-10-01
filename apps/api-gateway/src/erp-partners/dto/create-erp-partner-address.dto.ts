import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsEnum, IsOptional, IsString, MaxLength } from 'class-validator';
import { ErpAddressType } from '@prisma/client';

export class CreateErpPartnerAddressDto {
  @ApiProperty({ enum: ErpAddressType, example: ErpAddressType.BILLING })
  @IsEnum(ErpAddressType)
  type!: ErpAddressType;

  @ApiProperty({ example: 'Jl. Sudirman No. 1' })
  @IsString()
  @MaxLength(255)
  addressLine1!: string;

  @ApiPropertyOptional({ example: 'Gedung A Lt. 3' })
  @IsOptional()
  @IsString()
  @MaxLength(255)
  addressLine2?: string;

  @ApiPropertyOptional({ description: 'Country id (md_countries)' })
  @IsOptional()
  @IsString()
  countryId?: string;

  @ApiPropertyOptional({ description: 'Province id (md_provinces)' })
  @IsOptional()
  @IsString()
  provinceId?: string;

  @ApiPropertyOptional({ description: 'City id (md_cities)' })
  @IsOptional()
  @IsString()
  cityId?: string;

  @ApiPropertyOptional({ description: 'Area / kecamatan id (md_areas)' })
  @IsOptional()
  @IsString()
  areaId?: string;

  @ApiPropertyOptional({ description: 'Sub-area / kelurahan id (md_sub_areas)' })
  @IsOptional()
  @IsString()
  subAreaId?: string;

  @ApiPropertyOptional({ example: '10220' })
  @IsOptional()
  @IsString()
  @MaxLength(20)
  postalCode?: string;

  @ApiPropertyOptional({ example: '021-5551234' })
  @IsOptional()
  @IsString()
  @MaxLength(50)
  phone?: string;

  @ApiPropertyOptional({ example: '021-5554321' })
  @IsOptional()
  @IsString()
  @MaxLength(50)
  fax?: string;

  @ApiPropertyOptional({ example: 'info@example.com' })
  @IsOptional()
  @IsString()
  @MaxLength(255)
  email?: string;

  @ApiPropertyOptional({ example: 'https://www.example.com' })
  @IsOptional()
  @IsString()
  @MaxLength(255)
  website?: string;

  @ApiPropertyOptional({ example: false, default: false })
  @IsOptional()
  @IsBoolean()
  isDefault?: boolean = false;
}

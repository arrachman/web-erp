import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsEnum, IsInt, IsOptional, IsString, Max, MaxLength, Min } from 'class-validator';
import { Type } from 'class-transformer';
import { ErpPartnerCategoryKind } from '@prisma/client';

export class CreateErpPartnerCategoryDto {
  @ApiProperty({ example: 'CUST-RETAIL', description: 'Unique category code' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Retail Customer' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiProperty({ enum: ErpPartnerCategoryKind, example: ErpPartnerCategoryKind.CUSTOMER })
  @IsEnum(ErpPartnerCategoryKind)
  kind!: ErpPartnerCategoryKind;

  @ApiPropertyOptional({
    example: 1,
    minimum: 1,
    maximum: 10,
    description: 'Tingkat Harga/Diskon Jual (1–10) yang dipakai pelanggan kategori ini',
  })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(10)
  salesTier?: number;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

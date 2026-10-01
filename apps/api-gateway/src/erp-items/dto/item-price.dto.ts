import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsInt, IsOptional, IsString, Max, Min } from 'class-validator';

/** One sale price tier (legacy "Harga Jual N" + "Diskon Jual N"). */
export class ItemPriceDto {
  @ApiProperty({ example: 1, description: 'Price tier level 1..10' })
  @IsInt()
  @Min(1)
  @Max(10)
  level!: number;

  @ApiPropertyOptional({ example: '75000', description: 'Tier sale price (Decimal as string)' })
  @IsOptional()
  @IsString()
  price?: string;

  @ApiPropertyOptional({ example: '5', description: 'Tier sale discount percent (Decimal as string)' })
  @IsOptional()
  @IsString()
  discountPercent?: string;
}

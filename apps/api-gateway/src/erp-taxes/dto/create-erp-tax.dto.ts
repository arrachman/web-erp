import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsNotEmpty, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpTaxDto {
  @ApiProperty({ example: 'VAT11', description: 'Unique tax code' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'VAT 11%' })
  @IsString()
  @MaxLength(120)
  name!: string;

  @ApiProperty({ example: '11.00', description: 'Tax rate as Decimal (e.g. 11.00 for 11%)' })
  @IsNotEmpty()
  @IsString()
  rate!: string;

  @ApiPropertyOptional({ example: '1', description: 'Sale account ID (string BigInt)' })
  @IsOptional()
  @IsString()
  saleAccountId?: string | null;

  @ApiPropertyOptional({ example: '2', description: 'Purchase account ID (string BigInt)' })
  @IsOptional()
  @IsString()
  purchaseAccountId?: string | null;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

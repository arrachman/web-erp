import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsNotEmpty, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpItemCategoryDto {
  @ApiProperty({ example: 'RAW-MAT', description: 'Unique category code' })
  @IsString()
  @IsNotEmpty()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Raw Materials', description: 'Category name' })
  @IsString()
  @IsNotEmpty()
  @MaxLength(120)
  name!: string;

  @ApiPropertyOptional({
    example: '123',
    description: 'Parent category ID (BigInt as string), null for root',
    nullable: true,
  })
  @IsOptional()
  @IsString()
  parentId?: string | null;

  @ApiPropertyOptional({
    example: '456',
    description: 'Inventory account ID (BigInt as string)',
    nullable: true,
  })
  @IsOptional()
  @IsString()
  inventoryAccountId?: string | null;

  @ApiPropertyOptional({
    example: '457',
    description: 'COGS account ID (BigInt as string)',
    nullable: true,
  })
  @IsOptional()
  @IsString()
  cogsAccountId?: string | null;

  @ApiPropertyOptional({
    example: '458',
    description: 'Sales account ID (BigInt as string)',
    nullable: true,
  })
  @IsOptional()
  @IsString()
  salesAccountId?: string | null;

  @ApiPropertyOptional({
    example: '459',
    description: 'Sales return account ID (BigInt as string)',
    nullable: true,
  })
  @IsOptional()
  @IsString()
  salesReturnAccountId?: string | null;

  @ApiPropertyOptional({
    example: '460',
    description: 'Sales discount account ID (BigInt as string)',
    nullable: true,
  })
  @IsOptional()
  @IsString()
  salesDiscountAccountId?: string | null;

  @ApiPropertyOptional({
    example: '461',
    description: 'Purchase return account ID (BigInt as string)',
    nullable: true,
  })
  @IsOptional()
  @IsString()
  purchaseReturnAccountId?: string | null;

  @ApiPropertyOptional({
    example: '462',
    description: 'Purchase discount account ID (BigInt as string)',
    nullable: true,
  })
  @IsOptional()
  @IsString()
  purchaseDiscountAccountId?: string | null;

  @ApiPropertyOptional({
    example: '463',
    description: 'Consignment account ID (BigInt as string)',
    nullable: true,
  })
  @IsOptional()
  @IsString()
  consignmentAccountId?: string | null;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

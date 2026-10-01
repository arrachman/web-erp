import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { ErpNumberingReset } from '@prisma/client';
import { IsBoolean, IsEnum, IsInt, IsOptional, IsString, MaxLength, Min } from 'class-validator';

export class CreateErpDocumentNumberingDto {
  @ApiProperty({ example: 'INV-OUT', description: 'Unique document code identifier' })
  @IsString()
  @MaxLength(50)
  documentCode!: string;

  @ApiProperty({ example: 'Sales Invoice', description: 'Document type name' })
  @IsString()
  @MaxLength(200)
  name!: string;

  @ApiProperty({ example: 'INV', description: 'Document number prefix' })
  @IsString()
  @MaxLength(20)
  prefix!: string;

  @ApiProperty({ example: 5, description: 'Number of digits in sequence (e.g. 5 → 00001)' })
  @IsInt()
  @Min(1)
  digitCount!: number;

  @ApiProperty({ enum: ErpNumberingReset, example: ErpNumberingReset.YEARLY })
  @IsEnum(ErpNumberingReset)
  resetPolicy!: ErpNumberingReset;

  @ApiPropertyOptional({ example: 1, description: 'Starting sequence number', default: 1 })
  @IsOptional()
  @IsInt()
  @Min(1)
  nextNumber?: number;

  @ApiPropertyOptional({ example: '1', description: 'Menu ID to associate with' })
  @IsOptional()
  @IsString()
  menuId?: string;

  @ApiPropertyOptional({ example: false })
  @IsOptional()
  @IsBoolean()
  affectsLedger?: boolean;

  @ApiPropertyOptional({ example: false })
  @IsOptional()
  @IsBoolean()
  affectsInventory?: boolean;

  @ApiPropertyOptional({ example: false })
  @IsOptional()
  @IsBoolean()
  affectsCost?: boolean;

  @ApiPropertyOptional({ example: 'Used for outgoing invoices' })
  @IsOptional()
  @IsString()
  @MaxLength(500)
  notes?: string;
}

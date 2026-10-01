import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsEnum, IsOptional, IsString, MaxLength } from 'class-validator';
import { ErpPartnerTypeKind } from '@prisma/client';

export class CreateErpPartnerTypeDto {
  @ApiProperty({ example: 'CUST', description: 'Unique partner type code' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Customer' })
  @IsString()
  @MaxLength(150)
  name!: string;

  /** Optional; server always derives kind from `code` (CUST/SUP/SLS → role, else GENERAL). */
  @ApiPropertyOptional({
    enum: ErpPartnerTypeKind,
    example: ErpPartnerTypeKind.CUSTOMER,
    description: 'Ignored if sent — kind is derived from code',
  })
  @IsOptional()
  @IsEnum(ErpPartnerTypeKind)
  kind?: ErpPartnerTypeKind;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

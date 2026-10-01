import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsIn, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpPartnerSubCategoryDto {
  @ApiProperty({ example: 'PSC-001' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Partner Sub Category Sample' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiPropertyOptional({ example: "CUSTOMER", description: "Customer/Supplier/Salesman" })
  @IsIn(['CUSTOMER','SUPPLIER','SALESMAN'])
  type!: 'CUSTOMER'|'SUPPLIER'|'SALESMAN';

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

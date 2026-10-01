import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsIn, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpStockAdjustmentTypeDto {
  @ApiProperty({ example: 'SAT-001' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Stock Adjustment Type Sample' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiPropertyOptional({ example: 'IN', description: 'IN|OUT|TRANSFER' })
  @IsOptional()
  @IsIn(['IN', 'OUT', 'TRANSFER'])
  direction?: string;

  @ApiPropertyOptional({ example: '101', nullable: true, description: 'Postable CoA account id (No Akun)' })
  @IsOptional()
  @IsString()
  accountId?: string | null;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

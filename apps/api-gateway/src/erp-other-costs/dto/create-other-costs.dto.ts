import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpOtherCostDto {
  @ApiProperty({ example: 'OCT-001' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Other Cost Sample' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiPropertyOptional({ example: '101', nullable: true })
  @IsOptional()
  @IsString()
  debitAccountId?: string | null;

  @ApiPropertyOptional({ example: '102', nullable: true })
  @IsOptional()
  @IsString()
  creditAccountId?: string | null;

  @ApiPropertyOptional({ example: false, default: false })
  @IsOptional()
  @IsBoolean()
  isHPP?: boolean = false;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

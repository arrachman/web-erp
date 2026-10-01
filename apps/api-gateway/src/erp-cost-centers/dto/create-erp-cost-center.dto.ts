import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpCostCenterDto {
  @ApiProperty({ example: 'DIV-OPS' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Operations' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiPropertyOptional({ example: '1', description: 'Parent division id (for nesting)' })
  @IsOptional()
  @IsString()
  parentId?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

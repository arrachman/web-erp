import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpSubDivisionDto {
  @ApiProperty({ example: 'SUB-OPS-A' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Operations A' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiProperty({ description: 'Parent Division id (required)' })
  @IsString()
  divisionId!: string;

  @ApiPropertyOptional({ description: 'Parent sub-division id (for nesting)' })
  @IsOptional()
  @IsString()
  parentId?: string;

  @ApiPropertyOptional({ default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpDivisionDto {
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

  @ApiPropertyOptional({ example: 'DIV-OPS-001' })
  @IsOptional()
  @IsString()
  @MaxLength(100)
  barcode?: string;

  @ApiPropertyOptional({ example: 'Division catatan tambahan' })
  @IsOptional()
  @IsString()
  @MaxLength(500)
  note?: string;
}

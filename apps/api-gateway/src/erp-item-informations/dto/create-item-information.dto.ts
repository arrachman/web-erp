import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsInt, IsOptional, IsString } from 'class-validator';
import { Type } from 'class-transformer';

export class CreateErpItemInformationDto {
  @ApiProperty({ example: '1' })
  @Type(() => String)
  @IsString()
  itemId!: string;

  @ApiPropertyOptional({ example: 'Long description of the item' })
  @IsOptional()
  @IsString()
  longDescription?: string;

  @ApiPropertyOptional({ example: 'Specification details' })
  @IsOptional()
  @IsString()
  specifications?: string;

  @ApiPropertyOptional({ example: 'Acme Corp' })
  @IsOptional()
  @IsString()
  manufacturer?: string;

  @ApiPropertyOptional({ example: 'Indonesia' })
  @IsOptional()
  @IsString()
  countryOfOrigin?: string;

  @ApiPropertyOptional({ example: 12 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  warrantyPeriodMonths?: number;

  @ApiPropertyOptional({ example: 'electronics,imported' })
  @IsOptional()
  @IsString()
  tags?: string;

  @ApiPropertyOptional({ example: 'Additional notes' })
  @IsOptional()
  @IsString()
  notes?: string;
}

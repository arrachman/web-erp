import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsBoolean,
  IsInt,
  IsObject,
  IsOptional,
  IsString,
  Max,
  MaxLength,
  Min,
} from 'class-validator';

export class CreateErpHomeWidgetDto {
  @ApiProperty({ example: 'sales-summary', description: 'Unique widget key' })
  @IsString()
  @MaxLength(120)
  widgetKey!: string;

  @ApiProperty({ example: 'Ringkasan Penjualan' })
  @IsString()
  @MaxLength(200)
  title!: string;

  @ApiPropertyOptional({ example: 'Menampilkan total penjualan hari ini' })
  @IsOptional()
  @IsString()
  description?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  enabled?: boolean = true;

  @ApiPropertyOptional({ example: 0, default: 0 })
  @IsOptional()
  @IsInt()
  sortOrder?: number = 0;

  @ApiPropertyOptional({ example: 1, default: 1, minimum: 1, maximum: 4 })
  @IsOptional()
  @IsInt()
  @Min(1)
  @Max(4)
  colSpan?: number = 1;

  @ApiPropertyOptional({ type: Object, description: 'Free-form widget config' })
  @IsOptional()
  @IsObject()
  config?: Record<string, unknown>;
}

import { ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsInt, IsObject, IsOptional, IsString, Length } from 'class-validator';

export class UpdateErpUserPreferencesDto {
  @ApiPropertyOptional({ description: 'UI theme (light | dark | system)' })
  @IsOptional()
  @IsString()
  theme?: string;

  @ApiPropertyOptional({ description: 'UI language (id | en)' })
  @IsOptional()
  @IsString()
  @Length(2, 10)
  language?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  timezone?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  dateFormat?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  numberFormat?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsInt()
  tablePageSize?: number;

  @ApiPropertyOptional()
  @IsOptional()
  @IsBoolean()
  sidebarCollapsed?: boolean;

  @ApiPropertyOptional({
    description:
      'Free-form JSON for non-canonical preferences (appearance tweaks: primary, density, fontScale, sidebar template, …)',
  })
  @IsOptional()
  @IsObject()
  metadata?: Record<string, unknown>;
}

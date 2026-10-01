import { ApiPropertyOptional } from '@nestjs/swagger';
import { IsOptional, IsString, MaxLength } from 'class-validator';

export class QueryErpSettingDto {
  @ApiPropertyOptional({ example: 'GENERAL', description: 'Filter by group' })
  @IsOptional()
  @IsString()
  @MaxLength(100)
  group?: string;

  @ApiPropertyOptional({ example: 'COMPANY_NAME', description: 'Filter by key' })
  @IsOptional()
  @IsString()
  @MaxLength(200)
  key?: string;
}

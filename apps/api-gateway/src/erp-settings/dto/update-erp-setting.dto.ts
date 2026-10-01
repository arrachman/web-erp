import { ApiPropertyOptional } from '@nestjs/swagger';
import { IsOptional, IsString, MaxLength } from 'class-validator';

export class UpdateErpSettingDto {
  @ApiPropertyOptional({ example: 'PT Sentient Factory', description: 'Setting value' })
  @IsOptional()
  @IsString()
  value?: string;

  @ApiPropertyOptional({ example: 'Company full name', description: 'Setting description' })
  @IsOptional()
  @IsString()
  @MaxLength(500)
  description?: string;
}

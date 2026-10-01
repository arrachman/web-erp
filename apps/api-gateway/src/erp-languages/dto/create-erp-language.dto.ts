import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpLanguageDto {
  @ApiProperty() @IsString() @MaxLength(10)
  code!: string;

  @ApiProperty() @IsString()
  name!: string;

  @ApiProperty() @IsString()
  nativeName!: string;

  @ApiPropertyOptional() @IsBoolean() @IsOptional()
  isRtl?: boolean;

  @ApiPropertyOptional() @IsBoolean() @IsOptional()
  isDefault?: boolean;

  @ApiPropertyOptional() @IsBoolean() @IsOptional()
  isActive?: boolean;
}

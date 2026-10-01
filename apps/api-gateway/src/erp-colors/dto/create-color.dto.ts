import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpColorDto {
  @ApiProperty({ example: 'CLR-001' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Red' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiPropertyOptional({ example: '#FF0000' })
  @IsOptional()
  @IsString()
  @MaxLength(20)
  hexCode?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

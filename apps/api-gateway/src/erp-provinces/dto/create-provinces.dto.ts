import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsIn, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpProvinceDto {
  @ApiProperty({ example: 'PRV-001' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Province Sample' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiPropertyOptional({ example: "1", description: "Country id" })
  @IsString()
  countryId!: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

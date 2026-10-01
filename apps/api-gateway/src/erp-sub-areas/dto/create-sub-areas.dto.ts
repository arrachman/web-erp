import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpSubAreaDto {
  @ApiProperty({ example: 'SA-001' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Kelurahan Sample' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiProperty({ example: '1', description: 'Area (kecamatan) id (required)' })
  @IsString()
  areaId!: string;

  @ApiPropertyOptional({ example: '10220' })
  @IsOptional()
  @IsString()
  @MaxLength(20)
  postalCode?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

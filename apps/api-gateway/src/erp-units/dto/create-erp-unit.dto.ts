import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsBoolean,
  IsNotEmpty,
  IsOptional,
  IsString,
  MaxLength,
} from 'class-validator';

export class CreateErpUnitDto {
  @ApiProperty({ example: 'KG', description: 'Unique unit code' })
  @IsString()
  @IsNotEmpty()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Kilogram', description: 'Unit name' })
  @IsString()
  @IsNotEmpty()
  @MaxLength(120)
  name!: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;

  @ApiPropertyOptional({ example: '1', description: 'Faktor konversi: 1 satuan ini = N satuan dasar (mis. 1 kwintal = 100)' })
  @IsOptional()
  @IsString()
  conversionFactor?: string;
}

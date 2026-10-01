import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsBoolean,
  IsInt,
  IsOptional,
  IsString,
  Max,
  MaxLength,
  Min,
} from 'class-validator';

export class CreateErpCurrencyDto {
  @ApiProperty({ example: 'USD', description: 'ISO 4217 currency code' })
  @IsString()
  @MaxLength(10)
  code!: string;

  @ApiProperty({ example: 'US Dollar' })
  @IsString()
  @MaxLength(120)
  name!: string;

  @ApiPropertyOptional({ example: '$' })
  @IsOptional()
  @IsString()
  @MaxLength(10)
  symbol?: string;

  @ApiPropertyOptional({
    example: 2,
    default: 2,
    description: 'Decimal places for amounts in this currency (0–6)',
  })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(0)
  @Max(6)
  decimalPlaces?: number = 2;

  @ApiPropertyOptional({
    example: false,
    default: false,
    description: 'Org home/base currency (at most one active)',
  })
  @IsOptional()
  @IsBoolean()
  isBase?: boolean = false;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

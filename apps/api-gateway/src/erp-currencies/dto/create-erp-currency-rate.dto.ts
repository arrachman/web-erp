import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsDateString, IsNotEmpty, IsOptional, IsString } from 'class-validator';

export class CreateErpCurrencyRateDto {
  @ApiProperty({ example: '2026-05-18', description: 'Rate date in ISO date format YYYY-MM-DD' })
  @IsDateString()
  rateDate!: string;

  @ApiProperty({ example: '15750.00', description: 'Exchange rate (Decimal)' })
  @IsNotEmpty()
  @IsString()
  rate!: string;

  @ApiPropertyOptional({ example: 'BI middle rate' })
  @IsOptional()
  @IsString()
  note?: string;
}

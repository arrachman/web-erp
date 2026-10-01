import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsDateString,
  IsNotEmpty,
  IsNumberString,
  IsOptional,
  IsString,
} from 'class-validator';

export class CreateVendorAdvanceDto {
  @ApiPropertyOptional({ description: 'Doc number — auto-generated if omitted' })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiProperty({ example: '2026-06-01' })
  @IsDateString()
  transactionDate!: string;

  @ApiProperty({ example: '1', description: 'Fiscal period id' })
  @IsString()
  @IsNotEmpty()
  fiscalPeriodId!: string;

  @ApiProperty({ example: '1', description: 'Branch id' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiProperty({ example: '1', description: 'Vendor/supplier partner id' })
  @IsString()
  @IsNotEmpty()
  partnerId!: string;

  @ApiProperty({ example: 'Uang muka pembelian vendor X' })
  @IsString()
  @IsNotEmpty()
  description!: string;

  @ApiProperty({ example: '1', description: 'Currency id' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000', description: 'Exchange rate (Decimal string)' })
  @IsNumberString()
  exchangeRate!: string;

  @ApiProperty({ example: '5000000.0000', description: 'Advance amount (Decimal string)' })
  @IsNumberString()
  amount!: string;

  @ApiPropertyOptional({ description: 'Internal notes' })
  @IsOptional()
  @IsString()
  notes?: string;
}

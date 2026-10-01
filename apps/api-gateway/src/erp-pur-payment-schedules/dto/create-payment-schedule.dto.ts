import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsDateString,
  IsNotEmpty,
  IsNumberString,
  IsOptional,
  IsString,
} from 'class-validator';

export class CreatePaymentScheduleDto {
  @ApiProperty({ example: 'VPP-2026-000001', description: 'Leave blank for auto-number' })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiProperty({ example: '2026-06-01' })
  @IsDateString()
  transactionDate!: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  fiscalPeriodId!: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  partnerId!: string;

  @ApiProperty({ example: 'Jadwal pembayaran vendor ABC' })
  @IsString()
  @IsNotEmpty()
  description!: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000' })
  @IsNumberString()
  exchangeRate!: string;

  @ApiProperty({ example: '5000000.0000' })
  @IsNumberString()
  amount!: string;

  @ApiPropertyOptional({ example: 'Catatan tambahan' })
  @IsOptional()
  @IsString()
  notes?: string;
}

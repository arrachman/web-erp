import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsDateString,
  IsNotEmpty,
  IsNumberString,
  IsOptional,
  IsString,
} from 'class-validator';

export class CreateArCollectionDto {
  @ApiProperty({ example: 'IC-2026-000001' })
  @IsString()
  @IsNotEmpty()
  docNumber!: string;

  @ApiProperty({ example: '2026-05-20' })
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

  @ApiProperty({ example: 'Penagihan piutang customer' })
  @IsString()
  @IsNotEmpty()
  description!: string;

  @ApiPropertyOptional({ example: '1' })
  @IsOptional()
  @IsString()
  currencyId?: string;

  @ApiPropertyOptional({ example: '1.000000' })
  @IsOptional()
  @IsNumberString()
  exchangeRate?: string;

  @ApiProperty({ example: '500000.0000' })
  @IsNumberString()
  amount!: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  notes?: string;
}

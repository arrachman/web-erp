import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsDateString,
  IsEnum,
  IsInt,
  IsNotEmpty,
  IsNumberString,
  IsOptional,
  IsString,
  Min,
} from 'class-validator';

export enum ErpGiroTypeDto {
  INCOMING = 'INCOMING',
  OUTGOING = 'OUTGOING',
}

export enum ErpGiroStatusDto {
  OUTSTANDING = 'OUTSTANDING',
  CLEARED = 'CLEARED',
  BOUNCED = 'BOUNCED',
  CANCELLED = 'CANCELLED',
}

export class CreateGiroDto {
  @ApiProperty({ example: 'GIRO-2026-000001' })
  @IsString()
  @IsNotEmpty()
  giroNumber!: string;

  @ApiProperty({ enum: ErpGiroTypeDto })
  @IsEnum(ErpGiroTypeDto)
  type!: ErpGiroTypeDto;

  @ApiPropertyOptional() @IsOptional() @IsString() source?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() sourceTransactionId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() partnerId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() bankName?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() bankAccountNo?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() bankAccountId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() giroAccountId?: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000' })
  @IsNumberString()
  exchangeRate!: string;

  @ApiProperty({ example: '500000.0000' })
  @IsNumberString()
  amount!: string;

  @ApiPropertyOptional() @IsOptional() @IsNumberString() amountFx?: string;

  @ApiProperty({ example: '2026-06-20' })
  @IsDateString()
  dueDate!: string;

  @ApiPropertyOptional() @IsOptional() @IsDateString() clearedDate?: string;

  @ApiPropertyOptional({ enum: ErpGiroStatusDto })
  @IsOptional()
  @IsEnum(ErpGiroStatusDto)
  status?: ErpGiroStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1 })
  @Type(() => Number)
  @IsInt()
  @Min(1)
  lineNo!: number;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;
}

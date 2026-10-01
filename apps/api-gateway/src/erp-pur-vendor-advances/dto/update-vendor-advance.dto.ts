import { ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsDateString,
  IsNumberString,
  IsOptional,
  IsString,
} from 'class-validator';

export class UpdateVendorAdvanceDto {
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;
  @ApiPropertyOptional() @IsOptional() @IsDateString() transactionDate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() partnerId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() currencyId?: string;
  @ApiPropertyOptional() @IsOptional() @IsNumberString() exchangeRate?: string;
  @ApiPropertyOptional() @IsOptional() @IsNumberString() amount?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() bankAccountId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() advanceAccountId?: string;
}

import { ApiPropertyOptional } from '@nestjs/swagger';
import { IsDateString, IsOptional, IsString } from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

export class UpdateSlsCustomerAdvanceDto {
  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiPropertyOptional({ example: '2026-06-03' })
  @IsOptional()
  @IsDateString()
  docDate?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  branchId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  customerId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  currencyId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsDecimalString()
  exchangeRate?: string;

  @ApiPropertyOptional({ example: '5000000.0000' })
  @IsOptional()
  @IsDecimalString()
  amount?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  paymentTermId?: string;

  @ApiPropertyOptional({ example: '2026-07-03' })
  @IsOptional()
  @IsDateString()
  dueDate?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  description?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  notes?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  referenceNo?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  costCenterId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  divisionId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  projectId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  orderId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  legacyCode?: string;
}

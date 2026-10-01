import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsDateString, IsNotEmpty, IsOptional, IsString } from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

export class CreateSlsCustomerAdvanceDto {
  @ApiPropertyOptional({
    description: 'Auto-generate docNumber via sys_document_numberings',
    default: true,
  })
  @IsOptional()
  auto?: boolean;

  @ApiPropertyOptional({
    description: 'Manual doc number; omit (or set auto=true) to server-generate',
  })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiProperty({ example: '2026-06-03', description: 'Tanggal dokumen (YYYY-MM-DD)' })
  @IsDateString()
  docDate!: string;

  @ApiPropertyOptional({
    description: 'Fiscal period id; derived from docDate when omitted',
  })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty({ example: '1', description: 'Branch (md_branches) id — Cabang' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiPropertyOptional({ description: 'Customer (md_partners) id — Pelanggan' })
  @IsOptional()
  @IsString()
  customerId?: string;

  @ApiProperty({ example: '1', description: 'Currency (md_currencies) id — Uang' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000', description: 'Exchange rate' })
  @IsDecimalString()
  exchangeRate!: string;

  @ApiProperty({ example: '5000000.0000', description: 'Advance amount (base currency)' })
  @IsDecimalString()
  amount!: string;

  @ApiPropertyOptional({ description: 'Payment term (md_payment_terms) id — Termin' })
  @IsOptional()
  @IsString()
  paymentTermId?: string;

  @ApiPropertyOptional({ example: '2026-07-03', description: 'Jatuh tempo (YYYY-MM-DD)' })
  @IsOptional()
  @IsDateString()
  dueDate?: string;

  @ApiPropertyOptional({ description: 'Uraian / keterangan' })
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

  @ApiPropertyOptional({ description: 'Cost center (md_cost_centers) id' })
  @IsOptional()
  @IsString()
  costCenterId?: string;

  @ApiPropertyOptional({ description: 'Division (md_divisions) id' })
  @IsOptional()
  @IsString()
  divisionId?: string;

  @ApiPropertyOptional({ description: 'Project (md_projects) id' })
  @IsOptional()
  @IsString()
  projectId?: string;

  @ApiPropertyOptional({ description: 'Sales Order (sls_orders) id yang terkait' })
  @IsOptional()
  @IsString()
  orderId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  legacyCode?: string;
}

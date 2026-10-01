import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  ArrayMinSize,
  IsArray,
  IsDateString,
  IsInt,
  IsNotEmpty,
  IsOptional,
  IsString,
  Min,
  ValidateNested,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

/** One line of an Invoice Swap — references the source and target invoice. */
export class SlsInvoiceSwapLineDto {
  @ApiProperty({ example: '2001', description: 'Source invoice (sls_invoices) id' })
  @IsString()
  @IsNotEmpty()
  fromInvoiceId!: string;

  @ApiPropertyOptional({ example: '2002', description: 'Target invoice (sls_invoices) id' })
  @IsOptional()
  @IsString()
  toInvoiceId?: string;

  @ApiProperty({ example: '1500000.0000', description: 'Swap amount' })
  @IsDecimalString()
  amount!: string;

  @ApiProperty({ example: 1 })
  @IsInt()
  @Min(1)
  lineNo!: number;
}

export class CreateSlsInvoiceSwapDto {
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

  @ApiProperty({ example: '1.000000' })
  @IsDecimalString()
  exchangeRate!: string;

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
  legacyCode?: string;

  @ApiProperty({ type: [SlsInvoiceSwapLineDto] })
  @IsArray()
  @ArrayMinSize(0)
  @ValidateNested({ each: true })
  @Type(() => SlsInvoiceSwapLineDto)
  lines!: SlsInvoiceSwapLineDto[];
}

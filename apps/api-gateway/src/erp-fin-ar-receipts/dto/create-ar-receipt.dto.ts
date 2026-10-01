import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  ArrayMinSize,
  IsArray,
  IsDateString,
  IsEnum,
  IsInt,
  IsNotEmpty,
  IsOptional,
  IsString,
  Min,
  ValidateNested,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

/** Full Senti approval state machine (§2.7) — mirrors DB ErpDocumentStatus. */
export enum ErpDocumentStatusDto {
  DRAFT = 'DRAFT',
  NEED_APPROVE = 'NEED_APPROVE',
  APPROVED = 'APPROVED',
  REJECTED = 'REJECTED',
  POSTED = 'POSTED',
  VOID = 'VOID',
  CANCELLED = 'CANCELLED',
}

export enum ErpSettlementStatusDto {
  UNPAID = 'UNPAID',
  PARTIAL = 'PARTIAL',
  PAID = 'PAID',
}

/**
 * Cara bayar yang didukung AR Receipt saat ini. GIRO sengaja TIDAK termasuk —
 * giro punya alur sendiri (Receipt Giro → clearing, FR-FIN-02: "belum
 * menambah saldo bank" sampai dicairkan), bukan cash/bank langsung. Kirim
 * giro lewat modul fin-giro-entries, bukan di sini.
 */
export enum ArReceiptInstrumentMethodDto {
  CASH = 'CASH',
  TRANSFER = 'TRANSFER',
  CARD = 'CARD',
  OTHER = 'OTHER',
}

export class ArReceiptInstrumentDto {
  @ApiProperty({ enum: ArReceiptInstrumentMethodDto })
  @IsEnum(ArReceiptInstrumentMethodDto)
  method!: ArReceiptInstrumentMethodDto;

  @ApiProperty({ example: '1', description: 'Akun kas/bank (md_accounts) tujuan penerimaan' })
  @IsString()
  @IsNotEmpty()
  bankAccountId!: string;

  @ApiProperty({ example: '500000.0000' })
  @IsDecimalString()
  amount!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() bankName?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() bankAccountNo?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: 1 })
  @IsInt()
  @Min(1)
  lineNo!: number;
}

export class ArReceiptAllocationDto {
  @ApiProperty({ example: '42', description: 'Sales Invoice (sls_invoices) id yang dilunasi' })
  @IsString()
  @IsNotEmpty()
  invoiceId!: string;

  @ApiProperty({ example: '500000.0000', description: 'Nominal yang dialokasikan ke invoice ini' })
  @IsDecimalString()
  amount!: string;

  @ApiProperty({ example: 1 })
  @IsInt()
  @Min(1)
  lineNo!: number;
}

export class CreateArReceiptDto {
  @ApiPropertyOptional({ description: 'Auto-generate docNumber via sys_document_numberings', default: true })
  @IsOptional()
  auto?: boolean;

  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() autoNumber?: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() source?: string;

  @ApiProperty({ example: '2026-05-20' })
  @IsDateString()
  transactionDate!: string;

  @ApiPropertyOptional({ description: 'Fiscal period id; derived from transactionDate when omitted' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty({ example: '1', description: 'Pelanggan (md_partners) id' })
  @IsString()
  @IsNotEmpty()
  partnerId!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() contactPerson?: string;

  @ApiProperty({ example: 'Penerimaan dari pelanggan' })
  @IsString()
  @IsNotEmpty()
  description!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiProperty({ example: '1' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000' })
  @IsDecimalString()
  exchangeRate!: string;

  @ApiProperty({ type: [ArReceiptInstrumentDto], description: 'Rincian cara bayar (minimal 1)' })
  @IsArray()
  @ArrayMinSize(1)
  @ValidateNested({ each: true })
  @Type(() => ArReceiptInstrumentDto)
  instruments!: ArReceiptInstrumentDto[];

  @ApiProperty({
    type: [ArReceiptAllocationDto],
    description: 'Alokasi ke invoice outstanding (minimal 1) — jumlah harus sama dengan total instruments',
  })
  @IsArray()
  @ArrayMinSize(1)
  @ValidateNested({ each: true })
  @Type(() => ArReceiptAllocationDto)
  allocations!: ArReceiptAllocationDto[];

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;
}

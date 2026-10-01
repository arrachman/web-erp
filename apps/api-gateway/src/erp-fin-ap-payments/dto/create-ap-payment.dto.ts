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
 * Cara bayar yang didukung AP Payment saat ini. GIRO sengaja TIDAK termasuk
 * — giro keluar punya alur sendiri (Send Giro → clearing, FR-FIN-02: "belum
 * mengurangi saldo bank" sampai dicairkan). Kirim giro lewat modul
 * fin-giro-entries, bukan di sini.
 */
export enum ApPaymentInstrumentMethodDto {
  CASH = 'CASH',
  TRANSFER = 'TRANSFER',
  CARD = 'CARD',
  OTHER = 'OTHER',
}

export class ApPaymentInstrumentDto {
  @ApiProperty({ enum: ApPaymentInstrumentMethodDto })
  @IsEnum(ApPaymentInstrumentMethodDto)
  method!: ApPaymentInstrumentMethodDto;

  @ApiProperty({ example: '1', description: 'Akun kas/bank (md_accounts) sumber pembayaran' })
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

export class ApPaymentAllocationDto {
  @ApiProperty({ example: '42', description: 'Purchase Invoice (pur_invoices) id yang dilunasi' })
  @IsString()
  @IsNotEmpty()
  invoiceId!: string;

  @ApiProperty({ example: '500000.0000', description: 'Nominal yang dialokasikan ke invoice ini' })
  @IsDecimalString()
  amount!: string;

  @ApiPropertyOptional({ description: 'Selisih kurs (laba/rugi) untuk alokasi ini, bila mata uang invoice beda' })
  @IsOptional()
  @IsDecimalString()
  fxGainLossAmount?: string;

  @ApiPropertyOptional({ description: 'Potongan termin (early payment discount) untuk alokasi ini' })
  @IsOptional()
  @IsDecimalString()
  termDiscountAmount?: string;

  @ApiProperty({ example: 1 })
  @IsInt()
  @Min(1)
  lineNo!: number;
}

export class CreateApPaymentDto {
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

  @ApiProperty({ example: '1', description: 'Vendor (md_partners) id' })
  @IsString()
  @IsNotEmpty()
  partnerId!: string;

  @ApiPropertyOptional() @IsOptional() @IsString() contactPerson?: string;

  @ApiProperty({ example: 'Pembayaran ke vendor' })
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

  @ApiPropertyOptional({ description: 'Akun selisih kurs (header fallback bila allocation tidak set sendiri)' })
  @IsOptional()
  @IsString()
  fxGainLossAccountId?: string;

  @ApiPropertyOptional({ description: 'Akun potongan termin (header fallback bila allocation tidak set sendiri)' })
  @IsOptional()
  @IsString()
  termDiscountAccountId?: string;

  @ApiProperty({ type: [ApPaymentInstrumentDto], description: 'Rincian cara bayar (minimal 1)' })
  @IsArray()
  @ArrayMinSize(1)
  @ValidateNested({ each: true })
  @Type(() => ApPaymentInstrumentDto)
  instruments!: ApPaymentInstrumentDto[];

  @ApiProperty({
    type: [ApPaymentAllocationDto],
    description:
      'Alokasi ke invoice outstanding (minimal 1) — jumlah instrument harus sama dengan (alokasi + selisih kurs + potongan termin)',
  })
  @IsArray()
  @ArrayMinSize(1)
  @ValidateNested({ each: true })
  @Type(() => ApPaymentAllocationDto)
  allocations!: ApPaymentAllocationDto[];

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;
}

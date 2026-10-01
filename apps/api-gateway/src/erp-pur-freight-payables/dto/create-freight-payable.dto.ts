import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsDateString,
  IsNotEmpty,
  IsNumberString,
  IsOptional,
  IsString,
} from 'class-validator';

export class CreateFreightPayableDto {
  @ApiProperty({ example: 'PP-2026-000001', description: 'Document number (auto-generated if omitted)' })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiProperty({ example: '2026-06-01' })
  @IsDateString()
  transactionDate!: string;

  @ApiProperty({ example: '1', description: 'Fiscal period ID' })
  @IsString()
  @IsNotEmpty()
  fiscalPeriodId!: string;

  @ApiProperty({ example: '1', description: 'Branch ID' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiProperty({ example: '42', description: 'Supplier / partner ID' })
  @IsString()
  @IsNotEmpty()
  partnerId!: string;

  @ApiProperty({ example: 'Biaya pengiriman barang dari supplier' })
  @IsString()
  @IsNotEmpty()
  description!: string;

  @ApiProperty({ example: '1', description: 'Currency ID' })
  @IsString()
  @IsNotEmpty()
  currencyId!: string;

  @ApiProperty({ example: '1.000000', description: 'Exchange rate to base currency' })
  @IsNumberString()
  exchangeRate!: string;

  @ApiProperty({ example: '2500000.0000', description: 'Freight payable amount' })
  @IsNumberString()
  amount!: string;

  @ApiPropertyOptional({ example: 'Catatan tambahan' })
  @IsOptional()
  @IsString()
  notes?: string;

  @ApiPropertyOptional({ description: 'Akun kas/bank (md_accounts) pembayaran ongkos kirim — wajib diisi sebelum POST' })
  @IsOptional()
  @IsString()
  bankAccountId?: string;

  @ApiPropertyOptional({
    description:
      'Akun Beban Angkut Pembelian / Freight Expense (md_accounts) — wajib diisi sebelum POST. ' +
      'Ongkos kirim dicatat terpisah dari HPP (dikonfirmasi user) — bukan menambah nilai persediaan.',
  })
  @IsOptional()
  @IsString()
  expenseAccountId?: string;
}

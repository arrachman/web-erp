import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsDateString,
  IsNotEmpty,
  IsNumberString,
  IsOptional,
  IsString,
} from 'class-validator';

export class CreateFreightReceivableDto {
  @ApiProperty({ example: 'RP-2026-000001', description: 'Document number (auto-generated if omitted)' })
  @IsOptional()
  @IsString()
  docNumber?: string;

  @ApiProperty({ example: '2026-06-01' })
  @IsDateString()
  transactionDate!: string;

  @ApiPropertyOptional({ example: '1', description: 'Fiscal period ID (resolved from transactionDate when omitted)' })
  @IsOptional()
  @IsString()
  fiscalPeriodId?: string;

  @ApiProperty({ example: '1', description: 'Branch ID' })
  @IsString()
  @IsNotEmpty()
  branchId!: string;

  @ApiProperty({ example: '42', description: 'Customer / partner ID' })
  @IsString()
  @IsNotEmpty()
  partnerId!: string;

  @ApiProperty({ example: 'Tagihan ongkos kirim ke pelanggan' })
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

  @ApiProperty({ example: '750000.0000', description: 'Freight receivable amount' })
  @IsNumberString()
  amount!: string;

  @ApiPropertyOptional({ example: 'Catatan tambahan' })
  @IsOptional()
  @IsString()
  notes?: string;

  @ApiPropertyOptional({ description: 'Akun piutang (md_accounts) untuk tagihan ongkos kirim — wajib diisi sebelum POST' })
  @IsOptional()
  @IsString()
  receivableAccountId?: string;

  @ApiPropertyOptional({
    description:
      'Akun Pendapatan Jasa Angkut / Freight Income (md_accounts) — wajib diisi sebelum POST. ' +
      'Ongkos kirim dicatat terpisah dari penjualan barang (dikonfirmasi user) — bukan mengurangi HPP/margin.',
  })
  @IsOptional()
  @IsString()
  incomeAccountId?: string;
}

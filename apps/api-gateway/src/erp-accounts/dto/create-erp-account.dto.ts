import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsArray,
  IsBoolean,
  IsEnum,
  IsOptional,
  IsString,
  MaxLength,
} from 'class-validator';
import {
  ErpAccountType,
  ErpAccountKind,
  ErpNormalBalance,
  ErpCashFlowCategory,
} from '@prisma/client';

export const ERP_ACCOUNT_CODE_MESSAGE =
  'code wajib mengikuti format kode akun aktif (dinamis dari sys_settings)';

export class CreateErpAccountDto {
  @ApiProperty({
    example: '1101.01.001',
    description:
      'Unique account code. Format dinamis dari sys_settings (default NNNN.NN.NNN, 4-2-3). Validasi dilakukan service sesuai format aktif. HEADER pakai trailing zero: `1100.00.000`. POSTABLE default: `1101.01.001`.',
  })
  @IsString()
  @MaxLength(30)
  code!: string;

  @ApiProperty({ example: 'Cash on Hand' })
  @IsString()
  @MaxLength(120)
  name!: string;

  @ApiPropertyOptional({ example: 'Kas Besar' })
  @IsOptional()
  @IsString()
  @MaxLength(120)
  alias?: string;

  @ApiProperty({ enum: ErpAccountType, example: ErpAccountType.ASSET })
  @IsEnum(ErpAccountType)
  accountType!: ErpAccountType;

  @ApiPropertyOptional({
    enum: ErpAccountKind,
    example: ErpAccountKind.POSTABLE,
    description:
      'Jenis akun. Opsional — backend menderivasinya dari kode: leaf (segmen terakhir ada digit 1–9) → POSTABLE; non-leaf → HEADER. Nilai payload diabaikan bila dikirim.',
  })
  @IsOptional()
  @IsEnum(ErpAccountKind)
  accountKind?: ErpAccountKind;

  @ApiPropertyOptional({
    enum: ErpNormalBalance,
    description:
      'Saldo normal. Opsional — backend menderivasinya dari tipe akun efektif.',
  })
  @IsOptional()
  @IsEnum(ErpNormalBalance)
  normalBalance?: ErpNormalBalance;

  @ApiPropertyOptional({ enum: ErpCashFlowCategory })
  @IsOptional()
  @IsEnum(ErpCashFlowCategory)
  cashFlowCategory?: ErpCashFlowCategory;

  @ApiPropertyOptional({ example: '1', description: 'Parent account ID (string BigInt)' })
  @IsOptional()
  @IsString()
  parentId?: string | null;

  @ApiPropertyOptional({ example: '1', description: 'Currency ID (string BigInt)' })
  @IsOptional()
  @IsString()
  currencyId?: string | null;

  @ApiPropertyOptional({
    example: 1,
    description:
      'Level hierarki. Diabaikan — backend menderivasinya dari parent (root=1, anak=parent.level+1).',
  })
  @IsOptional()
  level?: number;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;

  @ApiPropertyOptional({
    example: false,
    default: false,
    deprecated: true,
    description:
      'Deprecated — Control Account dihapus dari UI. Kolom DB tetap; create baru always false.',
  })
  @IsOptional()
  @IsBoolean()
  isControlAccount?: boolean = false;

  @ApiPropertyOptional({
    example: '1',
    description: 'Bank master ID (md_banks) — untuk akun cek/giro. Leaf only.',
  })
  @IsOptional()
  @IsString()
  bankId?: string | null;

  @ApiPropertyOptional({
    example: 'Bank BCA',
    deprecated: true,
    description: 'Legacy free-text bank name. Prefer bankId.',
  })
  @IsOptional()
  @IsString()
  @MaxLength(100)
  bankName?: string;

  @ApiPropertyOptional({ example: '1234567890', description: 'Nomor rekening bank (cek/giro)' })
  @IsOptional()
  @IsString()
  @MaxLength(50)
  bankAccountNo?: string;

  @ApiPropertyOptional({ type: [String], description: 'Cabang multi-select (BigInt ids)' })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  branchIds?: string[];

  @ApiPropertyOptional({ type: [String], description: 'Lokasi multi-select (BigInt ids)' })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  locationIds?: string[];

  @ApiPropertyOptional({ type: [String], description: 'Divisi multi-select (BigInt ids)' })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  divisionIds?: string[];

  @ApiPropertyOptional({ example: 'Opening balance notes' })
  @IsOptional()
  @IsString()
  notes?: string;
}

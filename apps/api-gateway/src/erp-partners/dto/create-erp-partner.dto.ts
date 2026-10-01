import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsArray,
  IsBoolean,
  IsInt,
  IsOptional,
  IsString,
  Max,
  MaxLength,
  Min,
} from 'class-validator';

export class CreateErpPartnerDto {
  @ApiProperty({ example: 'CUST-001', description: 'Unique partner code' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'PT Maju Bersama' })
  @IsString()
  @MaxLength(200)
  name!: string;

  @ApiPropertyOptional({ example: '1', description: 'ErpPartnerCategory ID (string → BigInt)' })
  @IsOptional()
  @IsString()
  categoryId?: string;

  @ApiProperty({
    example: '1',
    description: 'ErpPartnerType ID (string → BigInt) — peran inti partner (Customer/Supplier/Salesman/General)',
  })
  @IsString()
  partnerTypeId!: string;

  @ApiPropertyOptional({ example: '5', description: 'ErpPartnerCategory ID (kind=CUSTOMER)' })
  @IsOptional()
  @IsString()
  customerCategoryId?: string | null;

  @ApiPropertyOptional({ example: '6', description: 'ErpPartnerCategory ID (kind=SUPPLIER)' })
  @IsOptional()
  @IsString()
  supplierCategoryId?: string | null;

  @ApiPropertyOptional({ example: '7', description: 'ErpPartnerCategory ID (kind=SALESMAN)' })
  @IsOptional()
  @IsString()
  salesmanCategoryId?: string | null;

  @ApiPropertyOptional({
    example: '10',
    description: 'ErpPartner ID (salesman) — wajib untuk partner tipe Customer.',
  })
  @IsOptional()
  @IsString()
  salesmanId?: string | null;

  @ApiPropertyOptional({ example: '01.234.567.8-901.000' })
  @IsOptional()
  @IsString()
  @MaxLength(50)
  taxNumber?: string;

  @ApiPropertyOptional({ example: false, default: false })
  @IsOptional()
  @IsBoolean()
  isTaxable?: boolean = false;

  @ApiPropertyOptional({
    example: '42',
    description:
      'ErpAccount ID (BigInt as string) — control account piutang (AR) untuk partner customer. Default override saat posting AR.',
  })
  @IsOptional()
  @IsString()
  receivableAccountId?: string | null;

  @ApiPropertyOptional({
    example: '43',
    description:
      'ErpAccount ID (BigInt as string) — control account hutang (AP) untuk partner supplier. Default override saat posting AP. Bisa di-split per mata uang / per kategori (mis. Hutang IDR vs Hutang USD).',
  })
  @IsOptional()
  @IsString()
  payableAccountId?: string | null;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;

  @ApiPropertyOptional({ example: '1', description: 'ErpCurrency ID — mata uang default partner' })
  @IsOptional()
  @IsString()
  currencyId?: string | null;

  @ApiPropertyOptional({ example: '5', description: 'ErpPaymentTerm ID — termin penjualan' })
  @IsOptional()
  @IsString()
  saleTermId?: string | null;

  @ApiPropertyOptional({ example: '6', description: 'ErpPaymentTerm ID — termin pembelian' })
  @IsOptional()
  @IsString()
  purchaseTermId?: string | null;

  @ApiPropertyOptional({ example: '5000000', description: 'Batas piutang (AR credit limit)' })
  @IsOptional()
  @IsString()
  arCreditLimit?: string | null;

  @ApiPropertyOptional({ example: '3000000', description: 'Batas hutang (AP credit limit)' })
  @IsOptional()
  @IsString()
  apCreditLimit?: string | null;

  @ApiPropertyOptional({
    example: 1,
    description: 'Tingkat harga jual 1–10',
    minimum: 1,
    maximum: 10,
  })
  @IsOptional()
  @IsInt()
  @Min(1)
  @Max(10)
  salesPriceTier?: number | null;

  // Multi-select dimensions (md_partner_dim_*). When sent, md_partners.branch_id
  // is synced server-side to the first branch id (denormalized default).
  @ApiPropertyOptional({ type: [String], description: 'Cabang multi-select (BigInt ids)' })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  branchIds?: string[];

  @ApiPropertyOptional({ type: [String], description: 'Gudang multi-select (BigInt ids)' })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  warehouseIds?: string[];

  @ApiPropertyOptional({ type: [String], description: 'Lokasi multi-select (BigInt ids)' })
  @IsOptional()
  @IsArray()
  @IsString({ each: true })
  locationIds?: string[];
}

import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpBankAccountDto {
  @ApiProperty({ example: 'BNK-001', description: 'Unique bank account code' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'Rekening Operasional' })
  @IsString()
  @MaxLength(150)
  name!: string;

  @ApiProperty({ example: 'Bank Central Asia' })
  @IsString()
  @MaxLength(150)
  bankName!: string;

  @ApiProperty({ example: '1234567890' })
  @IsString()
  @MaxLength(50)
  accountNumber!: string;

  @ApiProperty({ example: 'PT Sentient Factory' })
  @IsString()
  @MaxLength(150)
  accountHolder!: string;

  @ApiPropertyOptional({ example: 'KCP Sudirman' })
  @IsOptional()
  @IsString()
  @MaxLength(150)
  branch?: string;

  @ApiPropertyOptional({ example: '1', description: 'Currency id (string or number)' })
  @IsOptional()
  @IsString()
  currencyId?: string;

  @ApiPropertyOptional({ example: '10', description: 'GL account id (string or number)' })
  @IsOptional()
  @IsString()
  glAccountId?: string;

  @ApiPropertyOptional({ example: 'CENAIDJA' })
  @IsOptional()
  @IsString()
  @MaxLength(20)
  swiftCode?: string;

  @ApiPropertyOptional({ example: false, default: false })
  @IsOptional()
  @IsBoolean()
  isPrimary?: boolean = false;

  @ApiPropertyOptional({ example: 'Catatan tambahan' })
  @IsOptional()
  @IsString()
  notes?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

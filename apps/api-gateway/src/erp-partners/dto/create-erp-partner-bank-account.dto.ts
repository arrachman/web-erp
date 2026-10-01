import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsBoolean, IsOptional, IsString, MaxLength } from 'class-validator';

export class CreateErpPartnerBankAccountDto {
  @ApiProperty({ example: 'Bank BCA' })
  @IsString()
  @MaxLength(100)
  bankName!: string;

  @ApiProperty({ example: '1234567890' })
  @IsString()
  @MaxLength(50)
  accountNumber!: string;

  @ApiPropertyOptional({ example: 'PT Maju Bersama' })
  @IsOptional()
  @IsString()
  @MaxLength(150)
  accountHolder?: string;

  @ApiPropertyOptional({ example: false, default: false })
  @IsOptional()
  @IsBoolean()
  isDefault?: boolean = false;
}

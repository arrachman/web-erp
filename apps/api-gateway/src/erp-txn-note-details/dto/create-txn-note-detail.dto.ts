import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsInt, IsOptional, IsString, Min } from 'class-validator';
import { Type } from 'class-transformer';

export class CreateErpTxnNoteDetailDto {
  @ApiProperty({ example: '1' })
  @IsString()
  transactionNoteId!: string;

  @ApiPropertyOptional({ example: 0 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(0)
  sortOrder?: number;

  @ApiProperty({ example: 'Barang harus dalam kondisi tersegel.' })
  @IsString()
  content!: string;

  @ApiPropertyOptional({ example: 'TN-DTL-001' })
  @IsOptional()
  @IsString()
  legacyCode?: string;
}

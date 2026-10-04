import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsArray,
  IsDateString,
  IsEnum,
  IsInt,
  IsOptional,
  IsString,
  Min,
  ValidateNested,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';
import { ErpFundingSourceDto } from '../../erp-sls-orders/dto/create-sls-order.dto';

/**
 * One flat row of a SIPLah export file (parsed client-side from CSV/XLSX).
 * Rows sharing `externalOrderId` are grouped into one sales order; header
 * fields are taken from the group's first row. Import is idempotent: an
 * order whose (channel=SIPLAH, externalOrderId) already exists is skipped.
 */
export class ImportSiplahRowDto {
  @ApiProperty({ description: 'SIPLah order id (idempotency key)' })
  @IsString()
  externalOrderId!: string;

  @ApiProperty({ example: '2026-09-15', description: 'Order date (YYYY-MM-DD)' })
  @IsDateString()
  orderDate!: string;

  @ApiPropertyOptional({ description: 'Partner code of the school' })
  @IsOptional()
  @IsString()
  schoolCode?: string;

  @ApiPropertyOptional({ description: 'NPSN of the school (matched via school profile)' })
  @IsOptional()
  @IsString()
  schoolNpsn?: string;

  @ApiPropertyOptional({ description: 'School name (exact match fallback)' })
  @IsOptional()
  @IsString()
  schoolName?: string;

  @ApiProperty({ description: 'Item code (md_items.code)' })
  @IsString()
  itemCode!: string;

  @ApiProperty({ example: '10' })
  @IsDecimalString()
  quantity!: string;

  @ApiPropertyOptional({ example: '150000', description: 'Unit price; defaults to item sale price' })
  @IsOptional()
  @IsDecimalString()
  unitPrice?: string;

  @ApiPropertyOptional({ enum: ErpFundingSourceDto, default: ErpFundingSourceDto.BOS })
  @IsOptional()
  @IsEnum(ErpFundingSourceDto)
  fundingSource?: ErpFundingSourceDto;

  @ApiPropertyOptional({ description: 'Budget year; defaults to the order year' })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  budgetYear?: number;

  @ApiPropertyOptional({ description: 'BOS stage 1 or 2' })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  budgetStage?: number;

  @ApiPropertyOptional({ description: 'Marketplace (SIPLah) fee for the whole order' })
  @IsOptional()
  @IsDecimalString()
  marketplaceFee?: string;

  @ApiPropertyOptional({ description: 'Disbursement (pencairan) reference' })
  @IsOptional()
  @IsString()
  disbursementRef?: string;
}

export class ImportSiplahDto {
  @ApiProperty({ type: [ImportSiplahRowDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => ImportSiplahRowDto)
  rows!: ImportSiplahRowDto[];
}

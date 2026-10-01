import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsArray,
  IsDateString,
  IsEnum,
  IsOptional,
  IsString,
  ValidateNested,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';
import {
  ErpDocumentStatusDto,
  ErpPriceModeDto,
  SlsProformaInvoiceLineDto,
} from './create-sls-proforma-invoice.dto';

/**
 * All-optional update payload. Declared explicitly (not PartialType) so the
 * required header fields (docDate/branchId/currencyId/exchangeRate) become
 * optional for partial edits.
 */
export class UpdateSlsProformaInvoiceDto {
  @ApiPropertyOptional() @IsOptional() @IsString() docNumber?: string;

  @ApiPropertyOptional() @IsOptional() @IsDateString() docDate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() fiscalPeriodId?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() locationId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() warehouseId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() customerId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() paymentTermId?: string;
  @ApiPropertyOptional() @IsOptional() @IsDateString() dueDate?: string;

  @ApiPropertyOptional() @IsOptional() @IsString() currencyId?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() exchangeRate?: string;

  @ApiPropertyOptional({ enum: ErpPriceModeDto })
  @IsOptional()
  @IsEnum(ErpPriceModeDto)
  priceMode?: ErpPriceModeDto;

  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() referenceNo?: string;
  @ApiPropertyOptional() @IsOptional() @IsDateString() referenceDate?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() receivableAccountId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() salesDeptId?: string;

  @ApiPropertyOptional() @IsOptional() @IsDecimalString() discountPercent?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() discountAmount?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() tax1Amount?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() tax2Amount?: string;
  @ApiPropertyOptional() @IsOptional() @IsDecimalString() otherCostAmount?: string;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() legacyCode?: string;

  @ApiPropertyOptional({ description: 'Parent Sales Quotation (sls_quotations) id' })
  @IsOptional()
  @IsString()
  quotationId?: string;

  @ApiPropertyOptional({ description: 'Parent Sales Order (sls_orders) id' })
  @IsOptional()
  @IsString()
  orderId?: string;

  @ApiPropertyOptional({ description: 'Custom header fields from Form Builder (JSONB)' })
  @IsOptional()
  customFields?: Record<string, unknown>;

  @ApiPropertyOptional({ type: [SlsProformaInvoiceLineDto] })
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => SlsProformaInvoiceLineDto)
  lines?: SlsProformaInvoiceLineDto[];
}

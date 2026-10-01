import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsArray,
  IsBoolean,
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
  ErpSalesChannelDto,
  SlsInvoiceLineDto,
} from './create-sls-invoice.dto';

/**
 * All-optional update payload. Declared explicitly (not PartialType) so the
 * required header fields (docDate/branchId/currencyId/exchangeRate) become
 * optional for partial edits.
 */
export class UpdateSlsInvoiceDto {
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

  @ApiPropertyOptional({ description: 'Sales Order id (sls_orders)' })
  @IsOptional()
  @IsString()
  orderId?: string;

  @ApiPropertyOptional({ description: 'Delivery Order id (sls_delivery_orders)' })
  @IsOptional()
  @IsString()
  deliveryOrderId?: string;

  @ApiPropertyOptional({ description: 'Customer Advance id (sls_customer_advances)' })
  @IsOptional()
  @IsString()
  advanceId?: string;

  @ApiPropertyOptional({ example: '0.0000', description: 'Advance amount applied to this invoice' })
  @IsOptional()
  @IsDecimalString()
  advanceAmount?: string;

  @ApiPropertyOptional({ description: 'Tax invoice number (Nomor Faktur Pajak)' })
  @IsOptional()
  @IsString()
  taxInvoiceNo?: string;

  @ApiPropertyOptional({ enum: ErpSalesChannelDto })
  @IsOptional()
  @IsEnum(ErpSalesChannelDto)
  channel?: ErpSalesChannelDto;

  @ApiPropertyOptional({ description: 'Flag saldo awal piutang — Opening Balance' })
  @IsOptional()
  @IsBoolean()
  isOpeningBalance?: boolean;

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

  @ApiPropertyOptional({ description: 'Custom header fields from Form Builder (JSONB)' })
  @IsOptional()
  customFields?: Record<string, unknown>;

  @ApiPropertyOptional({ type: [SlsInvoiceLineDto] })
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => SlsInvoiceLineDto)
  lines?: SlsInvoiceLineDto[];
}

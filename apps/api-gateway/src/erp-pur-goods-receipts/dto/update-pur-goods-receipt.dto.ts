import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsArray, IsDateString, IsEnum, IsOptional, IsString, ValidateNested } from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';
import { ErpDocumentStatusDto, ErpPriceModeDto, PurGoodsReceiptLineDto } from './create-pur-goods-receipt.dto';

export class UpdatePurGoodsReceiptDto {
  @IsOptional() @IsString() docNumber?: string;
  @IsOptional() @IsDateString() docDate?: string;
  @IsOptional() @IsString() fiscalPeriodId?: string;
  @IsOptional() @IsString() branchId?: string;
  @IsOptional() @IsString() locationId?: string;
  @IsOptional() @IsString() warehouseId?: string;
  @IsOptional() @IsString() supplierId?: string;
  @IsOptional() @IsString() paymentTermId?: string;
  @IsOptional() @IsDateString() dueDate?: string;
  @IsOptional() @IsString() currencyId?: string;
  @IsOptional() @IsDecimalString() exchangeRate?: string;
  @IsOptional() @IsEnum(ErpPriceModeDto) priceMode?: ErpPriceModeDto;
  @IsOptional() @IsString() description?: string;
  @IsOptional() @IsString() notes?: string;
  @IsOptional() @IsString() referenceNo?: string;
  @IsOptional() @IsDateString() referenceDate?: string;
  @IsOptional() @IsString() payableAccountId?: string;
  @IsOptional() @IsString() orderId?: string;
  @IsOptional() @IsDecimalString() discountPercent?: string;
  @IsOptional() @IsDecimalString() discountAmount?: string;
  @IsOptional() @IsDecimalString() tax1Amount?: string;
  @IsOptional() @IsDecimalString() tax2Amount?: string;
  @IsOptional() @IsDecimalString() otherCostAmount?: string;
  @IsOptional() @IsEnum(ErpDocumentStatusDto) status?: ErpDocumentStatusDto;
  @IsOptional() @IsString() legacyCode?: string;

  @ApiPropertyOptional({ type: [PurGoodsReceiptLineDto] })
  @IsOptional() @IsArray() @ValidateNested({ each: true }) @Type(() => PurGoodsReceiptLineDto)
  lines?: PurGoodsReceiptLineDto[];
}

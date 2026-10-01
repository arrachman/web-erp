import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsArray, IsDateString, IsEnum, IsOptional, IsString, ValidateNested } from 'class-validator';
import { ErpDocumentStatusDto, PurRfqSupplierLineDto } from './create-pur-rfq.dto';

export class UpdatePurRfqDto {
  @IsOptional() @IsString() docNumber?: string;
  @IsOptional() @IsDateString() docDate?: string;
  @IsOptional() @IsString() fiscalPeriodId?: string;
  @IsOptional() @IsString() branchId?: string;
  @IsOptional() @IsString() locationId?: string;
  @IsOptional() @IsString() warehouseId?: string;
  @IsOptional() @IsString() currencyId?: string;
  @IsOptional() @IsString() exchangeRate?: string;
  @IsOptional() @IsString() requisitionId?: string;
  @IsOptional() @IsDateString() validFrom?: string;
  @IsOptional() @IsDateString() validTo?: string;
  @IsOptional() @IsString() description?: string;
  @IsOptional() @IsString() notes?: string;
  @IsOptional() @IsString() referenceNo?: string;
  @IsOptional() @IsEnum(ErpDocumentStatusDto) status?: ErpDocumentStatusDto;
  @IsOptional() @IsString() legacyCode?: string;

  @ApiPropertyOptional({ type: [PurRfqSupplierLineDto] })
  @IsOptional() @IsArray() @ValidateNested({ each: true }) @Type(() => PurRfqSupplierLineDto)
  suppliers?: PurRfqSupplierLineDto[];
}

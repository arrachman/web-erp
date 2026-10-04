import { Type } from 'class-transformer';
import {
  IsArray,
  IsIn,
  IsInt,
  IsOptional,
  IsString,
  MaxLength,
  Min,
  ValidateNested,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

export const PRINT_COMPONENTS = [
  'KERTAS',
  'TINTA',
  'PLAT',
  'PRE_PRESS',
  'CETAK',
  'FINISHING',
  'MAKLOON',
  'OVERHEAD',
  'LAIN',
] as const;

export class PrintEstimateLineDto {
  @IsInt()
  lineNo!: number;

  @IsIn(PRINT_COMPONENTS as unknown as string[])
  component!: string;

  @IsOptional()
  @IsString()
  @MaxLength(500)
  description?: string;

  @IsOptional()
  @IsString()
  itemId?: string;

  @IsDecimalString()
  quantity!: string;

  @IsOptional()
  @IsString()
  unitId?: string;

  @IsDecimalString()
  unitCost!: string;
}

export class CreatePrintEstimateDto {
  @IsString()
  branchId!: string;

  @IsString()
  docDate!: string;

  @IsOptional()
  @IsString()
  customerId?: string;

  @IsOptional()
  @IsString()
  itemId?: string;

  @IsString()
  @MaxLength(300)
  title!: string;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  paperSize?: string;

  @IsOptional()
  @IsInt()
  @Min(0)
  pageCount?: number;

  @IsDecimalString()
  printQuantity!: string;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  colorSpec?: string;

  @IsOptional()
  @IsString()
  @MaxLength(200)
  finishing?: string;

  @IsOptional()
  @IsDecimalString()
  marginPercent?: string;

  @IsOptional()
  @IsString()
  notes?: string;

  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => PrintEstimateLineDto)
  lines!: PrintEstimateLineDto[];
}

export class UpdatePrintEstimateDto {
  @IsOptional()
  @IsString()
  docDate?: string;

  @IsOptional()
  @IsString()
  customerId?: string;

  @IsOptional()
  @IsString()
  itemId?: string;

  @IsOptional()
  @IsString()
  @MaxLength(300)
  title?: string;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  paperSize?: string;

  @IsOptional()
  @IsInt()
  @Min(0)
  pageCount?: number;

  @IsOptional()
  @IsDecimalString()
  printQuantity?: string;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  colorSpec?: string;

  @IsOptional()
  @IsString()
  @MaxLength(200)
  finishing?: string;

  @IsOptional()
  @IsDecimalString()
  marginPercent?: string;

  @IsOptional()
  @IsString()
  notes?: string;

  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => PrintEstimateLineDto)
  lines?: PrintEstimateLineDto[];
}

export class QueryPrintEstimatesDto {
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  limit?: number;

  @IsOptional()
  @IsString()
  search?: string;

  @IsOptional()
  @IsIn(['DRAFT', 'ISSUED', 'CONVERTED', 'CANCELLED'])
  status?: string;

  @IsOptional()
  @IsIn(['docNumber', 'docDate', 'totalPrice', 'totalCost', 'createdAt'])
  sortBy?: string;

  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: string;
}

export class SetPrintEstimateStatusDto {
  @IsIn(['ISSUED', 'CANCELLED', 'DRAFT'])
  status!: string;
}

import { Type } from 'class-transformer';
import {
  IsBoolean,
  IsIn,
  IsInt,
  IsOptional,
  IsString,
  MaxLength,
  Min,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

export const PRINT_JOB_STAGES = [
  'PRE_PRESS',
  'CETAK',
  'FINISHING',
  'QC',
  'SELESAI',
  'CANCELLED',
] as const;

export class CreatePrintJobDto {
  @IsString()
  branchId!: string;

  @IsString()
  docDate!: string;

  @IsOptional()
  @IsString()
  estimateId?: string;

  @IsOptional()
  @IsString()
  itemId?: string;

  @IsOptional()
  @IsDecimalString()
  printQuantity?: string;

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
  @IsString()
  @MaxLength(100)
  colorSpec?: string;

  @IsOptional()
  @IsString()
  @MaxLength(200)
  finishing?: string;

  @IsOptional()
  @IsString()
  @MaxLength(300)
  masterFileName?: string;

  @IsOptional()
  @IsString()
  notes?: string;
}

export class UpdatePrintJobDto {
  @IsOptional()
  @IsString()
  @MaxLength(100)
  paperSize?: string;

  @IsOptional()
  @IsInt()
  @Min(0)
  pageCount?: number;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  colorSpec?: string;

  @IsOptional()
  @IsString()
  @MaxLength(200)
  finishing?: string;

  @IsOptional()
  @IsString()
  @MaxLength(300)
  masterFileName?: string;

  @IsOptional()
  @IsString()
  notes?: string;
}

export class QueryPrintJobsDto {
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
  @IsIn(PRINT_JOB_STAGES as unknown as string[])
  stage?: string;

  @IsOptional()
  @IsIn(['createdAt', 'printQuantity'])
  sortBy?: string;

  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: string;
}

export class ChecklistPrintJobDto {
  @IsString()
  key!: string;

  @IsBoolean()
  done!: boolean;
}

export class AdvancePrintJobDto {
  @IsIn(PRINT_JOB_STAGES as unknown as string[])
  toStage!: string;
}

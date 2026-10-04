import { Type } from 'class-transformer';
import { IsIn, IsInt, IsOptional, IsString, MaxLength, Min } from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

export const JOB_COST_TYPES = [
  'MATERIAL',
  'TENAGA_KERJA',
  'OVERHEAD',
  'MAKLOON',
  'LAIN',
] as const;

export const JOB_COST_STAGES = ['PRE_PRESS', 'CETAK', 'FINISHING', 'QC'] as const;

export class CreateJobCostEntryDto {
  @IsString()
  entryDate!: string;

  @IsIn(JOB_COST_TYPES as unknown as string[])
  costType!: string;

  @IsOptional()
  @IsIn(JOB_COST_STAGES as unknown as string[])
  stage?: string;

  @IsOptional()
  @IsString()
  @MaxLength(500)
  description?: string;

  @IsOptional()
  @IsString()
  itemId?: string;

  @IsOptional()
  @IsDecimalString()
  quantity?: string;

  @IsOptional()
  @IsString()
  unitId?: string;

  @IsDecimalString()
  unitCost!: string;
}

export class UpdateJobCostEntryDto {
  @IsOptional()
  @IsString()
  entryDate?: string;

  @IsOptional()
  @IsIn(JOB_COST_TYPES as unknown as string[])
  costType?: string;

  @IsOptional()
  @IsIn(JOB_COST_STAGES as unknown as string[])
  stage?: string;

  @IsOptional()
  @IsString()
  @MaxLength(500)
  description?: string;

  @IsOptional()
  @IsString()
  itemId?: string;

  @IsOptional()
  @IsDecimalString()
  quantity?: string;

  @IsOptional()
  @IsString()
  unitId?: string;

  @IsOptional()
  @IsDecimalString()
  unitCost?: string;
}

export class QueryJobCostEntriesDto {
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
  @IsIn(JOB_COST_TYPES as unknown as string[])
  costType?: string;
}

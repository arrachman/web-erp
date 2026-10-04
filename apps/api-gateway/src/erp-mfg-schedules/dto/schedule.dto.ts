import { Type } from 'class-transformer';
import { IsIn, IsInt, IsOptional, IsString, MaxLength, Min } from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

export const MACHINE_TYPES = ['OFFSET', 'DIGITAL', 'FINISHING', 'LAIN'] as const;
export const MACHINE_STATUSES = ['ACTIVE', 'MAINTENANCE', 'INACTIVE'] as const;
export const SCHEDULE_STATUSES = ['TERJADWAL', 'BERJALAN', 'SELESAI', 'BATAL'] as const;
export const SCHEDULE_STAGES = ['PRE_PRESS', 'CETAK', 'FINISHING', 'QC'] as const;

export class CreateMachineDto {
  @IsString()
  @MaxLength(50)
  code!: string;

  @IsString()
  @MaxLength(200)
  name!: string;

  @IsIn(MACHINE_TYPES as unknown as string[])
  machineType!: string;

  @IsOptional()
  @IsDecimalString()
  capacityPerHour?: string;

  @IsOptional()
  @IsString()
  @MaxLength(50)
  capacityUnit?: string;

  @IsOptional()
  @IsString()
  workStart?: string;

  @IsOptional()
  @IsString()
  workEnd?: string;

  @IsOptional()
  @IsString()
  notes?: string;
}

export class UpdateMachineDto {
  @IsOptional()
  @IsString()
  @MaxLength(200)
  name?: string;

  @IsOptional()
  @IsIn(MACHINE_TYPES as unknown as string[])
  machineType?: string;

  @IsOptional()
  @IsDecimalString()
  capacityPerHour?: string;

  @IsOptional()
  @IsString()
  @MaxLength(50)
  capacityUnit?: string;

  @IsOptional()
  @IsString()
  workStart?: string;

  @IsOptional()
  @IsString()
  workEnd?: string;

  @IsOptional()
  @IsIn(MACHINE_STATUSES as unknown as string[])
  status?: string;

  @IsOptional()
  @IsString()
  notes?: string;
}

export class CreateJobScheduleDto {
  @IsString()
  jobId!: string;

  @IsString()
  machineId!: string;

  @IsOptional()
  @IsIn(SCHEDULE_STAGES as unknown as string[])
  stage?: string;

  @IsString()
  plannedStart!: string;

  @IsString()
  plannedEnd!: string;

  @IsOptional()
  @IsInt()
  sequenceNo?: number;

  @IsOptional()
  @IsString()
  @MaxLength(500)
  notes?: string;
}

export class UpdateJobScheduleDto {
  @IsOptional()
  @IsString()
  machineId?: string;

  @IsOptional()
  @IsIn(SCHEDULE_STAGES as unknown as string[])
  stage?: string;

  @IsOptional()
  @IsString()
  plannedStart?: string;

  @IsOptional()
  @IsString()
  plannedEnd?: string;

  @IsOptional()
  @IsInt()
  sequenceNo?: number;

  @IsOptional()
  @IsString()
  @MaxLength(500)
  notes?: string;
}

export class SetJobScheduleStatusDto {
  @IsIn(SCHEDULE_STATUSES as unknown as string[])
  status!: string;
}

export class QueryJobSchedulesDto {
  @IsOptional()
  @IsString()
  from?: string;

  @IsOptional()
  @IsString()
  to?: string;

  @IsOptional()
  @IsString()
  machineId?: string;

  @IsOptional()
  @IsString()
  jobId?: string;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  limit?: number;
}

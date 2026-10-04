import { Type } from 'class-transformer';
import { IsIn, IsInt, IsOptional, IsString, MaxLength, Min } from 'class-validator';

export const LOT_STATUSES = ['ACTIVE', 'QUARANTINE', 'EXPIRED', 'BLOCKED'] as const;

export class CreateInvLotDto {
  @IsString()
  @MaxLength(100)
  lotNumber!: string;

  @IsString()
  itemId!: string;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  supplierLotNo?: string;

  @IsOptional()
  @IsString()
  manufactureDate?: string;

  @IsOptional()
  @IsString()
  expiryDate?: string;

  @IsOptional()
  @IsIn(LOT_STATUSES as unknown as string[])
  status?: string;

  @IsOptional()
  @IsString()
  @MaxLength(500)
  notes?: string;
}

export class UpdateInvLotDto {
  @IsOptional()
  @IsString()
  @MaxLength(100)
  lotNumber?: string;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  supplierLotNo?: string;

  @IsOptional()
  @IsString()
  manufactureDate?: string;

  @IsOptional()
  @IsString()
  expiryDate?: string;

  @IsOptional()
  @IsIn(LOT_STATUSES as unknown as string[])
  status?: string;

  @IsOptional()
  @IsString()
  @MaxLength(500)
  notes?: string;
}

export class QueryInvLotsDto {
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
  @IsString()
  itemId?: string;

  @IsOptional()
  @IsIn(LOT_STATUSES as unknown as string[])
  status?: string;

  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(0)
  expiringWithinDays?: number;
}

export class FefoQueryDto {
  @IsString()
  itemId!: string;

  @IsString()
  warehouseId!: string;

  @IsString()
  quantity!: string;
}

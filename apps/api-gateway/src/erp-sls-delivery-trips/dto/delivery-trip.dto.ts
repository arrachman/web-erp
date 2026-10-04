import { Type } from 'class-transformer';
import { IsIn, IsInt, IsOptional, IsString, MaxLength, Min } from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

export const VEHICLE_STATUSES = ['ACTIVE', 'MAINTENANCE', 'INACTIVE'] as const;
export const TRIP_STATUSES = ['DRAFT', 'MUAT', 'BERANGKAT', 'SELESAI', 'BATAL'] as const;

export class CreateVehicleDto {
  @IsString()
  @MaxLength(50)
  code!: string;

  @IsString()
  @MaxLength(200)
  name!: string;

  @IsOptional()
  @IsString()
  @MaxLength(50)
  plateNo?: string;

  @IsOptional()
  @IsString()
  @MaxLength(30)
  vehicleType?: string;

  @IsOptional()
  @IsDecimalString()
  capacityKg?: string;

  @IsOptional()
  @IsString()
  notes?: string;
}

export class UpdateVehicleDto {
  @IsOptional()
  @IsString()
  @MaxLength(200)
  name?: string;

  @IsOptional()
  @IsString()
  @MaxLength(50)
  plateNo?: string;

  @IsOptional()
  @IsString()
  @MaxLength(30)
  vehicleType?: string;

  @IsOptional()
  @IsDecimalString()
  capacityKg?: string;

  @IsOptional()
  @IsIn(VEHICLE_STATUSES as unknown as string[])
  status?: string;

  @IsOptional()
  @IsString()
  notes?: string;
}

export class CreateDeliveryTripDto {
  @IsString()
  tripDate!: string;

  @IsString()
  vehicleId!: string;

  @IsOptional()
  @IsString()
  branchId?: string;

  @IsOptional()
  @IsString()
  @MaxLength(200)
  driverName?: string;

  @IsOptional()
  @IsString()
  @MaxLength(500)
  notes?: string;
}

export class UpdateDeliveryTripDto {
  @IsOptional()
  @IsString()
  vehicleId?: string;

  @IsOptional()
  @IsString()
  tripDate?: string;

  @IsOptional()
  @IsString()
  @MaxLength(200)
  driverName?: string;

  @IsOptional()
  @IsString()
  @MaxLength(500)
  notes?: string;
}

export class SetTripCostsDto {
  @IsOptional()
  @IsDecimalString()
  fuelCost?: string;

  @IsOptional()
  @IsDecimalString()
  tollCost?: string;

  @IsOptional()
  @IsDecimalString()
  otherCost?: string;
}

export class SetDeliveryTripStatusDto {
  @IsIn(TRIP_STATUSES as unknown as string[])
  status!: string;
}

export class AddTripStopDto {
  @IsString()
  deliveryOrderId!: string;
}

export class ArriveTripStopDto {
  @IsString()
  @MaxLength(200)
  receiverName!: string;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  receiverTitle?: string;
}

export class FailTripStopDto {
  @IsString()
  @MaxLength(500)
  failureNote!: string;
}

export class QueryDeliveryTripsDto {
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
  @IsIn(TRIP_STATUSES as unknown as string[])
  status?: string;

  @IsOptional()
  @IsString()
  search?: string;
}

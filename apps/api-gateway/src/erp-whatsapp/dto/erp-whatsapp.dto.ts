import { Type } from 'class-transformer';
import {
  IsBoolean,
  IsIn,
  IsInt,
  IsObject,
  IsOptional,
  IsString,
  Matches,
  MaxLength,
  Min,
  MinLength,
} from 'class-validator';

export class CreateWaTemplateDto {
  @IsString() @MinLength(2) @MaxLength(80)
  name!: string;

  @IsString() @MinLength(2) @MaxLength(40)
  category!: string;

  @IsOptional() @IsString() @MaxLength(60)
  triggerEvent?: string;

  @IsString() @MinLength(5)
  body!: string;

  @IsOptional() @IsBoolean()
  isActive?: boolean;
}

export class UpdateWaTemplateDto {
  @IsOptional() @IsString() @MinLength(2) @MaxLength(80)
  name?: string;

  @IsOptional() @IsString() @MinLength(2) @MaxLength(40)
  category?: string;

  @IsOptional() @IsString() @MaxLength(60)
  triggerEvent?: string;

  @IsOptional() @IsString() @MinLength(5)
  body?: string;

  @IsOptional() @IsBoolean()
  isActive?: boolean;
}

export class QueryWaTemplateDto {
  @IsOptional() @Type(() => Number) @IsInt() @Min(1)
  page?: number;

  @IsOptional() @Type(() => Number) @IsInt() @Min(1)
  limit?: number;

  @IsOptional() @IsString()
  category?: string;

  @IsOptional() @IsString()
  search?: string;
}

export class QueryWaLogDto {
  @IsOptional() @Type(() => Number) @IsInt() @Min(1)
  page?: number;

  @IsOptional() @Type(() => Number) @IsInt() @Min(1)
  limit?: number;

  @IsOptional() @IsString()
  status?: string;

  @IsOptional() @IsString()
  recipientPhone?: string;
}

export class SendWaTestDto {
  @IsString() @MinLength(8) @MaxLength(20)
  phone!: string;

  @IsOptional() @IsString()
  templateId?: string;

  @IsOptional() @IsString() @MaxLength(2000)
  body?: string;

  @IsOptional() @IsObject()
  variables?: Record<string, string | number>;
}

export class UpdateWaSettingsDto {
  @IsOptional() @IsBoolean()
  sendEnabled?: boolean;
}

// ----- Device pairing -----

export class CreateWaDeviceDto {
  @IsString() @MinLength(1) @MaxLength(60)
  name!: string;

  @IsString() @MinLength(8) @MaxLength(20)
  phone!: string;

  @IsOptional() @IsIn(['on', 'off'])
  autoread?: 'on' | 'off';
}

export class WaDeviceQrDto {
  @IsString() @MinLength(8) @MaxLength(160)
  deviceToken!: string;

  @IsOptional() @IsBoolean()
  force?: boolean;
}

export class ActivateWaDeviceDto {
  @IsString() @MinLength(8) @MaxLength(160)
  deviceToken!: string;

  @IsOptional() @IsString() @MaxLength(30)
  devicePhone?: string;

  @IsOptional() @IsBoolean()
  removePrevious?: boolean;
}

export class UpdateWaDeviceDto {
  @IsString() @MinLength(8) @MaxLength(160)
  deviceToken!: string;

  @IsString() @MinLength(2) @MaxLength(30)
  name!: string;

  @IsString() @MinLength(8) @MaxLength(15)
  phone!: string;
}

export class DeleteWaDeviceDto {
  @IsString() @Matches(/^[0-9+]{8,20}$/)
  devicePhone!: string;
}

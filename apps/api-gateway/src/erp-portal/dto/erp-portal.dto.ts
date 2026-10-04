import {
  IsEmail,
  IsEnum,
  IsInt,
  IsOptional,
  IsString,
  Min,
  MinLength,
  IsArray,
  ArrayMinSize,
  ValidateNested,
} from 'class-validator';
import { Type } from 'class-transformer';

/** Fase 3 W2/W3 — Portal Sekolah: DTO publik + portal + admin. */

export enum PortalJenjangDto {
  PAUD = 'PAUD',
  TK = 'TK',
  SD = 'SD',
  SMP = 'SMP',
  SMA = 'SMA',
  SMK = 'SMK',
  SLB = 'SLB',
  OTHER = 'OTHER',
}

export enum PortalRoleDto {
  KEPALA_SEKOLAH = 'KEPALA_SEKOLAH',
  BENDAHARA = 'BENDAHARA',
  OPERATOR = 'OPERATOR',
  ORANG_TUA = 'ORANG_TUA',
}

export enum PortalFundingDto {
  BOS = 'BOS',
  NON_BOS = 'NON_BOS',
}

export class PortalRegisterDto {
  @IsString() schoolName!: string;
  @IsOptional() @IsEnum(PortalJenjangDto) jenjang?: PortalJenjangDto;
  @IsOptional() @IsString() npsn?: string;
  @IsOptional() @IsString() address?: string;
  @IsString() fullName!: string;
  @IsEmail() email!: string;
  @IsOptional() @IsString() phone?: string;
  @IsOptional() @IsEnum(PortalRoleDto) role?: PortalRoleDto;
  @IsString() @MinLength(8) password!: string;
}

export class PortalLoginDto {
  @IsEmail() email!: string;
  @IsString() password!: string;
}

/** Fase 3 W4 — pendaftaran orang tua: memilih sekolah yang SUDAH terdaftar. */
export class PortalRegisterParentDto {
  @IsString() fullName!: string;
  @IsEmail() email!: string;
  @IsOptional() @IsString() phone?: string;
  @IsString() @MinLength(8) password!: string;
  @IsString() schoolPartnerId!: string;
  @IsString() studentName!: string;
  @IsString() studentClass!: string;
}

export class PortalLeadDto {
  @IsString() schoolName!: string;
  @IsString() contactName!: string;
  @IsOptional() @IsString() phone?: string;
  @IsOptional() @IsEmail() email?: string;
  @IsOptional() @IsEnum(PortalJenjangDto) jenjang?: PortalJenjangDto;
  @IsOptional() @IsString() message?: string;
}

export class PortalOrderLineDto {
  @IsString() itemId!: string;
  @IsInt() @Min(1) quantity!: number;
}

export class PortalCreateOrderDto {
  @IsArray() @ArrayMinSize(1) @ValidateNested({ each: true })
  @Type(() => PortalOrderLineDto)
  lines!: PortalOrderLineDto[];

  @IsOptional() @IsEnum(PortalFundingDto) fundingSource?: PortalFundingDto;
  @IsOptional() @IsInt() budgetYear?: number;
  @IsOptional() @IsString() notes?: string;
  /** Idempotency key from the client; a repeat submit returns the same order. */
  @IsOptional() @IsString() clientRef?: string;
}

export class PortalUpdateProfileDto {
  @IsOptional() @IsString() fullName?: string;
  @IsOptional() @IsString() phone?: string;
  @IsOptional() @IsInt() @Min(0) studentCount?: number;
  @IsOptional() @IsInt() @Min(0) classCount?: number;
}

export class PortalRejectDto {
  @IsOptional() @IsString() reason?: string;
}

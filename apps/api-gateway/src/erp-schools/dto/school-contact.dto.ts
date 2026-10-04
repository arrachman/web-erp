import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  IsBoolean,
  IsEnum,
  IsOptional,
  IsString,
  MaxLength,
} from 'class-validator';
import { ErpPartnerContactRole, ErpSchoolActivityType } from '@prisma/client';

/**
 * One contact person of a school, synced through the school create/update
 * payload (the form edits the whole contact set at once). `id` is present
 * only for contacts that already exist; on update, contacts missing from
 * the array are soft-deleted and rows without `id` are created.
 */
export class SchoolContactInputDto {
  @ApiPropertyOptional({ example: '12', description: 'Existing contact id (update only)' })
  @IsOptional()
  @IsString()
  id?: string;

  @ApiProperty({ example: 'Budi Santoso' })
  @IsString()
  @MaxLength(200)
  name!: string;

  @ApiPropertyOptional({ enum: ErpPartnerContactRole, example: ErpPartnerContactRole.KEPALA_SEKOLAH })
  @IsOptional()
  @IsEnum(ErpPartnerContactRole)
  role?: ErpPartnerContactRole;

  @ApiPropertyOptional({ example: 'Kepala Sekolah', description: 'Jabatan bebas (opsional)' })
  @IsOptional()
  @IsString()
  @MaxLength(100)
  title?: string;

  @ApiPropertyOptional({ example: '0812-3456-7890' })
  @IsOptional()
  @IsString()
  @MaxLength(50)
  phone?: string;

  @ApiPropertyOptional({ example: 'kepsek@sekolah.sch.id' })
  @IsOptional()
  @IsString()
  @MaxLength(200)
  email?: string;

  @ApiPropertyOptional({ example: false })
  @IsOptional()
  @IsBoolean()
  isDefault?: boolean;
}

/** Payload for appending one entry to a school's activity log. */
export class CreateSchoolActivityDto {
  @ApiProperty({ enum: ErpSchoolActivityType, example: ErpSchoolActivityType.VISIT })
  @IsEnum(ErpSchoolActivityType)
  type!: ErpSchoolActivityType;

  @ApiPropertyOptional({ example: '2026-10-04', description: 'Tanggal aktivitas (default: sekarang)' })
  @IsOptional()
  @IsString()
  activityAt?: string;

  @ApiProperty({ example: 'Kunjungan awal, bertemu kepala sekolah & bendahara' })
  @IsString()
  @MaxLength(2000)
  notes!: string;

  @ApiPropertyOptional({ example: '12', description: 'Kontak yang ditemui (opsional)' })
  @IsOptional()
  @IsString()
  contactId?: string;
}

/** Optional single activity appended as part of a school create/update. */
export class SchoolNewActivityDto {
  @ApiProperty({ enum: ErpSchoolActivityType, example: ErpSchoolActivityType.VISIT })
  @IsEnum(ErpSchoolActivityType)
  type!: ErpSchoolActivityType;

  @ApiPropertyOptional({ example: '2026-10-04' })
  @IsOptional()
  @IsString()
  activityAt?: string;

  @ApiProperty({ example: 'Negosiasi harga paket BOS tahap 1' })
  @IsString()
  @MaxLength(2000)
  notes!: string;
}

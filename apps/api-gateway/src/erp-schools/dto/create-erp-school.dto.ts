import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsBoolean, IsInt, IsObject, IsOptional, IsString, MaxLength, Max, Min, ValidateNested } from 'class-validator';
import { ErpSchoolBosStage, ErpSchoolJenjang, ErpSchoolNegeriSwasta, ErpSchoolPipelineStage } from '@prisma/client';
import { SchoolContactInputDto, SchoolNewActivityDto } from './school-contact.dto';

/**
 * Create a school = create the customer partner (`md_partners`, type SCHOOL)
 * AND its 1:1 profile (`md_school_profiles`) in one transaction.
 *
 * Yayasan (parent organization) is NOT part of MVP-1 — see
 * docs/bahtera-madani-mvp-scope.md §7. A school is always the transaction
 * anchor; orders, invoices, BAST and AR all live on the partner row.
 */
export class CreateErpSchoolDto {
  @ApiProperty({ example: 'SCH-0001', description: 'Unique partner code' })
  @IsString()
  @MaxLength(50)
  code!: string;

  @ApiProperty({ example: 'SD Negeri 1 Bahtera' })
  @IsString()
  @MaxLength(200)
  name!: string;

  @ApiPropertyOptional({ example: '1', description: 'ErpPartner ID (salesman) — wajib untuk partner customer' })
  @IsOptional()
  @IsString()
  salesmanId?: string | null;

  @ApiPropertyOptional({ example: '01.234.567.8-901.000' })
  @IsOptional()
  @IsString()
  @MaxLength(50)
  taxNumber?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isTaxable?: boolean = false;

  @ApiPropertyOptional({ type: [String], description: 'Cabang multi-select (BigInt ids)' })
  @IsOptional()
  @IsString({ each: true })
  branchIds?: string[];

  @ApiPropertyOptional({ type: [String], description: 'Gudang multi-select (BigInt ids)' })
  @IsOptional()
  @IsString({ each: true })
  warehouseIds?: string[];

  @ApiPropertyOptional({ type: [String], description: 'Lokasi multi-select (BigInt ids)' })
  @IsOptional()
  @IsString({ each: true })
  locationIds?: string[];

  // ── School profile ──────────────────────────────────────────────────────────

  @ApiPropertyOptional({ example: '200001', description: 'NPSN' })
  @IsOptional()
  @IsString()
  @MaxLength(20)
  npsn?: string;

  @ApiPropertyOptional({ enum: ErpSchoolJenjang, example: ErpSchoolJenjang.SD })
  @IsOptional()
  jenjang?: ErpSchoolJenjang;

  @ApiPropertyOptional({ enum: ErpSchoolNegeriSwasta, example: ErpSchoolNegeriSwasta.NEGERI })
  @IsOptional()
  negeriSwasta?: ErpSchoolNegeriSwasta;

  @ApiPropertyOptional({ example: 'A', description: 'Akreditasi' })
  @IsOptional()
  @IsString()
  @MaxLength(20)
  accreditation?: string;

  @ApiPropertyOptional({ example: 180, description: 'Jumlah siswa' })
  @IsOptional()
  @IsInt()
  @Min(0)
  studentCount?: number;

  @ApiPropertyOptional({ example: 6, description: 'Jumlah kelas' })
  @IsOptional()
  @IsInt()
  @Min(0)
  classCount?: number;

  @ApiPropertyOptional({ example: '10000000', description: 'Pagu BOS (Rp)' })
  @IsOptional()
  @IsInt()
  @Min(0)
  bosPagu?: number;

  @ApiPropertyOptional({ example: '9500000', description: 'Realisasi BOS (Rp)' })
  @IsOptional()
  @IsInt()
  @Min(0)
  bosRealisasi?: number;

  @ApiPropertyOptional({ example: 'BOS 2026 Tahap 1', description: 'Label periode BOS' })
  @IsOptional()
  @IsString()
  @MaxLength(40)
  bosPeriodLabel?: string;

  @ApiPropertyOptional({ enum: ErpSchoolBosStage, example: ErpSchoolBosStage.TAHAP_1 })
  @IsOptional()
  bosPeriodStage?: ErpSchoolBosStage;

  @ApiPropertyOptional({ example: 2026, description: 'Tahun anggaran BOS' })
  @IsOptional()
  @IsInt()
  @Min(1900)
  @Max(2100)
  bosPeriodYear?: number;

  @ApiPropertyOptional({ example: 'Kontrak 3 tahun, berakhir 2027-06-30' })
  @IsOptional()
  @IsString()
  negotiationNotes?: string;

  @ApiPropertyOptional({ example: '2026-09-15', description: 'Kunjungan terakhir' })
  @IsOptional()
  @IsString()
  lastVisitAt?: string;

  @ApiPropertyOptional({ example: 'Sekolah minta contoh LKS sebelum memutuskan' })
  @IsOptional()
  @IsString()
  visitNotes?: string;

  @ApiPropertyOptional({ enum: ErpSchoolPipelineStage, example: ErpSchoolPipelineStage.PROSPEK })
  @IsOptional()
  pipelineStage?: ErpSchoolPipelineStage;

  @ApiPropertyOptional({
    example: { '1': 32, '2': 30, '3': 28 },
    description: 'Jumlah siswa per tingkat kelas (kunci = label kelas, mis. "1".."6" untuk SD)',
  })
  @IsOptional()
  @IsObject()
  studentsPerGrade?: Record<string, number>;

  @ApiPropertyOptional({ type: [SchoolContactInputDto], description: 'Kontak sekolah (kepala sekolah, bendahara, operator, TU, …)' })
  @IsOptional()
  @ValidateNested({ each: true })
  @Type(() => SchoolContactInputDto)
  contacts?: SchoolContactInputDto[];

  @ApiPropertyOptional({ type: SchoolNewActivityDto, description: 'Satu aktivitas awal untuk log kunjungan/negosiasi' })
  @IsOptional()
  @ValidateNested()
  @Type(() => SchoolNewActivityDto)
  newActivity?: SchoolNewActivityDto;

  @ApiPropertyOptional({ example: '2027-06-30', description: 'Expired kontrak' })
  @IsOptional()
  @IsString()
  contractExpiryAt?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}
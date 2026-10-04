import { ApiPropertyOptional } from '@nestjs/swagger';
import { Transform, Type } from 'class-transformer';
import { IsBoolean, IsEnum, IsIn, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpSchoolBosStage, ErpSchoolJenjang, ErpSchoolNegeriSwasta, ErpSchoolPipelineStage } from '@prisma/client';

const SORTABLE_FIELDS = ['code', 'name', 'npsn', 'contractExpiryAt', 'createdAt'] as const;
type SortableField = (typeof SORTABLE_FIELDS)[number];

export class QueryErpSchoolDto {
  @ApiPropertyOptional({ example: 1, default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ example: 10, default: 10 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(100)
  limit?: number = 10;

  @ApiPropertyOptional({ example: 'negeri' })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: ErpSchoolJenjang })
  @IsOptional()
  @IsEnum(ErpSchoolJenjang)
  jenjang?: ErpSchoolJenjang;

  @ApiPropertyOptional({ enum: ErpSchoolNegeriSwasta })
  @IsOptional()
  @IsEnum(ErpSchoolNegeriSwasta)
  negeriSwasta?: ErpSchoolNegeriSwasta;

  @ApiPropertyOptional({ enum: ErpSchoolBosStage })
  @IsOptional()
  @IsEnum(ErpSchoolBosStage)
  bosStage?: ErpSchoolBosStage;

  @ApiPropertyOptional({ enum: ErpSchoolPipelineStage })
  @IsOptional()
  @IsEnum(ErpSchoolPipelineStage)
  pipelineStage?: ErpSchoolPipelineStage;

  @ApiPropertyOptional({
    enum: ['NOT_ORDERED', 'CONTRACT_EXPIRING'],
    description: 'Filter peringatan: belum belanja pada tahun BOS (bosYear / tahun berjalan) / kontrak berakhir ≤ 90 hari',
  })
  @IsOptional()
  @IsIn(['NOT_ORDERED', 'CONTRACT_EXPIRING'])
  alert?: 'NOT_ORDERED' | 'CONTRACT_EXPIRING';

  @ApiPropertyOptional({ example: 2026 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  bosYear?: number;

  @ApiPropertyOptional({ example: true })
  @IsOptional()
  @Transform(({ value }) => {
    if (value === 'true') return true;
    if (value === 'false') return false;
    return value;
  })
  @IsBoolean()
  isActive?: boolean;

  @ApiPropertyOptional({ example: 'code', enum: SORTABLE_FIELDS })
  @IsOptional()
  @IsIn(SORTABLE_FIELDS)
  sortBy?: SortableField;

  @ApiPropertyOptional({ example: 'asc', enum: ['asc', 'desc'] })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc';
}
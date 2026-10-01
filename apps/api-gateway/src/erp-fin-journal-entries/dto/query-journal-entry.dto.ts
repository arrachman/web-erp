import { ApiPropertyOptional } from '@nestjs/swagger';
import { Transform, Type } from 'class-transformer';
import {
  IsBoolean,
  IsEnum,
  IsIn,
  IsInt,
  IsOptional,
  IsString,
  Max,
  Min,
} from 'class-validator';
import {
  ErpDocumentStatusDto,
  ErpJournalTypeDto,
} from './create-journal-entry.dto';

const SORTABLE = ['entryDate', 'docNumber', 'status', 'createdAt', 'updatedAt'] as const;

export class QueryJournalEntryDto {
  @ApiPropertyOptional({ default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ default: 10 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(100)
  limit?: number = 10;

  @ApiPropertyOptional({
    default: true,
    description: 'false = skip COUNT(*), return hasMore only',
  })
  @IsOptional()
  @Transform(({ value }) => {
    if (value === 'false' || value === false) return false;
    if (value === 'true' || value === true) return true;
    return value;
  })
  @IsBoolean()
  includeTotal?: boolean = true;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: ErpJournalTypeDto })
  @IsOptional()
  @IsEnum(ErpJournalTypeDto)
  journalType?: ErpJournalTypeDto;

  @ApiPropertyOptional({ enum: ErpDocumentStatusDto })
  @IsOptional()
  @IsEnum(ErpDocumentStatusDto)
  status?: ErpDocumentStatusDto;

  @ApiPropertyOptional() @IsOptional() @IsString() branchId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() partnerId?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() createdById?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() dateFrom?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() dateTo?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() docNumberFrom?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() docNumberTo?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() description?: string;
  @ApiPropertyOptional() @IsOptional() @IsString() notes?: string;

  @ApiPropertyOptional({ enum: SORTABLE, default: 'entryDate' })
  @IsOptional()
  @IsIn(SORTABLE as unknown as string[])
  sortBy?: string;

  @ApiPropertyOptional({ enum: ['asc', 'desc'], default: 'desc' })
  @IsOptional()
  @IsIn(['asc', 'desc'])
  sortDir?: 'asc' | 'desc';
}

import { ApiPropertyOptional } from '@nestjs/swagger';
import { Transform, Type } from 'class-transformer';
import { IsBoolean, IsInt, IsOptional, Max, Min } from 'class-validator';

/**
 * Shared list pagination fields. Mix into resource query DTOs via extends
 * or re-declare with the same validators so ValidationPipe stays consistent.
 *
 * - `includeTotal` defaults true (backward compatible). Set false to skip
 *   COUNT(*) and return hasMore instead — cheaper on large filtered sets.
 */
export class QueryPaginationDto {
  @ApiPropertyOptional({ default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ default: 10, maximum: 100 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(100)
  limit?: number = 10;

  @ApiPropertyOptional({
    default: true,
    description:
      'When false, skip COUNT(*) and return meta.hasMore (total approximate).',
  })
  @IsOptional()
  @Transform(({ value }) => {
    if (value === 'false' || value === false) return false;
    if (value === 'true' || value === true) return true;
    return value;
  })
  @IsBoolean()
  includeTotal?: boolean = true;
}

export interface ListMeta {
  page: number;
  limit: number;
  total: number;
  totalPages: number;
  hasMore?: boolean;
  totalExact?: boolean;
}

export function clampPageLimit(
  page?: number,
  limit?: number,
  maxLimit = 100,
): { page: number; limit: number; skip: number } {
  const p = Math.max(1, page ?? 1);
  const l = Math.min(Math.max(1, limit ?? 10), maxLimit);
  return { page: p, limit: l, skip: (p - 1) * l };
}

export function buildListMeta(args: {
  page: number;
  limit: number;
  total?: number | null;
  /** Row count actually returned (for hasMore when total skipped). */
  rowCount: number;
  includeTotal?: boolean;
}): ListMeta {
  const includeTotal = args.includeTotal !== false;
  if (includeTotal && args.total != null) {
    return {
      page: args.page,
      limit: args.limit,
      total: args.total,
      totalPages: Math.ceil(args.total / args.limit) || 1,
      hasMore: args.page * args.limit < args.total,
      totalExact: true,
    };
  }
  const hasMore = args.rowCount >= args.limit;
  // Approximate: at least the rows we've walked; +1 page if hasMore.
  const total = (args.page - 1) * args.limit + args.rowCount + (hasMore ? 1 : 0);
  return {
    page: args.page,
    limit: args.limit,
    total,
    totalPages: hasMore ? args.page + 1 : args.page,
    hasMore,
    totalExact: false,
  };
}

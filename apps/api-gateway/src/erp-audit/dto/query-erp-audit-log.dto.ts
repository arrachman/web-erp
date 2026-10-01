import { IsEnum, IsInt, IsOptional, IsString, Min } from 'class-validator';
import { Type } from 'class-transformer';
import { ApiPropertyOptional } from '@nestjs/swagger';
import { ErpAuditAction } from '@prisma/client';

export class QueryErpAuditLogDto {
  @ApiPropertyOptional({ description: 'Filter by entity name, e.g. ErpItem' })
  @IsOptional()
  @IsString()
  entityName?: string;

  @ApiPropertyOptional({ description: 'Filter by entity primary key (as string)' })
  @IsOptional()
  @IsString()
  entityId?: string;

  @ApiPropertyOptional({ enum: ErpAuditAction })
  @IsOptional()
  @IsEnum(ErpAuditAction)
  action?: ErpAuditAction;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  actorId?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  from?: string;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  to?: string;

  @ApiPropertyOptional({ default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ default: 20 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  limit?: number = 20;
}

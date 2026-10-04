import { Type } from 'class-transformer';
import { IsInt, IsOptional, IsString, Max, Min } from 'class-validator';

export class QueryPurRebateDto {
  @IsOptional() @Type(() => Number) @IsInt() @Min(1) page?: number;
  @IsOptional() @Type(() => Number) @IsInt() @Min(1) @Max(200) limit?: number;
  @IsOptional() @IsString() search?: string;
  @IsOptional() @Type(() => Number) @IsInt() periodYear?: number;
  @IsOptional() @Type(() => Number) @IsInt() supplierId?: number;
}

import { Transform } from 'class-transformer';
import { IsBoolean, IsIn, IsInt, IsOptional, IsString, Min } from 'class-validator';

export class QueryErpReportDto {
  @IsInt() @Min(1) @Transform(({ value }) => parseInt(value, 10)) @IsOptional() page?: number = 1;
  @IsInt() @Min(1) @Transform(({ value }) => parseInt(value, 10)) @IsOptional() limit?: number = 20;
  @IsString() @IsOptional() search?: string;
  @IsString() @IsOptional() module?: string;
  @IsString() @IsOptional() reportKey?: string;
  @Transform(({ value }) => value === 'true' ? true : value === 'false' ? false : undefined)
  @IsBoolean() @IsOptional() isActive?: boolean;
  @IsString() @IsIn(['code', 'name', 'module', 'createdAt']) @IsOptional() sortBy?: string = 'createdAt';
  @IsString() @IsIn(['asc', 'desc']) @IsOptional() sortDir?: 'asc' | 'desc' = 'desc';
}

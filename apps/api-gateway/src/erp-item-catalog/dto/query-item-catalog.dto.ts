import { Type } from 'class-transformer';
import { IsEnum, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpItemCurriculum, ErpSchoolJenjang, ErpStockClass } from '@prisma/client';

export class QueryItemCatalogDto {
  @IsOptional() @Type(() => Number) @IsInt() @Min(1) page?: number;
  @IsOptional() @Type(() => Number) @IsInt() @Min(1) @Max(200) limit?: number;
  @IsOptional() @IsString() search?: string;
  @IsOptional() @IsEnum(ErpSchoolJenjang) jenjang?: ErpSchoolJenjang;
  @IsOptional() @IsEnum(ErpItemCurriculum) curriculum?: ErpItemCurriculum;
  @IsOptional() @IsEnum(ErpStockClass) stockClass?: ErpStockClass;
  @IsOptional() @Type(() => Number) @IsInt() categoryId?: number;
}

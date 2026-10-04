import {
  IsBoolean,
  IsEnum,
  IsNumber,
  IsObject,
  IsOptional,
  IsString,
  MaxLength,
} from 'class-validator';
import { ErpItemCurriculum, ErpSchoolJenjang, ErpStockClass } from '@prisma/client';

/** D1 — school-catalog attributes for one item (all optional; PUT upserts). */
export class UpsertItemCatalogDto {
  @IsOptional() @IsString() @MaxLength(200) publisherName?: string;
  @IsOptional() @IsEnum(ErpSchoolJenjang) jenjang?: ErpSchoolJenjang;
  @IsOptional() @IsString() @MaxLength(50) gradeLevel?: string;
  @IsOptional() @IsEnum(ErpItemCurriculum) curriculum?: ErpItemCurriculum;
  @IsOptional() @IsString() @MaxLength(100) subject?: string;
  @IsOptional() @IsNumber() hetPrice?: number;
  @IsOptional() @IsBoolean() isCustomPrint?: boolean;
  @IsOptional() @IsEnum(ErpStockClass) stockClass?: ErpStockClass;
  @IsOptional() @IsObject() channels?: Record<string, boolean>;
  @IsOptional() @IsBoolean() isActive?: boolean;
}

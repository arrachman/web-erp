import { Type } from 'class-transformer';
import { IsInt, IsNumber, IsOptional, IsString, Max, MaxLength, Min } from 'class-validator';

export class CreatePurRebateDto {
  @Type(() => Number) @IsInt() supplierId!: number;
  @IsOptional() @Type(() => Number) @IsInt() categoryId?: number;
  @Type(() => Number) @IsNumber() @Min(0) @Max(100) percent!: number;
  @Type(() => Number) @IsInt() @Min(2000) @Max(2100) periodYear!: number;
  @IsOptional() @IsString() @MaxLength(500) notes?: string;
}

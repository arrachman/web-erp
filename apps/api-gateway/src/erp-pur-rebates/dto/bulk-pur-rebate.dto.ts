import { Type } from 'class-transformer';
import { IsArray, IsBoolean, IsNumber } from 'class-validator';

export class BulkRebateIdsDto {
  @IsArray() @Type(() => Number) @IsNumber({}, { each: true }) ids!: number[];
}

export class BulkRebateStatusDto extends BulkRebateIdsDto {
  @IsBoolean() isActive!: boolean;
}

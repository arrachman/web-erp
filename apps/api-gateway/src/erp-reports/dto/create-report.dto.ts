import { IsBoolean, IsNotEmpty, IsObject, IsOptional, IsString } from 'class-validator';

export class CreateErpReportDto {
  @IsString() @IsNotEmpty() code!: string;
  @IsString() @IsNotEmpty() name!: string;
  @IsString() @IsNotEmpty() module!: string;
  @IsString() @IsOptional() description?: string;
  @IsObject() @IsOptional() templateJson?: Record<string, unknown>;
  // Binds this template to a report (`<module>.<report>`, e.g. `fin.trial-balance`).
  @IsString() @IsOptional() reportKey?: string;
  @IsBoolean() @IsOptional() isActive?: boolean;
}

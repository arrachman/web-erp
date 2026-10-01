import { IsNotEmpty, IsObject, IsOptional, IsString } from 'class-validator';

export class ExecuteSqlDto {
  @IsString() @IsNotEmpty() sql!: string;
  @IsObject() @IsOptional() params?: Record<string, unknown>;
  @IsOptional() limit?: number;
}

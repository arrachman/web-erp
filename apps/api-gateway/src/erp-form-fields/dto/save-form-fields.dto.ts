import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsArray, IsBoolean, IsIn, IsInt, IsObject, IsOptional, IsString, Min, ValidateNested,
} from 'class-validator';

export const FORM_FIELD_KINDS = ['STRUCTURAL', 'CUSTOM'] as const;
export const FORM_FIELD_TYPES = [
  'TEXT', 'NUMBER', 'DATE',
  'PARTNER', 'ACCOUNT', 'BRANCH', 'LOCATION', 'CURRENCY', 'LOOKUP',
] as const;
export const FORM_COLUMN_SLOTS = ['LEFT', 'CENTER', 'RIGHT'] as const;

export class FormFieldInputDto {
  @ApiProperty({ example: 'partnerId' }) @IsString() fieldKey!: string;
  @ApiProperty({ enum: FORM_FIELD_KINDS }) @IsIn(FORM_FIELD_KINDS) kind!: (typeof FORM_FIELD_KINDS)[number];
  @ApiProperty({ example: 'Terima Dari' }) @IsString() label!: string;
  @ApiProperty({ enum: FORM_FIELD_TYPES }) @IsIn(FORM_FIELD_TYPES) fieldType!: (typeof FORM_FIELD_TYPES)[number];
  @ApiPropertyOptional({ example: 'partners' }) @IsOptional() @IsString() lookupSource?: string;
  @ApiPropertyOptional({ example: { typeKind: 'CUSTOMER' } }) @IsOptional() @IsObject() lookupDefaultFilter?: Record<string, unknown>;
  @ApiPropertyOptional({ example: 'name:asc' }) @IsOptional() @IsString() lookupDefaultSort?: string;
  @ApiPropertyOptional({ example: 'Pilih akun kas/bank…' }) @IsOptional() @IsString() placeholder?: string;
  @ApiPropertyOptional({ example: '1' }) @IsOptional() @IsString() defaultValue?: string;
  @ApiPropertyOptional({ example: false }) @IsOptional() @IsBoolean() isReadonly?: boolean;
  @ApiProperty({ example: false }) @IsBoolean() isRequired!: boolean;
  @ApiProperty({ example: true }) @IsBoolean() isVisible!: boolean;
  @ApiProperty({ example: 0 }) @IsInt() @Min(0) sortOrder!: number;
  @ApiProperty({ enum: FORM_COLUMN_SLOTS }) @IsIn(FORM_COLUMN_SLOTS) columnSlot!: (typeof FORM_COLUMN_SLOTS)[number];
}

export class SaveFormFieldsDto {
  @ApiProperty({ type: [FormFieldInputDto] })
  @IsArray() @ValidateNested({ each: true }) @Type(() => FormFieldInputDto)
  fields!: FormFieldInputDto[];
}

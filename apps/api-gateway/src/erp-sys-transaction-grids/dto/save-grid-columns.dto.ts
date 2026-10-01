import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsArray, IsBoolean, IsIn, IsInt, IsObject, IsOptional, IsString, Min, ValidateNested,
} from 'class-validator';

export const GRID_COLUMN_KINDS = ['STANDARD', 'CUSTOM'] as const;
export const GRID_DATA_TYPES = ['TEXT', 'NUMBER', 'DATE', 'LOOKUP'] as const;
// Presentation/edit slots — app-level allowlist (kept TEXT in DB; mirrored by FE dropdowns).
export const LABEL_FORMATTERS = ['NONE', 'NUMBER', 'DECIMAL', 'CURRENCY', 'PERCENT', 'DATE', 'DATETIME', 'BOOLEAN'] as const;
export const HEADER_RENDERERS = ['DEFAULT', 'REQUIRED', 'CENTER', 'WRAP', 'HELP'] as const;
export const CELL_RENDERERS = ['TEXT', 'NUMERIC', 'CURRENCY', 'BADGE', 'CHECK', 'LINK', 'LOOKUP'] as const;
export const CELL_EDITORS = [
  'TEXT', 'NUMBER', 'DATE', 'LOOKUP', 'TEXTAREA', 'CHECKBOX', 'NONE',
  'DISCOUNT', 'STEPPER', 'COMBOBOX', 'ACCOUNT_PICKER', 'PARTNER_PICKER',
  'ROWNUM',
] as const;
export const COLUMN_TYPES = [
  'text', 'number', 'rownum', 'currency', 'decimal', 'percent',
  'date', 'datetime', 'checkbox', 'lookup', 'textarea',
  'badge', 'link', 'discount', 'stepper', 'combobox',
  'account_picker', 'partner_picker',
] as const;

export class GridColumnInputDto {
  @ApiProperty({ example: 0 }) @IsInt() @Min(0) sortOrder!: number;
  @ApiProperty({ example: 'Akun (No · Nama)' }) @IsString() headerText!: string;
  @ApiProperty({ example: 'accountId' }) @IsString() dataField!: string;
  @ApiProperty({ example: 160 }) @IsInt() @Min(0) width!: number;
  @ApiProperty({ example: true }) @IsBoolean() isVisible!: boolean;
  @ApiProperty({ example: false }) @IsBoolean() isRequired!: boolean;
  @ApiProperty({ example: true }) @IsBoolean() isEditable!: boolean;
  @ApiPropertyOptional({ example: false }) @IsOptional() @IsBoolean() isSkippable?: boolean;
  @ApiProperty({ enum: GRID_COLUMN_KINDS }) @IsIn(GRID_COLUMN_KINDS) kind!: (typeof GRID_COLUMN_KINDS)[number];
  @ApiProperty({ enum: GRID_DATA_TYPES }) @IsIn(GRID_DATA_TYPES) dataType!: (typeof GRID_DATA_TYPES)[number];
  @ApiPropertyOptional({ example: 'accounts' }) @IsOptional() @IsString() lookupSource?: string;
  @ApiPropertyOptional({ example: { isActive: true } }) @IsOptional() @IsObject() lookupDefaultFilter?: Record<string, unknown>;
  @ApiPropertyOptional({ example: 'name:asc' }) @IsOptional() @IsString() lookupDefaultSort?: string;
  @ApiPropertyOptional({ example: 'Pilih satuan…' }) @IsOptional() @IsString() placeholder?: string;
  @ApiPropertyOptional({ example: '12' }) @IsOptional() @IsString() defaultValue?: string;
  @ApiPropertyOptional({ example: 'PCS - Pieces' }) @IsOptional() @IsString() defaultValueLabel?: string;
  @ApiPropertyOptional({ enum: LABEL_FORMATTERS }) @IsOptional() @IsIn(LABEL_FORMATTERS) labelFormatter?: (typeof LABEL_FORMATTERS)[number];
  @ApiPropertyOptional({ enum: HEADER_RENDERERS }) @IsOptional() @IsIn(HEADER_RENDERERS) headerRenderer?: (typeof HEADER_RENDERERS)[number];
  @ApiPropertyOptional({ enum: CELL_RENDERERS }) @IsOptional() @IsIn(CELL_RENDERERS) cellRenderer?: (typeof CELL_RENDERERS)[number];
  @ApiPropertyOptional({ enum: CELL_EDITORS }) @IsOptional() @IsIn(CELL_EDITORS) cellEditor?: (typeof CELL_EDITORS)[number];
  @ApiPropertyOptional({ enum: COLUMN_TYPES }) @IsOptional() @IsIn(COLUMN_TYPES) columnType?: (typeof COLUMN_TYPES)[number];
}

export class GridInputDto {
  @ApiProperty({ example: 'main' }) @IsString() key!: string;
  @ApiProperty({ example: 'Utama' }) @IsString() label!: string;
  @ApiProperty({ example: 0 }) @IsInt() @Min(0) sortOrder!: number;
  @ApiPropertyOptional({ example: 'fin_cash_bank_lines' }) @IsOptional() @IsString() lineTable?: string;
  @ApiPropertyOptional({ example: true }) @IsOptional() @IsBoolean() isPrimary?: boolean;
  @ApiProperty({ type: [GridColumnInputDto] })
  @IsArray() @ValidateNested({ each: true }) @Type(() => GridColumnInputDto)
  columns!: GridColumnInputDto[];
}

export class SaveGridsDto {
  @ApiProperty({ type: [GridInputDto] })
  @IsArray() @ValidateNested({ each: true }) @Type(() => GridInputDto)
  grids!: GridInputDto[];
}

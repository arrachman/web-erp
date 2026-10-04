import { Type } from 'class-transformer';
import {
  IsArray,
  IsBoolean,
  IsOptional,
  IsString,
  MaxLength,
  ValidateNested,
} from 'class-validator';
import { IsDecimalString } from '../../erp-common/decorators/is-decimal-string.decorator';

export class PackingStudentDto {
  @IsString()
  @MaxLength(200)
  studentName!: string;

  @IsOptional()
  @IsString()
  @MaxLength(100)
  className?: string;
}

export class PackingContentItemDto {
  @IsString()
  itemId!: string;

  @IsDecimalString()
  quantity!: string;
}

export class GeneratePackingUnitsDto {
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => PackingStudentDto)
  students?: PackingStudentDto[];

  /** Alternatif: teks roster, satu siswa per baris "Nama,Kelas" (atau "Nama;Kelas"). */
  @IsOptional()
  @IsString()
  rosterCsv?: string;

  /** Isi per unit; bawaan: snapshot baris Packing List. */
  @IsOptional()
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => PackingContentItemDto)
  items?: PackingContentItemDto[];
}

export class SetPackingUnitPackedDto {
  @IsBoolean()
  packed!: boolean;
}

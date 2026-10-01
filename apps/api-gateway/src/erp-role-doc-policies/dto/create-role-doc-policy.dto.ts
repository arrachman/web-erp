import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import {
  ArrayMinSize,
  IsArray,
  IsBoolean,
  IsIn,
  IsOptional,
  IsString,
  MaxLength,
} from 'class-validator';

/** Creation statuses available for selection when making a new document. */
export const CREATION_STATUSES = [
  'DRAFT', 'NEED_APPROVE',
  'APPROVE_1', 'APPROVE_2', 'APPROVE_3', 'APPROVE_4',
  'APPROVED', 'REJECTED',
] as const;
export type CreationStatus = (typeof CREATION_STATUSES)[number];

export class CreateRoleDocPolicyDto {
  @ApiProperty({ example: '3', description: 'adm_roles.id' })
  @IsString()
  roleId!: string;

  @ApiProperty({ example: 'CASH_RECEIPT', description: 'Document type code' })
  @IsString()
  @MaxLength(60)
  documentType!: string;

  @ApiProperty({
    example: ['DRAFT', 'NEED_APPROVE', 'APPROVED'],
    description: 'Statuses the role may select on document creation',
    isArray: true,
  })
  @IsArray()
  @ArrayMinSize(1)
  @IsIn(CREATION_STATUSES, { each: true })
  allowedStatuses!: string[];

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

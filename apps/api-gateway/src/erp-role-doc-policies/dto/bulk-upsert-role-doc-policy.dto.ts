import { ApiProperty } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsArray, IsIn, IsNotEmpty, IsString, ValidateNested } from 'class-validator';
import { CREATION_STATUSES } from './create-role-doc-policy.dto';

export class BulkEntryDto {
  @ApiProperty({ example: 'CASH_RECEIPT' })
  @IsString()
  @IsNotEmpty()
  documentType!: string;

  @ApiProperty({ example: ['DRAFT', 'NEED_APPROVE'], isArray: true })
  @IsArray()
  @IsIn(CREATION_STATUSES, { each: true })
  allowedStatuses!: string[];
}

export class BulkUpsertRoleDocPolicyDto {
  @ApiProperty({ example: '3', description: 'adm_roles.id' })
  @IsString()
  @IsNotEmpty()
  roleId!: string;

  @ApiProperty({ type: [BulkEntryDto] })
  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => BulkEntryDto)
  entries!: BulkEntryDto[];
}

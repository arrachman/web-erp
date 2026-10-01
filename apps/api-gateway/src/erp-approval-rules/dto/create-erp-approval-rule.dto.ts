import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import {
  IsBoolean,
  IsInt,
  IsOptional,
  IsString,
  MaxLength,
  Min,
} from 'class-validator';

export class CreateErpApprovalRuleDto {
  @ApiProperty({ example: 'PUR.PO', description: 'Document type code the rule applies to' })
  @IsString()
  @MaxLength(60)
  documentType!: string;

  @ApiProperty({ example: 'Persetujuan PO di atas 10 juta' })
  @IsString()
  @MaxLength(160)
  name!: string;

  @ApiPropertyOptional({ example: 1, default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  level?: number = 1;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  requiresApproval?: boolean = true;

  @ApiPropertyOptional({ example: '10000000', description: 'Threshold — rule applies when amount >= this' })
  @IsOptional()
  @IsString()
  minAmount?: string;

  @ApiPropertyOptional({ example: '5', description: 'Approver role id' })
  @IsOptional()
  @IsString()
  approverRoleId?: string;

  @ApiPropertyOptional({ example: 'Catatan tambahan' })
  @IsOptional()
  @IsString()
  notes?: string;

  @ApiPropertyOptional({ example: true, default: true })
  @IsOptional()
  @IsBoolean()
  isActive?: boolean = true;
}

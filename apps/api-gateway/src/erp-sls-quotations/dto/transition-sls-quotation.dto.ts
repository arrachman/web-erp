import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions over the Senti state machine (§2.7). */
export enum SlsQuotationTransitionAction {
  SUBMIT = 'SUBMIT', // DRAFT|REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT', // NEED_APPROVE -> REJECTED
  POST = 'POST', // APPROVED -> POSTED (+ generate ledger entries)
  REOPEN = 'REOPEN', // APPROVED -> DRAFT
}

export class TransitionSlsQuotationDto {
  @ApiProperty({ enum: SlsQuotationTransitionAction })
  @IsEnum(SlsQuotationTransitionAction)
  action!: SlsQuotationTransitionAction;

  @ApiPropertyOptional({ description: 'Required for REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}

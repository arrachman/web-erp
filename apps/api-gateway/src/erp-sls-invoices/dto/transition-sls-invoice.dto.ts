import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions over the Senti state machine (§2.7). */
export enum SlsInvoiceTransitionAction {
  SUBMIT = 'SUBMIT', // DRAFT|REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT', // NEED_APPROVE -> REJECTED
  POST = 'POST', // APPROVED -> POSTED (+ create AR ledger entry)
  REOPEN = 'REOPEN', // APPROVED -> DRAFT (reverse AR)
}

export class TransitionSlsInvoiceDto {
  @ApiProperty({ enum: SlsInvoiceTransitionAction })
  @IsEnum(SlsInvoiceTransitionAction)
  action!: SlsInvoiceTransitionAction;

  @ApiPropertyOptional({ description: 'Required for REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}

import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions over the Senti state machine (§2.7). */
export enum SlsInvoiceTransitionAction {
  SUBMIT = 'SUBMIT', // DRAFT|REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT', // NEED_APPROVE -> REJECTED
  POST = 'POST', // APPROVED -> POSTED (+ create AR ledger entry)
  REOPEN = 'REOPEN', // APPROVED -> DRAFT (reverse AR)
  VOID = 'VOID', // POSTED -> VOID (dated reversing journal, alasan wajib)
}

export class TransitionSlsInvoiceDto {
  @ApiProperty({ enum: SlsInvoiceTransitionAction })
  @IsEnum(SlsInvoiceTransitionAction)
  action!: SlsInvoiceTransitionAction;

  @ApiPropertyOptional({ description: 'Required for REJECT and VOID' })
  @IsOptional()
  @IsString()
  reason?: string;
}

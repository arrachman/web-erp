import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions over the Senti state machine (§2.7). */
export enum ApPaymentTransitionAction {
  SUBMIT = 'SUBMIT', // DRAFT|REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT', // NEED_APPROVE -> REJECTED
  POST = 'POST', // APPROVED -> POSTED (+ write ledger + allocate to invoices)
  REOPEN = 'REOPEN', // POSTED|APPROVED -> DRAFT (reverse ledger + un-allocate)
}

export class TransitionApPaymentDto {
  @ApiProperty({ enum: ApPaymentTransitionAction })
  @IsEnum(ApPaymentTransitionAction)
  action!: ApPaymentTransitionAction;

  @ApiPropertyOptional({ description: 'Required for REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}

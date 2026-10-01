import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions for Vendor Payment Plan (VPP) state machine.
 *  DRAFT → NEED_APPROVE → APPROVED → POSTED
 *  NEED_APPROVE → REJECTED → NEED_APPROVE (SUBMIT)
 *  APPROVED → DRAFT (REOPEN)
 */
export enum PaymentScheduleTransitionAction {
  SUBMIT = 'SUBMIT',   // DRAFT | REJECTED → NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE → APPROVED
  REJECT = 'REJECT',   // NEED_APPROVE → REJECTED
  POST = 'POST',       // APPROVED → POSTED
  REOPEN = 'REOPEN',  // APPROVED → DRAFT
}

export class TransitionPaymentScheduleDto {
  @ApiProperty({ enum: PaymentScheduleTransitionAction })
  @IsEnum(PaymentScheduleTransitionAction)
  action!: PaymentScheduleTransitionAction;

  @ApiPropertyOptional({ description: 'Required when action is REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}

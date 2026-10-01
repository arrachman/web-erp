import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions for Freight Payable state machine.
 *  DRAFT → NEED_APPROVE → APPROVED → POSTED
 *  NEED_APPROVE → REJECTED → NEED_APPROVE (via SUBMIT)
 *  APPROVED → DRAFT (via REOPEN)
 */
export enum FreightPayableTransitionAction {
  SUBMIT = 'SUBMIT',   // DRAFT | REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT',   // NEED_APPROVE -> REJECTED
  POST = 'POST',       // APPROVED -> POSTED
  REOPEN = 'REOPEN',  // APPROVED -> DRAFT
}

export class TransitionFreightPayableDto {
  @ApiProperty({ enum: FreightPayableTransitionAction })
  @IsEnum(FreightPayableTransitionAction)
  action!: FreightPayableTransitionAction;

  @ApiPropertyOptional({ description: 'Required when action is REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}

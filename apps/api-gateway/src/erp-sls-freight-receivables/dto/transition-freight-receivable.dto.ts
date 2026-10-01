import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions for Freight Receivable state machine (§2.7). */
export enum FreightReceivableTransitionAction {
  SUBMIT = 'SUBMIT',   // DRAFT | REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT',   // NEED_APPROVE -> REJECTED
  POST = 'POST',       // APPROVED -> POSTED
  REOPEN = 'REOPEN',   // APPROVED | POSTED -> DRAFT
}

export class TransitionFreightReceivableDto {
  @ApiProperty({ enum: FreightReceivableTransitionAction })
  @IsEnum(FreightReceivableTransitionAction)
  action!: FreightReceivableTransitionAction;

  @ApiPropertyOptional({ description: 'Required when action is REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}

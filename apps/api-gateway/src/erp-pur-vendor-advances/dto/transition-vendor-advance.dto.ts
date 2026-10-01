import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

/** Workflow actions for Vendor Advance state machine. */
export enum VendorAdvanceTransitionAction {
  SUBMIT = 'SUBMIT',   // DRAFT | REJECTED -> NEED_APPROVE
  APPROVE = 'APPROVE', // NEED_APPROVE -> APPROVED
  REJECT = 'REJECT',   // NEED_APPROVE -> REJECTED
  POST = 'POST',       // APPROVED -> POSTED
  REOPEN = 'REOPEN',  // APPROVED -> DRAFT
}

export class TransitionVendorAdvanceDto {
  @ApiProperty({ enum: VendorAdvanceTransitionAction })
  @IsEnum(VendorAdvanceTransitionAction)
  action!: VendorAdvanceTransitionAction;

  @ApiPropertyOptional({ description: 'Required when action is REJECT' })
  @IsOptional()
  @IsString()
  reason?: string;
}

import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { IsEnum, IsOptional, IsString } from 'class-validator';

export enum PurRfqTransitionAction {
  SUBMIT = 'SUBMIT', APPROVE = 'APPROVE', REJECT = 'REJECT', POST = 'POST', REOPEN = 'REOPEN',
}

export class TransitionPurRfqDto {
  @ApiProperty({ enum: PurRfqTransitionAction }) @IsEnum(PurRfqTransitionAction) action!: PurRfqTransitionAction;
  @ApiPropertyOptional() @IsOptional() @IsString() reason?: string;
}

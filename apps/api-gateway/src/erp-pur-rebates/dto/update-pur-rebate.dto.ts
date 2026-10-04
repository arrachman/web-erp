import { PartialType } from '@nestjs/swagger';
import { IsBoolean, IsOptional } from 'class-validator';
import { CreatePurRebateDto } from './create-pur-rebate.dto';

export class UpdatePurRebateDto extends PartialType(CreatePurRebateDto) {
  @IsOptional() @IsBoolean() isActive?: boolean;
}

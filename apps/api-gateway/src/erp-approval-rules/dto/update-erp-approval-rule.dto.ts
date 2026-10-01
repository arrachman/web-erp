import { PartialType } from '@nestjs/swagger';
import { CreateErpApprovalRuleDto } from './create-erp-approval-rule.dto';

export class UpdateErpApprovalRuleDto extends PartialType(CreateErpApprovalRuleDto) {}

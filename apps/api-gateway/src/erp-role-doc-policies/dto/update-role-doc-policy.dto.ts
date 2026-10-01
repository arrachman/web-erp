import { PartialType } from '@nestjs/swagger';
import { CreateRoleDocPolicyDto } from './create-role-doc-policy.dto';

export class UpdateRoleDocPolicyDto extends PartialType(CreateRoleDocPolicyDto) {}

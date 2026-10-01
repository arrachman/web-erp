import { PartialType } from '@nestjs/swagger';
import { CreateErpRoleDto } from './create-erp-role.dto';

export class UpdateErpRoleDto extends PartialType(CreateErpRoleDto) {}

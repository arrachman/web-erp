import { PartialType } from '@nestjs/swagger';
import { CreateErpSubDepartmentDto } from './create-erp-sub-department.dto';

export class UpdateErpSubDepartmentDto extends PartialType(CreateErpSubDepartmentDto) {}

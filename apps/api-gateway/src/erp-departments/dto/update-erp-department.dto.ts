import { PartialType } from '@nestjs/swagger';
import { CreateErpDepartmentDto } from './create-erp-department.dto';

export class UpdateErpDepartmentDto extends PartialType(CreateErpDepartmentDto) {}

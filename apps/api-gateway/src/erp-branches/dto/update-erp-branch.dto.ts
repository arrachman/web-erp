import { PartialType } from '@nestjs/swagger';
import { CreateErpBranchDto } from './create-erp-branch.dto';

export class UpdateErpBranchDto extends PartialType(CreateErpBranchDto) {}

import { PartialType } from '@nestjs/swagger';
import { CreateErpProjectDto } from './create-erp-project.dto';

export class UpdateErpProjectDto extends PartialType(CreateErpProjectDto) {}

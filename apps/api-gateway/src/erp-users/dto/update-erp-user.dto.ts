import { PartialType } from '@nestjs/swagger';
import { CreateErpUserDto } from './create-erp-user.dto';

export class UpdateErpUserDto extends PartialType(CreateErpUserDto) {}

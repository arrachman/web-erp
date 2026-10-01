import { PartialType } from '@nestjs/swagger';
import { CreateErpSysMenuDto } from './create-erp-sys-menu.dto';

export class UpdateErpSysMenuDto extends PartialType(CreateErpSysMenuDto) {}

import { PartialType } from '@nestjs/swagger';
import { CreateErpAccountDto } from './create-erp-account.dto';

export class UpdateErpAccountDto extends PartialType(CreateErpAccountDto) {}

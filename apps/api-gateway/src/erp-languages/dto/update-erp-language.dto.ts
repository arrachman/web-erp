import { PartialType } from '@nestjs/swagger';
import { CreateErpLanguageDto } from './create-erp-language.dto';

export class UpdateErpLanguageDto extends PartialType(CreateErpLanguageDto) {}

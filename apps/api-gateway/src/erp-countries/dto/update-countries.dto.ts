import { PartialType } from '@nestjs/swagger';
import { CreateErpCountryDto } from './create-countries.dto';

export class UpdateErpCountryDto extends PartialType(CreateErpCountryDto) {}

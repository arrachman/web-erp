import { PartialType } from '@nestjs/swagger';
import { CreateErpCityDto } from './create-cities.dto';

export class UpdateErpCityDto extends PartialType(CreateErpCityDto) {}

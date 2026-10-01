import { PartialType } from '@nestjs/swagger';
import { CreateErpPriceIndexDto } from './create-price-index.dto';

export class UpdateErpPriceIndexDto extends PartialType(CreateErpPriceIndexDto) {}

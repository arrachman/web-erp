import { PartialType } from '@nestjs/swagger';
import { CreateErpProductionRouteDto } from './create-production-routes.dto';

export class UpdateErpProductionRouteDto extends PartialType(CreateErpProductionRouteDto) {}

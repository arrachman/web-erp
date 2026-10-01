import { PartialType } from '@nestjs/swagger';
import { CreateErpHomeWidgetDto } from './create-erp-home-widget.dto';

export class UpdateErpHomeWidgetDto extends PartialType(CreateErpHomeWidgetDto) {}

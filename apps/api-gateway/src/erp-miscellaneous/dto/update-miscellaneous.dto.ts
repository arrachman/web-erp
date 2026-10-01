import { PartialType } from '@nestjs/swagger';
import { CreateErpMiscellaneousDto } from './create-miscellaneous.dto';

export class UpdateErpMiscellaneousDto extends PartialType(CreateErpMiscellaneousDto) {}

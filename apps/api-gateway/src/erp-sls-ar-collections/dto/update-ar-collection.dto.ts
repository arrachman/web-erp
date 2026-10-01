import { PartialType } from '@nestjs/swagger';
import { CreateArCollectionDto } from './create-ar-collection.dto';

export class UpdateArCollectionDto extends PartialType(CreateArCollectionDto) {}

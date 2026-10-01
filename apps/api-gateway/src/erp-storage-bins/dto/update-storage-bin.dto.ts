import { PartialType } from '@nestjs/swagger';
import { CreateErpStorageBinDto } from './create-storage-bin.dto';

export class UpdateErpStorageBinDto extends PartialType(CreateErpStorageBinDto) {}

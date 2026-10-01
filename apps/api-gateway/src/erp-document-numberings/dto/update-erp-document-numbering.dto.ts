import { PartialType } from '@nestjs/swagger';
import { CreateErpDocumentNumberingDto } from './create-erp-document-numbering.dto';

export class UpdateErpDocumentNumberingDto extends PartialType(CreateErpDocumentNumberingDto) {}

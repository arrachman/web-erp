import { PartialType } from '@nestjs/swagger';
import { CreateErpTransactionNoteDto } from './create-transaction-notes.dto';

export class UpdateErpTransactionNoteDto extends PartialType(CreateErpTransactionNoteDto) {}

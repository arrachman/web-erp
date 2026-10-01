import { PartialType } from '@nestjs/swagger';
import { CreateErpTxnNoteDetailDto } from './create-txn-note-detail.dto';

export class UpdateErpTxnNoteDetailDto extends PartialType(CreateErpTxnNoteDetailDto) {}

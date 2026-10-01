import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpTxnNoteDetailsController } from './txn-note-details.controller';
import { ErpTxnNoteDetailsService } from './txn-note-details.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpTxnNoteDetailsController],
  providers: [ErpTxnNoteDetailsService],
  exports: [ErpTxnNoteDetailsService],
})
export class ErpTxnNoteDetailsModule {}

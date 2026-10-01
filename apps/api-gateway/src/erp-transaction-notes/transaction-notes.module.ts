import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpTransactionNotesController } from './transaction-notes.controller';
import { ErpTransactionNotesService } from './transaction-notes.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpTransactionNotesController],
  providers: [ErpTransactionNotesService],
  exports: [ErpTransactionNotesService],
})
export class ErpTransactionNotesModule {}

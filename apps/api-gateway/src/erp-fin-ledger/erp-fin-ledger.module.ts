import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpFinLedgerController } from './erp-fin-ledger.controller';
import { ErpFinLedgerService } from './erp-fin-ledger.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpFinLedgerController],
  providers: [ErpFinLedgerService],
  exports: [ErpFinLedgerService],
})
export class ErpFinLedgerModule {}

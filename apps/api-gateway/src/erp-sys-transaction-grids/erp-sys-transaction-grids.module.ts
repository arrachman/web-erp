import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpSysTransactionGridsController } from './erp-sys-transaction-grids.controller';
import { ErpSysTransactionGridsService } from './erp-sys-transaction-grids.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSysTransactionGridsController],
  providers: [ErpSysTransactionGridsService],
  exports: [ErpSysTransactionGridsService],
})
export class ErpSysTransactionGridsModule {}

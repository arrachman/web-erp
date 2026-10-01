import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { InvStockCountPostingService } from './inv-stock-count-posting.service';
import { ErpInvStockCountsController } from './erp-inv-stock-counts.controller';
import { ErpInvStockCountsService } from './erp-inv-stock-counts.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpInvStockCountsController],
  providers: [ErpInvStockCountsService, InvStockCountPostingService],
  exports: [ErpInvStockCountsService],
})
export class ErpInvStockCountsModule {}

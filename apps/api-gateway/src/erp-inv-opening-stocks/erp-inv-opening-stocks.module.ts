import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { InvOpeningStockPostingService } from './inv-opening-stock-posting.service';
import { ErpInvOpeningStocksController } from './erp-inv-opening-stocks.controller';
import { ErpInvOpeningStocksService } from './erp-inv-opening-stocks.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpInvOpeningStocksController],
  providers: [ErpInvOpeningStocksService, InvOpeningStockPostingService],
  exports: [ErpInvOpeningStocksService],
})
export class ErpInvOpeningStocksModule {}

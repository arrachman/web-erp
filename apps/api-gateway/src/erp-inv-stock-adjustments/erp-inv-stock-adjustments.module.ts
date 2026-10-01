import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { InvStockAdjustmentPostingService } from './inv-stock-adjustment-posting.service';
import { ErpInvStockAdjustmentsController } from './erp-inv-stock-adjustments.controller';
import { ErpInvStockAdjustmentsService } from './erp-inv-stock-adjustments.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpInvStockAdjustmentsController],
  providers: [ErpInvStockAdjustmentsService, InvStockAdjustmentPostingService],
  exports: [ErpInvStockAdjustmentsService],
})
export class ErpInvStockAdjustmentsModule {}

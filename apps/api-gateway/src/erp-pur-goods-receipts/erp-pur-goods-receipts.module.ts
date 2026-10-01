import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { PurGoodsReceiptPostingService } from './pur-goods-receipt-posting.service';
import { ErpPurGoodsReceiptsController } from './erp-pur-goods-receipts.controller';
import { ErpPurGoodsReceiptsService } from './erp-pur-goods-receipts.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurGoodsReceiptsController],
  providers: [ErpPurGoodsReceiptsService, PurGoodsReceiptPostingService],
  exports: [ErpPurGoodsReceiptsService],
})
export class ErpPurGoodsReceiptsModule {}

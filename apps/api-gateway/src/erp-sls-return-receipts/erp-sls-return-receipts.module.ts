import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsReturnReceiptPostingService } from './sls-return-receipt-posting.service';
import { ErpSlsReturnReceiptsController } from './erp-sls-return-receipts.controller';
import { ErpSlsReturnReceiptsService } from './erp-sls-return-receipts.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsReturnReceiptsController],
  providers: [ErpSlsReturnReceiptsService, SlsReturnReceiptPostingService],
  exports: [ErpSlsReturnReceiptsService],
})
export class ErpSlsReturnReceiptsModule {}

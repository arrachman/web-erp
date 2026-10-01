import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ArReceiptPostingService } from './ar-receipt-posting.service';
import { ErpFinArReceiptsController } from './erp-fin-ar-receipts.controller';
import { ErpFinArReceiptsService } from './erp-fin-ar-receipts.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpFinArReceiptsController],
  providers: [ErpFinArReceiptsService, ArReceiptPostingService],
  exports: [ErpFinArReceiptsService],
})
export class ErpFinArReceiptsModule {}

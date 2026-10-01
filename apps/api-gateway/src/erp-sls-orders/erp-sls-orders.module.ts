import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsOrderPostingService } from './sls-order-posting.service';
import { ErpSlsOrdersController } from './erp-sls-orders.controller';
import { ErpSlsOrdersService } from './erp-sls-orders.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsOrdersController],
  providers: [ErpSlsOrdersService, SlsOrderPostingService],
  exports: [ErpSlsOrdersService],
})
export class ErpSlsOrdersModule {}

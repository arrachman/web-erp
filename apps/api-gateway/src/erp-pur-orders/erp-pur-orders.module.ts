import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { PurOrderPostingService } from './pur-order-posting.service';
import { ErpPurOrdersController } from './erp-pur-orders.controller';
import { ErpPurOrdersService } from './erp-pur-orders.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurOrdersController],
  providers: [ErpPurOrdersService, PurOrderPostingService],
  exports: [ErpPurOrdersService],
})
export class ErpPurOrdersModule {}

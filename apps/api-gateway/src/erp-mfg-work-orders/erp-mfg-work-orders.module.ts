import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpMfgWorkOrdersController } from './erp-mfg-work-orders.controller';
import { ErpMfgWorkOrdersService } from './erp-mfg-work-orders.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpMfgWorkOrdersController],
  providers: [ErpMfgWorkOrdersService],
  exports: [ErpMfgWorkOrdersService],
})
export class ErpMfgWorkOrdersModule {}

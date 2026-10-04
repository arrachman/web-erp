import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpInvLotsController } from './erp-inv-lots.controller';
import { ErpInvLotsService } from './erp-inv-lots.service';
import { InvLotAllocationService } from './inv-lot-allocation.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpInvLotsController],
  providers: [ErpInvLotsService, InvLotAllocationService],
  exports: [ErpInvLotsService, InvLotAllocationService],
})
export class ErpInvLotsModule {}

import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpInvGlModule } from '../erp-inv-gl/erp-inv-gl.module';
import { ErpInvPriceAdjustmentsController } from './erp-inv-price-adjustments.controller';
import { ErpInvPriceAdjustmentsService } from './erp-inv-price-adjustments.service';

@Module({
  imports: [PrismaModule, ErpInvGlModule],
  controllers: [ErpInvPriceAdjustmentsController],
  providers: [ErpInvPriceAdjustmentsService],
  exports: [ErpInvPriceAdjustmentsService],
})
export class ErpInvPriceAdjustmentsModule {}

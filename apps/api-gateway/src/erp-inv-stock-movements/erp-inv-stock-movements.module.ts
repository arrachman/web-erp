import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpInvGlModule } from '../erp-inv-gl/erp-inv-gl.module';
import { InvStockMovementPostingService } from './inv-stock-movement-posting.service';
import { ErpInvStockMovementsController } from './erp-inv-stock-movements.controller';
import { ErpInvStockMovementsService } from './erp-inv-stock-movements.service';

@Module({
  imports: [PrismaModule, ErpInvGlModule],
  controllers: [ErpInvStockMovementsController],
  providers: [ErpInvStockMovementsService, InvStockMovementPostingService],
  exports: [ErpInvStockMovementsService],
})
export class ErpInvStockMovementsModule {}

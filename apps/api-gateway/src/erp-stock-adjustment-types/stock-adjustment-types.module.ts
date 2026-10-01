import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpStockAdjustmentTypesController } from './stock-adjustment-types.controller';
import { ErpStockAdjustmentTypesService } from './stock-adjustment-types.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpStockAdjustmentTypesController],
  providers: [ErpStockAdjustmentTypesService],
  exports: [ErpStockAdjustmentTypesService],
})
export class ErpStockAdjustmentTypesModule {}

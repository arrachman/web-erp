import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpPriceIndicesController } from './erp-price-indices.controller';
import { ErpPriceIndicesService } from './erp-price-indices.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpPriceIndicesController],
  providers: [ErpPriceIndicesService],
  exports: [ErpPriceIndicesService],
})
export class ErpPriceIndicesModule {}

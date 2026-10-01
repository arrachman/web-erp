import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpPriceCategoriesController } from './price-categories.controller';
import { ErpPriceCategoriesService } from './price-categories.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpPriceCategoriesController],
  providers: [ErpPriceCategoriesService],
  exports: [ErpPriceCategoriesService],
})
export class ErpPriceCategoriesModule {}

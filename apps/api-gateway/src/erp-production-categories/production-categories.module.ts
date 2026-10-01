import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpProductionCategoriesController } from './production-categories.controller';
import { ErpProductionCategoriesService } from './production-categories.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpProductionCategoriesController],
  providers: [ErpProductionCategoriesService],
  exports: [ErpProductionCategoriesService],
})
export class ErpProductionCategoriesModule {}

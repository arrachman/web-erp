import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpPointCategoriesController } from './point-categories.controller';
import { ErpPointCategoriesService } from './point-categories.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpPointCategoriesController],
  providers: [ErpPointCategoriesService],
  exports: [ErpPointCategoriesService],
})
export class ErpPointCategoriesModule {}

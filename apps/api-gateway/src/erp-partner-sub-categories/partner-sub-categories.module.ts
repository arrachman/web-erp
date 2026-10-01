import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpPartnerSubCategoriesController } from './partner-sub-categories.controller';
import { ErpPartnerSubCategoriesService } from './partner-sub-categories.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpPartnerSubCategoriesController],
  providers: [ErpPartnerSubCategoriesService],
  exports: [ErpPartnerSubCategoriesService],
})
export class ErpPartnerSubCategoriesModule {}

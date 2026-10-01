import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPartnerCategoriesController } from './erp-partner-categories.controller';
import { ErpPartnerCategoriesService } from './erp-partner-categories.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPartnerCategoriesController],
  providers: [ErpPartnerCategoriesService],
  exports: [ErpPartnerCategoriesService],
})
export class ErpPartnerCategoriesModule {}

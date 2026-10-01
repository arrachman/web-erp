import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpItemCategoriesController } from './erp-item-categories.controller';
import { ErpItemCategoriesService } from './erp-item-categories.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpItemCategoriesController],
  providers: [ErpItemCategoriesService],
  exports: [ErpItemCategoriesService],
})
export class ErpItemCategoriesModule {}

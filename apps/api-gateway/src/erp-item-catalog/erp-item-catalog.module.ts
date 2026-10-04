import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpItemCatalogController } from './erp-item-catalog.controller';
import { ErpItemCatalogService } from './erp-item-catalog.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpItemCatalogController],
  providers: [ErpItemCatalogService],
  exports: [ErpItemCatalogService],
})
export class ErpItemCatalogModule {}

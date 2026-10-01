import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsPackingListPostingService } from './sls-packing-list-posting.service';
import { ErpSlsPackingListsController } from './erp-sls-packing-lists.controller';
import { ErpSlsPackingListsService } from './erp-sls-packing-lists.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsPackingListsController],
  providers: [ErpSlsPackingListsService, SlsPackingListPostingService],
  exports: [ErpSlsPackingListsService],
})
export class ErpSlsPackingListsModule {}

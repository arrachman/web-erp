import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { PurRequisitionPostingService } from './pur-requisition-posting.service';
import { ErpPurRequisitionsController } from './erp-pur-requisitions.controller';
import { ErpPurRequisitionsService } from './erp-pur-requisitions.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurRequisitionsController],
  providers: [ErpPurRequisitionsService, PurRequisitionPostingService],
  exports: [ErpPurRequisitionsService],
})
export class ErpPurRequisitionsModule {}

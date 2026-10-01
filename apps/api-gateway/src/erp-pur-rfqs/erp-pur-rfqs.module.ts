import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPurRfqsController } from './erp-pur-rfqs.controller';
import { ErpPurRfqsService } from './erp-pur-rfqs.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurRfqsController],
  providers: [ErpPurRfqsService],
  exports: [ErpPurRfqsService],
})
export class ErpPurRfqsModule {}
